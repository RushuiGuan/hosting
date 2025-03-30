# New Service Application
* Please find the [sample project](../Sample.Daemon/) here.
* Create a new console application that targets net8.0
* Open the project file and change the Project Sdk to `Microsoft.NET.Sdk.Web`
* Add a reference to the `Albatross.Hosting` and `Microsoft.Extensions.Hosting.Abstractions` assembly
* Create the `appsettings.json` and `serilog.json` file in the project root
* Update `appsettings.json` and set the `program.serviceManager` property to `windows` or `systemd`.
	* `windows` is for Microsoft windows services
	* `systemd` is for Mac or linux.
	```json
	{
		"program": {
			"app": "Sample App",
			"serviceManager": "windows"
		}
	}
	```
* Set the files to `Copy to Output Directory` = `Copy if newer`
* Create `MyHostedService.cs` file.  This is the entry point of your service.
	```csharp
	public class MyHostedService : IHostedService {
		private readonly ILogger<MyHostedService> logger;
		public MyHostedService(ILogger<MyHostedService> logger) {
			this.logger = logger;
		}
		public Task StartAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Startup started");
			return Task.CompletedTask;
		}
		public Task StopAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Shutdown started");
			return Task.CompletedTask;
		}
	}
	```
* Create `MySetup.cs` file and setup the dependencies here
	```csharp
	public class MySetup : Setup {
		public MySetup(string[] args) : base(args) { }
		public override void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
			base.ConfigureServices(services, configuration);
		}
	}
	```
* Update the Main method of the`Program.cs` file with code below.
	```csharp
	public static Task Main(string[] args) {
	System.Environment.CurrentDirectory = AppContext.BaseDirectory;
	return new MySetup(args)
		.ConfigureServiceHost<MyHostedService>()
		.RunAsService()
		.RunAsync();
	}
	```