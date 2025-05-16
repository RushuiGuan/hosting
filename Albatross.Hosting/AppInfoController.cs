using Albatross.Authentication;
using Albatross.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Albatross.Hosting {
	[Route("api/app-info")]
	[ApiController]
	public class AppInfoController : ControllerBase {
		private readonly ProgramSetting programSetting;
		private readonly IGetCurrentLogin getCurrentLogin;
		private readonly EnvironmentSetting environmentSetting;

		public AppInfoController(ProgramSetting programSetting, IGetCurrentLogin getCurrentLogin, EnvironmentSetting environmentSetting) {
			this.programSetting = programSetting;
			this.getCurrentLogin = getCurrentLogin;
			this.environmentSetting = environmentSetting;
		}

		[HttpGet]
		public ProgramSetting Get() => programSetting;

		[HttpGet("env")]
		public EnvironmentSetting GetEnvironment() => environmentSetting;
	}
}