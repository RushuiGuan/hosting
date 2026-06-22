using Albatross.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace Albatross.Hosting.ExceptionHandling {
	/// <summary>
	/// Fallback handler that converts an unhandled exception into an RFC 7807 <see cref="ProblemDetails"/>
	/// response. It is wired into ASP.NET Core's exception handler middleware and is meant as a safety net for
	/// unexpected failures only — expected error conditions should be returned explicitly from controller
	/// actions via <c>ActionResult</c>.
	/// </summary>
	public sealed class GlobalExceptionHandler {
		private readonly bool maskExceptionDetail;

		/// <summary>
		/// Serializer options used to write the <see cref="ProblemDetails"/> body: camelCase property names with
		/// null-valued properties omitted.
		/// </summary>
		public static readonly JsonSerializerOptions SerializerOptions = new() {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
		};

		/// <summary>
		/// Maps an exception to the HTTP status code used in the response. Exposed as a static method so the
		/// exception handler middleware's <c>SuppressDiagnosticsCallback</c> can reuse it, keeping the logging
		/// decision consistent with the response status (only 5xx is treated as a server error worth logging).
		/// </summary>
		public static int GetStatusCode(Exception exception) {
			return exception switch {
				NotFoundException => StatusCodes.Status404NotFound,
				ConflictException => StatusCodes.Status409Conflict,
				ValidationException => StatusCodes.Status422UnprocessableEntity,
				FluentValidation.ValidationException => StatusCodes.Status422UnprocessableEntity,
				System.ComponentModel.DataAnnotations.ValidationException => StatusCodes.Status422UnprocessableEntity,
				ArgumentException => StatusCodes.Status400BadRequest,
				NotSupportedException => StatusCodes.Status501NotImplemented,
				NotAuthenticatedException => StatusCodes.Status401Unauthorized,
				AuthenticationException => StatusCodes.Status401Unauthorized,
				ForbiddenException => StatusCodes.Status403Forbidden,
				AccessViolationException => StatusCodes.Status403Forbidden,
				UnauthorizedAccessException => StatusCodes.Status403Forbidden,
				PreconditionFailedException => StatusCodes.Status412PreconditionFailed,
				TimeoutException => StatusCodes.Status408RequestTimeout,
				_ => StatusCodes.Status500InternalServerError,
			};
		}

		/// <param name="maskExceptionDetail">
		/// When true, the detail of all error responses is set to null so internal information is not leaked
		/// to the caller. Enable for applications with high security requirements; leave off for internal
		/// applications where the detail aids diagnostics.
		/// </param>
		public GlobalExceptionHandler(bool maskExceptionDetail) {
			this.maskExceptionDetail = maskExceptionDetail;
		}

		/// <summary>
		/// Writes a <see cref="ProblemDetails"/> response for the exception captured in
		/// <see cref="IExceptionHandlerFeature"/>. Intended to be assigned as the <c>ExceptionHandler</c>
		/// delegate of <c>UseExceptionHandler</c>. Does nothing if there is no exception or the response has
		/// already started.
		/// </summary>
		public async Task Handle(HttpContext context) {
			var feature = context.Features.Get<IExceptionHandlerFeature>();
			var exception = feature?.Error;
			if (exception != null) {
				if (context.Response.HasStarted) {
					// the response has already been flushed; we can no longer change the status code or write a body
					return;
				}
				var status = GetStatusCode(exception);
				var problem = new ProblemDetails {
					Title = "An error occurred while processing your request",
					Status = status,
					Extensions = {
						["traceId"] = context.TraceIdentifier
					}
				};
				problem.Detail = maskExceptionDetail ? null : exception.Message;
				context.Response.StatusCode = status;
				context.Response.ContentType = MediaTypeNames.Application.ProblemJson;
				try {
					await JsonSerializer.SerializeAsync(context.Response.BodyWriter.AsStream(), problem, SerializerOptions);
				} catch {
					// Serialization failure — response headers already sent; nothing more can be done
				}
			}
		}
	}
}