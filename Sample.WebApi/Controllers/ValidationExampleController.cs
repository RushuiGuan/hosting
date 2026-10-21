using Albatross.Exceptions;
using Albatross.Hosting;
using Microsoft.AspNetCore.Mvc;
using Sample.Core.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class ValidationExampleController : ControllerBase {
		[HttpPost]
		public ActionResult Post([FromBody] ValidationExampleRequest request) {
			if (request.Validate().HasProblem(out var problem)) {
				return UnprocessableEntity(problem);
			}
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

		public static IEnumerable<ValidationError> Validate(string? name) {
			if (string.IsNullOrEmpty(name)) {
				yield return new ValidationError(nameof(Value), "required");
			}
		}

		public static bool TryParse([NotNullWhen(true)] string? value, IFormatProvider? provider, out EntityName entityName) {
			if (!Validate(value).Any()) {
				entityName = new EntityName(value!.ToLowerInvariant());
				return true;
			} else {
				entityName = default;
				return false;
			}
		}

		public static EntityName Parse(string value, IFormatProvider? provider) {
			var errors = Validate(value).ToArray();
			if (errors.Length == 0) {
				return new EntityName(value!.ToLowerInvariant());
			} else {
				throw new FieldValidationException(errors);
			}
		}

		public override string ToString() => Value ?? string.Empty;
	}
}