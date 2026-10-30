using Microsoft.Azure.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.WebApi.Services {
	/// <summary>
	/// Holds the hub context for Azure SignalR Service in serverless mode.  The app hosts no hub; clients connect
	/// directly to the service with an access token minted by <see cref="ServiceHubContext.NegotiateAsync"/>.
	/// </summary>
	public class SignalRHub : IHostedService {
		// one hub per application: the client token is bound to this hub, so clients only receive what is sent to it
		public const string HubName = "sample";
		// primary endpoint is the section's own value; children such as "backup:secondary" are named endpoints
		const string ConnectionStringKey = "Azure:SignalR:ConnectionString";
		readonly IConfiguration configuration;
		readonly ILoggerFactory loggerFactory;
		ServiceHubContext? hubContext;

		public SignalRHub(IConfiguration configuration, ILoggerFactory loggerFactory) {
			this.configuration = configuration;
			this.loggerFactory = loggerFactory;
		}

		public ServiceHubContext Context => hubContext ?? throw new InvalidOperationException("SignalR hub context has not been started");

		public async Task StartAsync(CancellationToken cancellationToken) {
			var section = configuration.GetSection(ConnectionStringKey);
			var named = section.AsEnumerable(makePathsRelative: true)
				.Where(x => !string.IsNullOrEmpty(x.Value))
				.Select(x => new ServiceEndpoint(x.Key, x.Value!));
			var endpoints = (string.IsNullOrEmpty(section.Value) ? named : named.Prepend(new ServiceEndpoint(section.Value))).ToArray();
			if (endpoints.Length == 0) {
				throw new InvalidOperationException($"Azure SignalR connection string is missing; set the {ConnectionStringKey} configuration value");
			}
			var serviceManager = new ServiceManagerBuilder()
				.WithOptions(options => {
					options.ServiceEndpoints = endpoints;
					// transient (rest) transport does not support multiple endpoints
					options.ServiceTransportType = endpoints.Length > 1 ? ServiceTransportType.Persistent : ServiceTransportType.Transient;
				})
				.WithLoggerFactory(loggerFactory)
				.BuildServiceManager();
			hubContext = await serviceManager.CreateHubContextAsync(HubName, cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken) {
			if (hubContext != null) {
				await hubContext.DisposeAsync();
			}
		}
	}
}
