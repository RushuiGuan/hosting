using Albatross.Hosting.ExceptionHandling;
using Xunit;

namespace Albatross.Hosting.UnitTesting {
	public class GlobalExceptionHandler_GetStatusCode {
		[Theory]
		[MemberData(nameof(StatusCodeCases.All), MemberType = typeof(StatusCodeCases))]
		public void ExceptionToStatusCode(Exception exception, int expected) {
			Assert.Equal(expected, GlobalExceptionHandler.GetStatusCode(exception));
		}
	}
}
