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
		public const string NotFoundMessage = "Not found";
		public const string ConflictMessage = "Conflict";
		public const string ValidationFailedMessage = "Validation failed";
		public const string BadRequestMessage = "Bad request";
		public const string NotAuthenticatedMessage = "Not authenticated";
		public const string ForbiddenMessage = "Forbidden";
		public const string PreconditionFailedMessage = "Precondition failed";
		public const string NotSupportedMessage = "Not supported";
		public const string RequestTimeoutMessage = "Request timeout";
		public const string UnhandledExceptionMessage = "Unhandled exception";

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
					logger.LogWarning(err, NotFoundMessage);
					return new NotFoundObjectResult(new ProblemDetails {
						Title = NotFoundMessage,
						Status = StatusCodes.Status404NotFound,
						Detail = detail
					});
				case ConflictException:
					logger.LogWarning(err, ConflictMessage);
					return new ConflictObjectResult(new ProblemDetails {
						Title = ConflictMessage,
						Status = StatusCodes.Status409Conflict,
						Detail = detail
					});
				case ValidationException:
					logger.LogWarning(err, ValidationFailedMessage);
					return new UnprocessableEntityObjectResult(new ProblemDetails { Title = ValidationFailedMessage, Status = StatusCodes.Status422UnprocessableEntity, Detail = detail });
				case ArgumentException:
					logger.LogWarning(err, BadRequestMessage);
					return new BadRequestObjectResult(new ProblemDetails { Title = BadRequestMessage, Status = StatusCodes.Status400BadRequest, Detail = detail });
				case NotAuthenticatedException:
					logger.LogWarning(err, NotAuthenticatedMessage);
					return new UnauthorizedObjectResult(new ProblemDetails { Title = NotAuthenticatedMessage, Status = StatusCodes.Status401Unauthorized, Detail = detail });
				case ForbiddenException:
					logger.LogWarning(err, ForbiddenMessage);
					return new ObjectResult(new ProblemDetails { Title = ForbiddenMessage, Status = StatusCodes.Status403Forbidden, Detail = detail }) {
						StatusCode = StatusCodes.Status403Forbidden
					};
				case PreconditionFailedException:
					logger.LogWarning(err, PreconditionFailedMessage);
					return new ObjectResult(new ProblemDetails { Title = PreconditionFailedMessage, Status = StatusCodes.Status412PreconditionFailed, Detail = detail }) {
						StatusCode = StatusCodes.Status412PreconditionFailed
					};
				case NotSupportedException:
					logger.LogWarning(err, NotSupportedMessage);
					return new ObjectResult(new ProblemDetails { Title = NotSupportedMessage, Status = StatusCodes.Status501NotImplemented, Detail = detail }) {
						StatusCode = StatusCodes.Status501NotImplemented
					};
				case TimeoutException:
					logger.LogWarning(err, RequestTimeoutMessage);
					return new ObjectResult(new ProblemDetails { Title = RequestTimeoutMessage, Status = StatusCodes.Status408RequestTimeout, Detail = detail }) {
						StatusCode = StatusCodes.Status408RequestTimeout
					};
				default:
					logger.LogError(err, UnhandledExceptionMessage);
					return new ObjectResult(new ProblemDetails { Title = UnhandledExceptionMessage, Status = StatusCodes.Status500InternalServerError, Detail = detail }) {
						StatusCode = StatusCodes.Status500InternalServerError
					};
			}
		}
	}
}