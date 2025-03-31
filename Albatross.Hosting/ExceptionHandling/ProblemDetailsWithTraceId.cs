using Microsoft.AspNetCore.Mvc;

namespace Albatross.Hosting.ExceptionHandling {
	public class ProblemDetailsWithTraceId : ProblemDetails{
		public string? TraceId { get; set; }
	}
}
