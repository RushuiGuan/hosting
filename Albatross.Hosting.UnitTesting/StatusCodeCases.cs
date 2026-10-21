using Albatross.Exceptions;
using System.Security.Authentication;
using Xunit;

namespace Albatross.Hosting.UnitTesting {
	// the documented exception to status code table; both handlers are tested against it so they cannot drift apart
	public static class StatusCodeCases {
		public static TheoryData<Exception, int> All => new() {
			{ new NotFoundException("Item 42 not found"), 404 },
			{ new ConflictException("Item 42 already exists"), 409 },
			{ new ValidationException("Name is missing"), 422 },
			{ new FieldValidationException(new ValidationError("name", "Name is missing")), 422 },
			{ new System.ComponentModel.DataAnnotations.ValidationException("Name is missing"), 422 },
			{ new ArgumentException("Name is invalid"), 400 },
			// a subclass of a mapped exception takes the mapping of its base
			{ new ArgumentNullException("name"), 400 },
			{ new NotSupportedException("Export is not supported"), 501 },
			{ new NotAuthenticatedException("Caller is not authenticated"), 401 },
			{ new AuthenticationException("Token has expired"), 401 },
			{ new ForbiddenException("Caller is forbidden"), 403 },
			{ new AccessViolationException("Access is denied"), 403 },
			{ new UnauthorizedAccessException("Access is denied"), 403 },
			{ new PreconditionFailedException("Item 42 has changed"), 412 },
			{ new TimeoutException("Query has timed out"), 408 },
			{ new InvalidOperationException("State is invalid"), 500 },
			{ new Exception("Something failed"), 500 },
		};
	}
}
