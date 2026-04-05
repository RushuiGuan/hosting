using Albatross.Hosting;
using Albatross.Input;
using Microsoft.AspNetCore.Mvc;
using Sample.Core.Requests;
using System;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class ValidationExampleController : ControllerBase {

		[HttpPost]
		public ActionResult Post([FromBody] ValidationExampleRequest request) {
			if(request.Validate(out var sanitized).HasProblem(out var problem)) {
				return BadRequest(problem);
			} 
			// Do something with sanitized request
			return Ok();
		}


		[HttpGet("{resource}")]
		public void Get([FromRoute] ResourceName resource) {
		}
	}

	public struct ResourceName: IParsable<ResourceName>
		public string Value { get; }
		public ResourceName(string value) {
			this.Value = value;
		}
		public static ResourceName Parse(string s, IFormatProvider provider) {
			if (string.IsNullOrEmpty(s)) {
				throw new FormatException("Resource name cannot be empty");
			}
			return new ResourceName(s);
		}

		public static bool TryParse(string s, IFormatProvider provider, out ResourceName result) {
			if (string.IsNullOrEmpty(s)) {
				result = default;
				return false;
			}
			result = new ResourceName(s);
			return true; {
	}
}