using Albatross.EFCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Albatross.Hosting.ExceptionHandling {
	public sealed class GlobalExceptionHandler {
		public static readonly JsonSerializerOptions SerializerOptions = new() {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
		};

		public async Task Handle(HttpContext context) {
			var feature = context.Features.Get<IExceptionHandlerFeature>();
			var exception = feature?.Error;
			if (exception != null) {
				var problem = new ProblemDetails {
					Title = "An error occurred while processing your request",
					Extensions = {
						["traceId"] = context.TraceIdentifier
					}
				};
				if (exception is NotFoundException) {
					problem.Detail = exception.Message;
					problem.Status = StatusCodes.Status404NotFound;
				} else if (exception is ArgumentException argEx) {
					problem.Detail = argEx.Message;
					problem.Status = StatusCodes.Status400BadRequest;
				} else {
					problem.Detail = $"{exception.GetType().Namespace}: {exception.Message}";
					problem.Status = StatusCodes.Status500InternalServerError;
				}
				context.Response.StatusCode = problem.Status.Value;
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