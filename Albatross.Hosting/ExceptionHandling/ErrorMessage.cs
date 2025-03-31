using System;

namespace Albatross.Hosting.ExceptionHandling {
	/// <summary>
	/// similar to ProblemDetails, but with a different structure.  This class is used as the default structure for error responses due to
	/// legacy support.
	/// </summary>
	public class ErrorMessage {
		public string? Message { get; set; }
		public string? Type { get; set; }
		public int StatusCode { get; set; }
		public ErrorMessage? InnerError { get; set; }

		public ErrorMessage() { }
		public ErrorMessage(int statusCode, Exception exception) { 
			StatusCode = statusCode;
			Type = exception.GetType().FullName;
			Message = exception.Message;
			InnerError = exception.InnerException == null ? null : new ErrorMessage(statusCode, exception.InnerException);
		}
	}
}