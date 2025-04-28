using Microsoft.AspNetCore.Http;
using System;

namespace Albatross.Hosting.ExceptionHandling {
	[Obsolete]
	public class LegacyGobalExceptionHandler : DefaultGlobalExceptionHandler {
		public override object ConvertToObject(HttpContext context, int statusCode, Exception exception) {
			return new ErrorMessage(statusCode, exception);
		}
	}
}
