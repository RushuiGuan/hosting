using Albatross.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Sample.WebApi.Controllers {
	[Route("api/login-google")]
	[ApiController]
	public class LoginByGoogleController : ControllerBase {
		private readonly IGetCurrentLogin getCurrentLogin;

		public LoginByGoogleController(IGetCurrentLogin getCurrentLogin) {
			this.getCurrentLogin = getCurrentLogin;
		}

		[HttpGet("user-claim")]
		[Authorize(AuthenticationSchemes ="Google")]
		public string[] GetUserClaims() => HttpContext.User?.Claims?.Select(args => $"{args.Type}: {args.Value}")?.ToArray() ?? new string[0];

		[HttpGet("login")]
		[Authorize(AuthenticationSchemes ="Google")]
		public ILogin? GetCurrentUser() => getCurrentLogin.Get();
	}
}
