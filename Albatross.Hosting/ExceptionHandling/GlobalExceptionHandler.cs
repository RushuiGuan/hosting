using Albatross.EFCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
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
		/// <returns>
		/// 404 for <see cref="NotFoundException"/>; 400 for argument/validation exceptions; 401 for
		/// <see cref="AuthenticationException"/>; 403 for <see cref="UnauthorizedAccessException"/>; 409 for
		/// <see cref="ConflictException"/>; otherwise 500.
		/// </returns>
		public static int GetStatusCode(Exception exception) {
			if (exception is NotFoundException) {
				return StatusCodes.Status404NotFound;
			} else if (exception is ArgumentException || exception is ValidationException || exception is FluentValidation.ValidationException) {
				return StatusCodes.Status400BadRequest;
			} else if (exception is AuthenticationException) {
				// http 401 is actually unauthenticated.  status code description is poorly named
				return StatusCodes.Status401Unauthorized;
			} else if (exception is UnauthorizedAccessException) {
				// http status naming is incorrect here.
				// UnauthorizedAccessException means user does not have enough permission to access
				// therefore mapping to 403 forbidden not 401 unauthorized
				return StatusCodes.Status403Forbidden;
			} else if (exception is ConflictException) {
				return StatusCodes.Status409Conflict;
			} else {
				return StatusCodes.Status500InternalServerError;
			}
		}

		/// <param name="maskExceptionDetail">
		/// When true, the detail of 500 responses is replaced with a generic message so internal information
		/// (sql, file paths, connection strings, etc.) is not leaked to the caller. Messages of known 4xx
		/// exceptions are always returned regardless of this setting. Enable for applications with high security
		/// requirements; leave off for internal applications where the detail aids diagnostics.
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
				if (status == StatusCodes.Status500InternalServerError) {
					// unexpected server error: the message can carry internals (sql, file paths, connection
					// strings, etc.), so mask it when configured.  the full exception is always written to the
					// server log, and the caller can quote the trace id to correlate.
					problem.Detail = maskExceptionDetail
						? "An unexpected error occurred.  Please provide the trace id to support for assistance."
						: $"{exception.GetType().FullName}: {exception.Message}";
				} else {
					// known/expected exceptions (validation, not found, conflict, etc.) carry intentional,
					// client-safe messages that are part of the api contract, so always return them
					problem.Detail = exception.Message;
				}
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