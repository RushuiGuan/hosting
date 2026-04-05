using Albatross.Input;
using FluentValidation;

namespace Sample.Core.Requests {
	public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>, ICached<CreateCompanyRequestValidator> {
		public CreateCompanyRequestValidator() {
			RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
		}
	}

	public record class CreateCompanyRequest : IRequest<CreateCompanyRequest> {
		public required string Name { get; init; }

		public CreateCompanyRequest Sanitize() {
			return this with { Name = Name.Trim() };
		}

		public static AbstractValidator<CreateCompanyRequest> Validator => ICached<CreateCompanyRequestValidator>.Instance;
	}
}
