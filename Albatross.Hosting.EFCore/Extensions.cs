using Albatross.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Hosting.EFCore {
	public static class Extensions {
		public static ActionResult<T> HandleSaveResult<T>(this SaveResults results, T data) {
			if (results.Success) {
				return data;
			} else {
				return HandleSaveResult(results);
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
		public static async Task<ActionResult> SaveAndReturn(this IRepository repository, Func<CancellationToken, Task> func, CancellationToken cancellationToken) {
			try {
				await func(cancellationToken);
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult);
			} catch (NotFoundException err) {
				return new NotFoundObjectResult(new ProblemDetails {
					Detail = err.Message,
				});
			} catch (Exception err) {
				return new ObjectResult(new ProblemDetails { Detail = err.Message }) {
					StatusCode = StatusCodes.Status500InternalServerError
				};
			}
		}
		public static async Task<ActionResult<T>> SaveAndReturn<T>(this IRepository repository, Func<T> func, CancellationToken cancellationToken) {
			try {
				var data = func();
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult, data);
			} catch (NotFoundException err) {
				return new NotFoundObjectResult(new ProblemDetails {
					Detail = err.Message,
				});
			} catch (Exception err) {
				return new ObjectResult(new ProblemDetails { Detail = err.Message }) {
					StatusCode = StatusCodes.Status500InternalServerError
				};
			}
		}
		public static async Task<ActionResult<T>> SaveAndReturn<T>(this IRepository repository, Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken) {
			try {
				var data = await func(cancellationToken);
				var saveResult = await repository.SaveChangesAsync(false, cancellationToken);
				return HandleSaveResult(saveResult, data);
			} catch (NotFoundException err) {
				return new NotFoundObjectResult(new ProblemDetails {
					Detail = err.Message,
				});
			} catch (Exception err) {
				return new ObjectResult(new ProblemDetails { Detail = err.Message }) {
					StatusCode = StatusCodes.Status500InternalServerError
				};
			}
		}
	}
}

