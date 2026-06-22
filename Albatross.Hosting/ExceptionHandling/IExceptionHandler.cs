using Albatross.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Albatross.Hosting.ExceptionHandling {
	public interface IExceptionHandler {
		bool MaskExceptionDetail { get; }
		ActionResult Handle(Exception exception, Func<Exception, bool, ActionResult?>? customHandler);
	}
	public class DefaultExceptionHandler : IExceptionHandler {
		private readonly ILogger logger;
		public DefaultExceptionHandler(bool maskExceptionDetail, ILogger logger) {
			MaskExceptionDetail = maskExceptionDetail;
			this.logger = logger;
		}
		public bool MaskExceptionDetail { get; }
		public ActionResult Handle(Exception err, Func<Exception, bool, ActionResult?>? customHandler) {
			var result = customHandler?.Invoke(err, MaskExceptionDetail);
			if (result != null) { return result; }
			var detail = MaskExceptionDetail ? null : err.Message;
			switch (err) {
				case NotFoundException:
					logger.LogWarning(err, "Not found");
					return new NotFoundObjectResult(new ProblemDetails { Detail = detail });
				case ConflictException:
					logger.LogWarning(err, "Conflict");
					return new ConflictObjectResult(new ProblemDetails { Detail = detail });
				case ValidationException:
					logger.LogWarning(err, "Validation failed");
					return new UnprocessableEntityObjectResult(new ProblemDetails { Detail = detail });
				case ArgumentException:
					logger.LogWarning(err, "Bad request");
					return new BadRequestObjectResult(new ProblemDetails { Detail = detail });
				case NotAuthenticatedException:
					logger.LogWarning(err, "Not authenticated");
					return new UnauthorizedObjectResult(new ProblemDetails { Detail = detail });
				case ForbiddenException:
					logger.LogWarning(err, "Forbidden");
					return new ForbidResult();
				case PreconditionFailedException:
					logger.LogWarning(err, "Precondition failed");
					return new ObjectResult(new ProblemDetails { Detail = detail }) {
						StatusCode = StatusCodes.Status412PreconditionFailed
					};
				case NotSupportedException:
					logger.LogWarning(err, "Not supported");
					return new ObjectResult(new ProblemDetails { Detail = detail }) {
						StatusCode = StatusCodes.Status501NotImplemented
					};
				case TimeoutException:
					logger.LogWarning(err, "Request timeout");
					return new ObjectResult(new ProblemDetails { Detail = detail }) {
						StatusCode = StatusCodes.Status408RequestTimeout
					};
				default:
					logger.LogError(err, "Unhandled exception");
					return new ObjectResult(new ProblemDetails { Detail = detail }) {
						StatusCode = StatusCodes.Status500InternalServerError
					};
			}
		}
	}
}