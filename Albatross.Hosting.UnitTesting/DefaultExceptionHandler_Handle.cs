using Albatross.Exceptions;
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Albatross.Hosting.UnitTesting {
	public class DefaultExceptionHandler_Handle {
		static ValidationError[] Errors => [
			new("name", "Name is missing"),
			new("", "Request is inconsistent"),
		];

		static ObjectResult Handle(Exception exception, bool mask) {
			var handler = new DefaultExceptionHandler(mask, NullLogger.Instance);
			return Assert.IsAssignableFrom<ObjectResult>(handler.Handle(exception, null));
		}

		[Theory]
		[MemberData(nameof(StatusCodeCases.All), MemberType = typeof(StatusCodeCases))]
		public void ResultAndBodyCarryTheSameStatus(Exception exception, int expected) {
			var result = Handle(exception, false);
			Assert.Equal(expected, result.StatusCode);
			Assert.Equal(expected, Assert.IsAssignableFrom<ProblemDetails>(result.Value).Status);
		}

		public static TheoryData<Exception, bool, string?> Details => new() {
			{ new NotFoundException("Item 42 not found"), false, "Item 42 not found" },
			{ new NotFoundException("Item 42 not found"), true, null },
			{ new Exception("Connection string 'x' is invalid"), false, "Connection string 'x' is invalid" },
			{ new Exception("Connection string 'x' is invalid"), true, null },
		};

		[Theory]
		[MemberData(nameof(Details))]
		public void DetailMasking(Exception exception, bool mask, string? expected) {
			var problem = Assert.IsAssignableFrom<ProblemDetails>(Handle(exception, mask).Value);
			Assert.Equal(expected, problem.Detail);
		}

		public static TheoryData<Exception, bool> ValidationMessages => new() {
			{ new ValidationException("Cen code is missing"), false },
			{ new ValidationException("Cen code is missing"), true },
			{ new System.ComponentModel.DataAnnotations.ValidationException("Cen code is missing"), false },
			{ new System.ComponentModel.DataAnnotations.ValidationException("Cen code is missing"), true },
		};

		// a validation message describes the caller's own input, so masking does not hide it
		[Theory]
		[MemberData(nameof(ValidationMessages))]
		public void ValidationMessage_NotMasked(Exception exception, bool mask) {
			var problem = Assert.IsAssignableFrom<ProblemDetails>(Handle(exception, mask).Value);
			Assert.Equal("Cen code is missing", problem.Detail);
		}

		// field errors describe the caller's own input, so masking does not hide them
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ValidationErrors_ReturnedAsFieldErrors(bool mask) {
			var result = Handle(new FieldValidationException(Errors), mask);
			var problem = Assert.IsType<ValidationProblemDetails>(result.Value);
			Assert.Equal(422, result.StatusCode);
			Assert.Equal(DefaultExceptionHandler.ValidationFailedMessage, problem.Title);
			Assert.Equal(["Name is missing"], problem.Errors["name"]);
			Assert.Equal(["Request is inconsistent"], problem.Errors[""]);
			Assert.Equal(2, problem.Errors.Count);
		}

		[Fact]
		public void CustomHandlerResult_ReturnedAsIs() {
			var handler = new DefaultExceptionHandler(false, NullLogger.Instance);
			var custom = new StatusCodeResult(418);
			var result = handler.Handle(new NotFoundException("Item 42 not found"), (_, _) => custom);
			Assert.Same(custom, result);
		}

		[Fact]
		public void CustomHandlerNull_FallsThroughToDefault() {
			var handler = new DefaultExceptionHandler(false, NullLogger.Instance);
			var result = handler.Handle(new NotFoundException("Item 42 not found"), (_, _) => null);
			Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
		}
	}
}
