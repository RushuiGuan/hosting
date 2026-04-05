using Albatross.Hosting;
using Albatross.Input;
using Microsoft.AspNetCore.Mvc;
using Sample.Core.Requests;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	public class ValidationExampleController : ControllerBase {

		[HttpPost]
		public ActionResult Post([FromBody] ValidationExampleRequest request) {
			if(request.Validate(out var sanitized).HasProblem(out var problem)) {
				return BadRequest(problem);
			} 
			// Do something with sanitized request
			return Ok();
		}
	}
}