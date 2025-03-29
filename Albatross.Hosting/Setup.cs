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
		string environment { get; init; }
		protected SetupSerilog setupSerilog;

		public Setup(string[] args, bool supressUnhandledArgumentExceptionLogging = false) {
			this.SupressUnhandledArgumentExceptionLogging = supressUnhandledArgumentExceptionLogging;
			environment = EnvironmentSetting.ASPNETCORE_ENVIRONMENT.Value;
			hostBuilder = Host.CreateDefaultBuilder(args).UseSerilog();
			this.setupSerilog = ConfigureLogging(new SetupSerilog(), environment, args);
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", false, true);
			if (!string.IsNullOrEmpty(environment)) { configBuilder.AddJsonFile($"appsettings.{environment}.json", true, true); }

			this.configuration = configBuilder.AddJsonFile("hostsettings.json", true, false)
				.AddEnvironmentVariables()
				.AddCommandLine(args)
				.Build();

			hostBuilder.ConfigureAppConfiguration(builder => {
				builder.Sources.Clear();
				builder.AddConfiguration(configuration);
			});
		}

		/// <summary>
		/// If true, unhandled ArgumentException will be not be logged.  This is useful for public faceing API where the client often send bad request.
		/// </summary>
		public bool SupressUnhandledArgumentExceptionLogging { get; private set; }
		private readonly static ScalarValue ExceptionHandlerMiddlewareSourceContext = new ScalarValue("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware");
		protected virtual SetupSerilog ConfigureLogging(SetupSerilog setup, string environment, string[] args) {
			if (SupressUnhandledArgumentExceptionLogging) {
				setup.Configure(c => c.Filter.ByExcluding(e => e.Exception is ArgumentException
					&& e.Properties.TryGetValue(Serilog.Core.Constants.SourceContextPropertyName, out var sourceContext)
					&& sourceContext.Equals(ExceptionHandlerMiddlewareSourceContext)));
			}
			setup.UseConfigFile(environment, null, args);
			return setup;
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
			using var logger = this.setupSerilog.Create();
			this.hostBuilder.ConfigureServices((context, services) => this.ConfigureServices(services, context.Configuration));
			await this.hostBuilder.Build().RunAsync();
			logger.Information("Application stopped");
		}
	}
}