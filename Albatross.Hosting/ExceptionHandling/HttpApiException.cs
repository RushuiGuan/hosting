using System;

namespace Albatross.Hosting.ExceptionHandling {
	/// <summary>
	/// Same as the deprecated HttpResponseException from Microsoft.AspNet.WebApi.Core.  This class allows us to throw an exception dynamically
	/// from a controller with a specific status code and error response.
	/// The downside of this approach is that a code generator will not know the type of the error response.  It would have to be annotated at
	/// the end point.
	/// </summary>
	public class HttpApiException : Exception {
		public HttpApiException(int statusCode, object? error): this(statusCode, "An error occurred while processing your request", error) {
			StatusCode = statusCode;
			Error = error;
		}
		public HttpApiException(int statusCode, string title, object? error) : base(title) {
			StatusCode = statusCode;
			Error = error;
		}
		public int StatusCode { get; }
		public object? Error { get; }
	}
}