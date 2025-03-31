using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Albatross.Hosting.ExceptionHandling {
	public interface IGlobalExceptionHandler {
		HttpApiException Convert(HttpContext context, Exception exception);
		public async Task Handle(HttpContext context) {
			var feature = context.Features.Get<IExceptionHandlerFeature>();
			var error = feature?.Error;
			if (error != null) {
				var exception = Convert(context, error);
				context.Response.StatusCode = exception.StatusCode;
				if (exception.Error is string text) {
					context.Response.ContentType = MediaTypeNames.Text.Plain;
					await context.Response.WriteAsync(text);
				} else {
					context.Response.ContentType = MediaTypeNames.Application.Json;
					await JsonSerializer.SerializeAsync(context.Response.BodyWriter.AsStream(), exception.Error, ExceptionHandlerSerializationOptions.Value);
				}
			}
		}
	}
}
