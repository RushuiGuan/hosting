using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class ExceptionTestCaseController : ControllerBase {
		private readonly ILogger<ExceptionTestCaseController> logger;

		public ExceptionTestCaseController(ILogger<ExceptionTestCaseController> logger) {
			this.logger = logger;
		}

		[HttpGet("send-via-controllerBase.problem-method")]
		public ActionResult UseProblem() {
			logger.LogInformation("Return an object result using the ControllerBase.Problem method");
			return Problem(detail: "error detail", instance: "1", title: "test", statusCode: StatusCodes.Status500InternalServerError, type: "ExceptionType");
		}

		[HttpGet("send-by-throwing-exception")]
		public void ThrowException() {
			// by default, this will result in a response with status code 500 and json content of type Albatross.Hosting.ErrorMessage
			throw new Exception("This is a test exception");
		}

		[HttpGet("send-by-throwing-text-http-api-exception")]
		public void ThrowTextHttpApiException() {
			throw new HttpApiException(400, "This is a test exception");
		}

		[HttpGet("send-by-throwing-json-http-api-exception")]
		public void ThrowJsonHttpApiException() {
			throw new HttpApiException(400, new { id = 100, message = "This is a test exception" });
		}

		[HttpGet("send-by-throwing-argument-exception")]
		public void ThrowArgumentException() {
			throw new ArgumentException("This is a test exception");
		}

		[HttpGet("send-by-throwing-exception-with-inner-exception")]
		public void ThrowExceptionWithInnerException() {
			throw new InvalidOperationException("This is a test exception", new InvalidOperationException("This is an inner exception"));
		}

		[HttpGet("throw-after-async-enumerable")]
		public async IAsyncEnumerable<int> ThrowAfterAsyncEnumerable() {
			await Task.Delay(1);
			for (int i = 0; i < 100; i++) {
				yield return i;
			}
			throw new ArgumentException("this is a test exception");
		}

		[HttpGet("throw-after-yield-return")]
		public IEnumerable<int> ThrowAfterYieldReturn() {
			for (int i = 0; i < 100; i++) {
				yield return i;
			}
			throw new ArgumentException("this is a test exception");
		}
	}
}
