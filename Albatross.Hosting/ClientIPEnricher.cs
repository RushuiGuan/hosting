using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Albatross.Hosting {
	/// <summary>
	/// A Serilog enricher that adds the client IP address to log events.
	/// </summary>
	public class ClientIPEnricher : ILogEventEnricher {
		public const string PropertyName = "ClientIP";
		private readonly IHttpContextAccessor httpContextAccessor;

		public ClientIPEnricher(IHttpContextAccessor httpContextAccessor) {
			this.httpContextAccessor = httpContextAccessor;
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
			var httpContext = httpContextAccessor.HttpContext;
			if (httpContext == null) {
				return;
			}

			var remoteIp = httpContext.Connection.RemoteIpAddress;
			if (remoteIp == null) {
				return;
			}

			var property = propertyFactory.CreateProperty(PropertyName, remoteIp.ToString());
			logEvent.AddPropertyIfAbsent(property);
		}
	}
}
