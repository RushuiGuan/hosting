using Albatross.Exceptions;
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Albatross.Hosting {
	public static class ValidationExtensions {
		public static ValidationProblemDetails CreateProblemDetails(this IEnumerable<ValidationError> errors, string? title)
			=> new ValidationProblemDetails(errors.GroupBy(e => e.Field).ToDictionary(g => g.Key, g => g.Select(e => e.Error).ToArray())) {
				Title = title,
				Status = ValidationException.StatusCode,
			};

		// field errors describe the caller's own input, so they are never masked
		public static ProblemDetails CreateProblemDetails(this FieldValidationException exception, string? title)
			=> exception.Errors.CreateProblemDetails(title);

		public static bool HasProblem(this IEnumerable<ValidationError> errors, [NotNullWhen(true)] out ValidationProblemDetails? details) {
			var array = errors.ToArray();
			if (array.Length > 0) {
				details = array.CreateProblemDetails(DefaultExceptionHandler.ValidationFailedMessage);
				return true;
			} else {
				details = null;
				return false;
			}
		}
	}
}