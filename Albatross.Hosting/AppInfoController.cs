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
		private readonly IGetCurrentUser getCurrentUser;
		private readonly EnvironmentSetting environmentSetting;

		public AppInfoController(ProgramSetting programSetting, IGetCurrentUser getCurrentUser, EnvironmentSetting environmentSetting) {
			this.programSetting = programSetting;
			this.getCurrentUser = getCurrentUser;
			this.environmentSetting = environmentSetting;
		}

		[HttpGet]
		public ProgramSetting Get() => programSetting;

		[HttpGet("env")]
		public EnvironmentSetting GetEnvironment() => environmentSetting;

		[Authorize]
		[HttpGet("user-claim")]
		public string[] GetUserClaims() => HttpContext.User?.Claims?.Select(args => $"{args.Type}: {args.Value}")?.ToArray() ?? new string[0];

		[Authorize]
		[HttpGet("user")]
		public string GetCurrentUser() => getCurrentUser.Get();
	}
}