using Albatross.Exceptions;

namespace Sample.Core.Requests {
	public static partial class Validators {
		public static IEnumerable<ValidationError> Validate(this CreateCompanyRequest request) {
			if (string.IsNullOrEmpty(request.Name)) {
				yield return new ValidationError(nameof(CreateCompanyRequest.Name), "required");
			} else if (request.Name.Length > 256) {
				yield return new ValidationError(nameof(CreateCompanyRequest.Name), "name should be less than 256 characters");
			}
		}
	}

	public record class CreateCompanyRequest {
		public required string Name { get; init; }
	}
}
