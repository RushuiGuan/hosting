using Microsoft.AspNetCore.Http;
using System;

namespace Albatross.Hosting.ExceptionHandling {
	public class DefaultGlobalExceptionHandler : IGlobalExceptionHandler {
		public const int BadRequest = 400;
		public const int InternalServerError = 500;

		public virtual HttpApiException Convert(HttpContext context, Exception exception) {
			if (exception is HttpApiException httpApiException) {
				return httpApiException;
			} else {
				var statusCode = exception is ArgumentException ? BadRequest : InternalServerError;
				var obj = ConvertToObject(context, statusCode, exception);
				return new HttpApiException(statusCode, obj);
			}
		}

		public virtual object ConvertToObject(HttpContext context, int statusCode, Exception exception) {
			return new ErrorMessage(statusCode, exception);
		}
	}
}
