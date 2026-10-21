using Albatross.Exceptions;

namespace Sample.Core.Requests {
	public static partial class Validators {
		public static IEnumerable<ValidationError> Validate(this ValidationExampleRequest request) {
			if (string.IsNullOrEmpty(request.Name)) {
				yield return new ValidationError(nameof(ValidationExampleRequest.Name), "required");
			}
		}
	}

	public record class ValidationExampleRequest {
		public string? Name { get; init; }
	}
}
