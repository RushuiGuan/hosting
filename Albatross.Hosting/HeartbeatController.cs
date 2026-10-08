using Albatross.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Albatross.Hosting {
	public record class Heartbeat(string App, string Environment, DateTime Timestamp);

	[Route("api/heartbeat")]
	[ApiController]
	[AllowAnonymous]
	public class HeartbeatController : ControllerBase {
		private readonly ProgramSetting programSetting;
		private readonly EnvironmentSetting environmentSetting;

		public HeartbeatController(ProgramSetting programSetting, EnvironmentSetting environmentSetting) {
			this.programSetting = programSetting;
			this.environmentSetting = environmentSetting;
		}

		[HttpGet]
		public Heartbeat Get() => new Heartbeat(programSetting.App, environmentSetting.Value, DateTime.UtcNow);
	}
}
