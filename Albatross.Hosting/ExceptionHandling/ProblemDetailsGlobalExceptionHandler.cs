using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Albatross.Hosting.ExceptionHandling {
	public class ProblemDetailsGlobalExceptionHandler : DefaultGlobalExceptionHandler {
		public override object ConvertToObject(HttpContext context, int statusCode, Exception exception) {
			return new ProblemDetailsWithTraceId {
				Status = statusCode,
				Title = "An error occurred while processing your request",
				Detail = exception.Message,
				Type = exception.GetType().FullName,
				TraceId = context.TraceIdentifier,
			};
		}
	}
}
