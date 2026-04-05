using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Albatross.Hosting {
	public static class ValidationExtensions {
		public static bool HasProblem(this ValidationResult result, [NotNullWhen(true)] out ProblemDetails? details) {
			if (!result.IsValid) {
				details = new ValidationProblemDetails(
					result.Errors.GroupBy(e => e.PropertyName).ToDictionary(
						g => g.Key,
						g => g.Select(e => e.ErrorMessage).ToArray()
					)
				);
				return true;
			} else {
				details = null;
				return false;
			}
		}
	}
}