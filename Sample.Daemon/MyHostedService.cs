using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Daemon {
	public class MyHostedService : IHostedService {
		private readonly ILogger<MyHostedService> logger;

		public MyHostedService(ILogger<MyHostedService> logger) {
			this.logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Startup started");
			logger.LogInformation("Startup completed");
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Shutdown started");
			logger.LogInformation("Shutdown completed");
			return Task.CompletedTask;
		}
	}
}
