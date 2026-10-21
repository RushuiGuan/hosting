using Albatross.Exceptions;
using Albatross.Hosting.ExceptionHandling;
using Xunit;

namespace Albatross.Hosting.UnitTesting {
	public class ValidationExtensions_HasProblem {
		[Fact]
		public void NoErrors_NoProblem() {
			Assert.False(Array.Empty<ValidationError>().HasProblem(out var problem));
			Assert.Null(problem);
		}

		[Fact]
		public void Errors_GroupedByField() {
			ValidationError[] errors = [
				new("name", "Name is missing"),
				new("cenCode", "Cen code 'ABCDE' is invalid"),
				new("name", "Name 'x' is reserved"),
				new("", "Request is inconsistent"),
			];
			Assert.True(errors.HasProblem(out var problem));
			Assert.Equal(DefaultExceptionHandler.ValidationFailedMessage, problem.Title);
			Assert.Equal(422, problem.Status);
			Assert.Equal(["Name is missing", "Name 'x' is reserved"], problem.Errors["name"]);
			Assert.Equal(["Cen code 'ABCDE' is invalid"], problem.Errors["cenCode"]);
			Assert.Equal(["Request is inconsistent"], problem.Errors[""]);
			Assert.Equal(3, problem.Errors.Count);
		}

		// a validator written with yield return must run once, or checks that are not repeatable could disagree
		[Fact]
		public void LazyErrors_EnumeratedOnce() {
			var count = 0;
			IEnumerable<ValidationError> Validate() {
				count++;
				yield return new("name", "Name is missing");
			}
			Assert.True(Validate().HasProblem(out var problem));
			Assert.Equal(["Name is missing"], problem.Errors["name"]);
			Assert.Equal(1, count);
		}
	}
}
