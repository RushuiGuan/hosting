using Albatross.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Sample.WebApi.Controllers {
	[Route("api/login-windows")]
	[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	[ApiController]
	public class LoginByWindowsController : ControllerBase {
		private readonly IGetCurrentLogin getCurrentLogin;
		private readonly IGetCurrentUser getCurrentUser;

		public LoginByWindowsController(IGetCurrentLogin getCurrentLogin, IGetCurrentUser getCurrentUser) {
			this.getCurrentLogin = getCurrentLogin;
			this.getCurrentUser = getCurrentUser;
		}

		[HttpGet("user-claim")]
		public string[] GetUserClaims() => HttpContext.User?.Claims?.Select(args => $"{args.Type}: {args.Value}")?.ToArray() ?? Array.Empty<string>();

		[HttpGet("login")]
		public ILogin? GetCurrentUser() => getCurrentLogin.Get();
		
		[HttpGet("login-legacy")]
		public string GetCurrentUserLegacy() => getCurrentUser.Get();
	}
}
