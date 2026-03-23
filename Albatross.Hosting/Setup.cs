using Albatross.Config;
using Albatross.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Albatross.Hosting {
	public class Setup {
		protected IHostBuilder hostBuilder;
		protected IConfiguration configuration;
		protected SetupSerilog setupSerilog;
		string GetConfigRoot(string[] args, string? environmentalPrefix) {
			var builder = new ConfigurationBuilder()
				.AddEnvironmentVariables(environmentalPrefix)
				.AddCommandLine(args);
			var configRoot = builder.Build()["config-root"];
			return configRoot ?? AppContext.BaseDirectory;
		}
		public Setup(string[] args, string? environmentPrefix) {
			var environment = EnvironmentSetting.ASPNETCORE_ENVIRONMENT.Value;
			hostBuilder = Host.CreateDefaultBuilder(args).UseSerilog();
			var configRoot = GetConfigRoot(args, environmentPrefix);
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(configRoot)
				.AddJsonFile("appsettings.json", false, true);
			configBuilder.AddJsonFile("serilog.json", false, true);
			if (!string.IsNullOrEmpty(environment)) {
				configBuilder.AddJsonFile($"appsettings.{environment}.json", true, true);
				configBuilder.AddJsonFile($"serilog.{environment}.json", true, true);
			}
			configBuilder.AddCommandLine(args);
			this.configuration = configBuilder.Build();
			hostBuilder.ConfigureAppConfiguration(builder => {
				builder.Sources.Clear();
				builder.AddConfiguration(configuration);
			});
			this.setupSerilog = new SetupSerilog();
			this.setupSerilog.Configure(cfg => cfg.ReadFrom.Configuration(this.configuration));
		}

		public virtual Setup RunAsService() {
			var setting = new ProgramSetting(configuration);
			switch (setting.ServiceManager) {
				case ProgramSetting.WindowsServiceManager:
					hostBuilder.UseWindowsService();
					break;
				case ProgramSetting.SystemDServiceManager:
					hostBuilder.UseSystemd();
					break;
				default:
					throw new ConfigurationException("Service configuration not set at Program__ServiceManager");
			}
			return this;
		}
		public virtual Setup ConfigureWebHost<Startup>() where Startup : Hosting.Startup {
			hostBuilder.ConfigureWebHostDefaults(webBuilder => {
				webBuilder.UseStartup<Startup>();
				webBuilder.PreferHostingUrls(true);
			});
			return this;
		}
		public virtual Setup ConfigureServiceHost<T>() where T : class, IHostedService {
			hostBuilder.ConfigureServices((hostContext, services) => {
				services.AddHostedService<T>();
			});
			return this;
		}
		public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
			services.TryAddSingleton(new ProgramSetting(configuration));
			services.TryAddSingleton(EnvironmentSetting.ASPNETCORE_ENVIRONMENT);
			services.TryAddSingleton(provider => provider.GetRequiredService<ILoggerFactory>().CreateLogger("default"));
		}
		public virtual async Task RunAsync() {
			await using var logger = this.setupSerilog.Create();
			this.hostBuilder.ConfigureServices((context, services) => this.ConfigureServices(services, context.Configuration));
			await this.hostBuilder.Build().RunAsync();
			logger.Information("Application stopped");
		}
	}
}