using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Albatross.Hosting.ExceptionHandling {
	public sealed class GlobalExceptionHandler {
		public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
		};

		public const int BadRequest = 400;
		public const int InternalServerError = 500;

		public async Task Handle(HttpContext context) {
			var feature = context.Features.Get<IExceptionHandlerFeature>();
			var exception = feature?.Error;
			if (exception != null) {
				var statusCode = exception is ArgumentException ? BadRequest : InternalServerError;
				var problem = new ProblemDetails {
					Status = statusCode,
					Title = "An error occurred while processing your request",
					Detail = $"{exception.GetType().FullName}: {exception.Message}",
					Extensions = {
						["traceId"] = context.TraceIdentifier
					}
				};
				context.Response.StatusCode = problem.Status.Value;
				context.Response.ContentType = MediaTypeNames.Application.ProblemJson;
				await JsonSerializer.SerializeAsync(context.Response.BodyWriter.AsStream(), problem, SerializerOptions);
			}
		}
	}
}