using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Albatross.Hosting {
	public class ArgumentExceptionFilter : IExceptionFilter {
		public void OnException(ExceptionContext context) {
			if (context.Exception is ArgumentException error) {
				context.Result = new BadRequestObjectResult(new { error = error.Message });
				context.ExceptionHandled = true;
			}
		}
	}
}