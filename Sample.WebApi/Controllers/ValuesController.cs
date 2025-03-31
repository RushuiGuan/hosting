using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase {
		private readonly ILogger logger;

		public ValuesController(ILogger logger) {
			this.logger = logger;
		}

		[HttpGet]
		public string Get() {
			return DateTime.UtcNow.ToString();
		}

		[HttpGet("datetime")]
		public void Get([FromQuery] DateTime datetime) {
			logger.LogInformation("kind: {kind}", datetime.Kind);
			logger.LogInformation("value: {value:yyyy-MM-ddTHH:mm:sszzz}", datetime);
		}

		[HttpGet("datetimeoffset")]
		public void Get([FromQuery] DateTimeOffset datetime) {
			logger.LogInformation("value: {value:yyyy-MM-ddTHH:mm:sszzz}", datetime);
		}

		[HttpGet("datetime-from-route/{datetime1}/{id}/{datetime2}")]
		public void Get([FromRoute] DateTime datetime1, [FromRoute] int id, [FromRoute] DateTime datetime2) {
			logger.LogInformation("value: {value:yyyy-MM-ddTHH:mm:sszzz}", datetime1);
			logger.LogInformation("value: {value:yyyy-MM-ddTHH:mm:sszzz}", datetime2);
		}

		[HttpGet("nullable-route/{id1}/{id2}/{id3}")]
		public string Get(int? id1, int? ID2, int id3) {
			logger.LogInformation("{id1}, {id2}, {id3}", id1, ID2, id3);
			return $"{id1}.{ID2}.{id3}";
		}
	}
}