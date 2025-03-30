using Albatross.Authentication.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Albatross.Hosting {
	public class UsageWriter {
		public ILogger Logger { get; }

		public UsageWriter(ILogger logger) {
			Logger = logger;
		}

		public void Write(UsageData data) {
			Logger.LogInformation("{user} {remoteAddress} {url} {method}", data.User, data.RemoteIpAddress, data.Url, data.Method);
		}
	}
	public record class UsageData {
		public UsageData(HttpContext context) {
			User = context.GetIdentity();
			Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
			Method = context.Request.Method;
			RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "N.A.";
		}
		public string? User { get; set; }
		public string Url { get; set; }
		public string RemoteIpAddress { get; set; }
		public string Method { get; set; }
	}
	public class HttpRequestLoggingMiddleware {
		private readonly RequestDelegate next;

		public HttpRequestLoggingMiddleware(RequestDelegate next) {
			this.next = next;
		}

		public async Task InvokeAsync(HttpContext context, UsageWriter writer) {
			var data = new UsageData(context);
			writer.Write(data);
			await next(context);
		}
	}
}