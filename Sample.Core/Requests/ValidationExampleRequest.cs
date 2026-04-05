using Albatross.Input;
using FluentValidation;

namespace Sample.Core.Requests {
	public class VadidationExampleRequestValidator : AbstractValidator<ValidationExampleRequest>, ICached<VadidationExampleRequestValidator> {
		public VadidationExampleRequestValidator() {
			RuleFor(x => x).NotNull();
		}
	}

	public record class ValidationExampleRequest : IRequest<ValidationExampleRequest> {
		public ValidationExampleRequest Sanitize() {
			return this with { };
		}
		public static AbstractValidator<ValidationExampleRequest> Validator => ICached<VadidationExampleRequestValidator>.Instance;
	}
}