using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Sample.WebApi.Services;
using System;
using System.Threading.Tasks;

namespace Sample.WebApi.Controllers {
	[Route("api/signalr")]
	[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	[ApiController]
	public class SignalRController : ControllerBase {
		readonly SignalRHub hub;

		public SignalRController(SignalRHub hub) {
			this.hub = hub;
		}

		/// <summary>
		/// Called by the SignalR js client as {hubUrl}/negotiate.  Returning url and accessToken redirects the client to Azure SignalR Service.
		/// </summary>
		[HttpPost("negotiate")]
		public async Task<NegotiateResult> Negotiate() {
			var response = await hub.Context.NegotiateAsync(new NegotiationOptions {
				UserId = User.Identity?.Name,
				TokenLifetime = TimeSpan.FromMinutes(30),
			}, HttpContext.RequestAborted);
			return new NegotiateResult(response.Url ?? string.Empty, response.AccessToken ?? string.Empty);
		}

		/// <summary>
		/// Test trigger; any publisher holding the connection string can send to this application's hub the same way.
		/// </summary>
		[HttpPost("broadcast")]
		public Task Broadcast([FromBody] string message)
			=> hub.Context.Clients.All.SendAsync("message", User.Identity?.Name, message, HttpContext.RequestAborted);
	}

	public record class NegotiateResult(string Url, string AccessToken);
}
