using Albatross.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Hosting.EFCore {
	public static class Extensions {
		public static ActionResult<T> HandleSaveResult<T>(this SaveResults results, T data)
			=> results.Success ? data : HandleSaveResult(results);
		public static ActionResult HandleException(this Exception err, Func<Exception, ActionResult?>? customHandler) {
			var result = customHandler?.Invoke(err);
			if (result != null) { return result; }
			if (err is NotFoundException) {
				return new NotFoundObjectResult(new ProblemDetails {
					Detail = err.Message,
				});
			} else {
				return new ObjectResult(new ProblemDetails { Detail = err.Message }) {
					StatusCode = StatusCodes.Status500InternalServerError
				};
			}
		}
		public static ActionResult HandleSaveResult(this SaveResults results) {
			if (results.Success) {
				return new NoContentResult();
			} else if (results.NameConflict) {
				return new ConflictObjectResult(new ProblemDetails {
					Detail = results.Error.Message
				});
			} else if (results.ForeignKeyConflict) {
				return new UnprocessableEntityObjectResult(new ProblemDetails {
					Detail = results.Error.Message
				});
			} else {
				return new ObjectResult(new ProblemDetails { Detail = results.Error.Message }) {
					StatusCode = StatusCodes.Status500InternalServerError
				};
			}
		}
		public static async Task<ActionResult> SaveAndReturn(this IRepository repository, CancellationToken cancellationToken, Func<Exception, ActionResult?>? customErrorHandler = null) {
			try {
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult);
			} catch (Exception err) {
				return err.HandleException(customErrorHandler);
			}
		}
		public static async Task<ActionResult> SaveAndReturn(this IRepository repository, Action action, CancellationToken cancellationToken, Func<Exception, ActionResult?>? customErrorHandler = null) {
			try {
				action();
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult);
			} catch (Exception err) {
				return err.HandleException(customErrorHandler);
			}
		}
		public static async Task<ActionResult> SaveAndReturn(this IRepository repository, Func<CancellationToken, Task> func, CancellationToken cancellationToken, Func<Exception, ActionResult?>? customErrorHandler = null) {
			try {
				await func(cancellationToken);
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult);
			} catch (Exception err) {
				return err.HandleException(customErrorHandler);
			}
		}
		public static async Task<ActionResult<T>> SaveAndReturn<T>(this IRepository repository, Func<T> func, CancellationToken cancellationToken, Func<Exception, ActionResult?>? customErrorHandler = null) {
			try {
				var data = func();
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult, data);
			} catch (Exception err) {
				return err.HandleException(customErrorHandler);
			}
		}
		public static async Task<ActionResult<T>> SaveAndReturn<T>(this IRepository repository, Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken, Func<Exception, ActionResult?>? customErrorHandler = null) {
			try {
				var data = await func(cancellationToken);
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult, data);
			} catch (Exception err) {
				return err.HandleException(customErrorHandler);
			}
		}
	}
}