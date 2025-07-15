using Albatross.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Sample.WebApi.Controllers {
	[Route("api/login")]
	[ApiController]
	public class LoginController : ControllerBase {
		private readonly IGetCurrentLogin getCurrentLogin;
		private readonly IGetCurrentUser getCurrentUser;

		public LoginController(IGetCurrentLogin getCurrentLogin, IGetCurrentUser getCurrentUser) {
			this.getCurrentLogin = getCurrentLogin;
			this.getCurrentUser = getCurrentUser;
		}

		[HttpGet("user-claim")]
		// [Authorize(AuthenticationSchemes ="Google")]
		[Authorize]
		public string[] GetUserClaims() => HttpContext.User?.Claims?.Select(args => $"{args.Type}: {args.Value}")?.ToArray() ?? new string[0];

		[HttpGet("login")]
		// [Authorize(AuthenticationSchemes ="Google")]
		[Authorize]
		public ILogin? GetCurrentLogin() => getCurrentLogin.Get();

		[HttpGet("current-user")]
		// [Authorize(AuthenticationSchemes ="Google")]
		[Authorize]
		public string GetCurrentUser() => getCurrentUser.Get();
	}
}
