using Albatross.Authentication;
using Albatross.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Albatross.Hosting {
	[Route("api/app-info")]
	[ApiController]
	[Authorize]
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

		[HttpGet("versions")]
		public string Versions() {
			var sb = new StringBuilder();
			var assembly = Assembly.GetEntryAssembly();
			if (assembly != null) {
				sb.AppendLine(assembly.FullName);
				foreach (var refAssembly in assembly.GetReferencedAssemblies()) {
					sb.AppendLine(refAssembly.FullName);
				}
			}
			return sb.ToString();
		}

		[HttpGet("env")]
		public EnvironmentSetting GetEnvironment() => environmentSetting;

		[HttpGet("user-claim")]
		public string[] GetUserClaims() => HttpContext.User?.Claims?.Select(args => $"{args.Type}: {args.Value}")?.ToArray() ?? new string[0];

		[HttpGet("user")]
		public string GetCurrentUser() => getCurrentUser.Get();
	}
}