using Albatross.Hosting;
using Albatross.Input;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sample.Core.Requests;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class ValidationExampleController : ControllerBase {

		[HttpPost]
		public ActionResult Post([FromBody] ValidationExampleRequest request) {
			if(request.Validate().HasProblem(out var problem)) {
				return BadRequest(problem);
			} 
			// Do something with sanitized request
			return Ok();
		}


		[HttpGet("{resource}")]
		public void Get([FromRoute] EntityName resource) {
		}
	}

	public readonly record struct EntityName : IParsable<EntityName> {
		public string Value { get; }

		private EntityName(string value) {
			Value = value;
		}

		public static bool TryParse([NotNullWhen(true)] string? value, IFormatProvider? provider, out EntityName entityName) {
			var result = RequiredEntityNameValidator.Instance.Validate(value);
			if (result.IsValid) {
				entityName = new EntityName(value!.ToLowerInvariant());
				return true;
			} else {
				entityName = default;
				return false;
			}
		}

		public static EntityName Parse(string value, IFormatProvider? provider) {
			var result = RequiredEntityNameValidator.Instance.Validate(value);
			if (result.IsValid) {
				return new EntityName(value!.ToLowerInvariant());
			} else {
				throw new ValidationException(string.Join('\n', result.Errors.Select(x => x.ErrorMessage)));
			}
		}

		public override string ToString() => Value ?? string.Empty;
	}
	public class RequiredEntityNameValidator : AbstractValidator<string?> {
		public RequiredEntityNameValidator() {
			RuleFor(x => x)
				.NotEmpty().WithMessage("'{PropertyPath}' is missing")
				.Length(2, 10)
				.WithMessage("'{PropertyPath}' must be between {MinLength} and {MaxLength} characters");
		}
		public static readonly RequiredEntityNameValidator Instance = new();
	}
}