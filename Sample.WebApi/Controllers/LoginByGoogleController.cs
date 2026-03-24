using Albatross.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Sample.WebApi.Controllers {
	[Route("api/login")]
	[Authorize(AuthenticationSchemes ="Google")]
	[ApiController]
	public class LoginByGoogleController : ControllerBase {
		private readonly IGetCurrentLogin getCurrentLogin;
		private readonly IGetCurrentUser getCurrentUser;

		public LoginByGoogleController(IGetCurrentLogin getCurrentLogin, IGetCurrentUser getCurrentUser) {
			this.getCurrentLogin = getCurrentLogin;
			this.getCurrentUser = getCurrentUser;
		}

		[HttpGet("user-claim")]
		public string[] GetUserClaims() => HttpContext.User?.Claims?.Select(args => $"{args.Type}: {args.Value}")?.ToArray() ?? Array.Empty<string>();

		[HttpGet("login")]
		public ILogin? GetCurrentLogin() => getCurrentLogin.Get();

		[HttpGet("current-user")]
		public string GetCurrentUser() => getCurrentUser.Get();
	}
}
