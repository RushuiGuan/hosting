# Creating a Background Service Application

This guide walks you through creating a background service application using `Albatross.Hosting`. Background services are long-running applications that can run as Windows Services or Linux systemd daemons.

## Prerequisites

- .NET 10 SDK or later
- A code editor (Visual Studio, VS Code, or Rider)

## Step 1: Create the Project

Create a new console application:

```bash
dotnet new console -n MyService
cd MyService
```

Your project file (`MyService.csproj`) should look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
    </PropertyGroup>
</Project>
```

## Step 2: Add NuGet Packages

```bash
dotnet add package Albatross.Hosting
dotnet add package Microsoft.Extensions.Hosting.Abstractions
```

## Step 3: Create Configuration Files

Create two configuration files in your project root:

### appsettings.json

```json
{
    "program": {
        "app": "My Background Service",
        "serviceManager": "windows"
    }
}
```

| Property | Values | Description |
|----------|--------|-------------|
| `app` | string | Application name for logging and identification |
| `serviceManager` | `windows` or `systemd` | Service manager type |

- Use `windows` for Windows Services
- Use `systemd` for Linux/Mac daemons

### serilog.json

```json
{
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "System": "Warning",
                "Microsoft": "Warning"
            }
        },
        "WriteTo": {
            "Console": {
                "Name": "Console",
                "Args": {
                    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:sszzz} [{Level:w3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                }
            },
            "File": {
                "Name": "File",
                "Args": {
                    "path": "logs/myservice.log",
                    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:sszzz} [{Level:w3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                    "rollingInterval": "Day"
                }
            }
        },
        "Using": [
            "Albatross.Logging"
        ],
        "Enrich": [
            "FromLogContext",
            "WithThreadId",
            "WithMachineName",
            "WithErrorMessage"
        ]
    }
}
```

### Configure Files to Copy to Output

Update your `.csproj` to copy configuration files to the output directory:

```xml
<ItemGroup>
    <None Update="appsettings.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Update="serilog.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

## Step 4: Create the Hosted Service

Create a file named `MyHostedService.cs`. This is the entry point for your background service:

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MyService {
    public class MyHostedService : IHostedService {
        private readonly ILogger<MyHostedService> logger;

        public MyHostedService(ILogger<MyHostedService> logger) {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            logger.LogInformation("Service starting...");
            // Initialize your service here
            logger.LogInformation("Service started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            logger.LogInformation("Service stopping...");
            // Cleanup resources here
            logger.LogInformation("Service stopped");
            return Task.CompletedTask;
        }
    }
}
```

## Step 5: Create the Setup Class

Create a file named `MySetup.cs` to configure dependency injection:

```csharp
using Albatross.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyService {
    public class MySetup : Setup {
        public MySetup(string[] args) : base(args) { }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
            base.ConfigureServices(services, configuration);
            // Register your services here
            // services.AddSingleton<IMyDependency, MyDependency>();
        }
    }
}
```

## Step 6: Update Program.cs

Replace the contents of `Program.cs`:

```csharp
using System;
using System.Threading.Tasks;

namespace MyService {
    public class Program {
        public static Task Main(string[] args) {
            // Set current directory to the application's base directory
            // This is required for services to find configuration files
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            return new MySetup(args)
                .ConfigureServiceHost<MyHostedService>()
                .RunAsService()
                .RunAsync();
        }
    }
}
```

> **Important**: Setting `Environment.CurrentDirectory = AppContext.BaseDirectory` is required for services. When running as a Windows Service or systemd daemon, the working directory may not be the application directory, causing configuration files to not be found.

## Step 7: Run the Application

### Running as a Console Application

For development and testing, run directly:

```bash
dotnet run
```

### Running as a Windows Service

1. Publish the application:

```bash
dotnet publish -c Release -o ./publish
```

2. Install the service using `sc.exe`:

```cmd
sc create MyService binPath="C:\path\to\publish\MyService.exe"
```

3. Start the service:

```cmd
sc start MyService
```

4. Stop and remove the service:

```cmd
sc stop MyService
sc delete MyService
```

### Running as a Linux systemd Service

1. Publish the application:

```bash
dotnet publish -c Release -o ./publish
```

2. Create a systemd service file at `/etc/systemd/system/myservice.service`:

```ini
[Unit]
Description=My Background Service
After=network.target

[Service]
Type=notify
ExecStart=/path/to/publish/MyService
WorkingDirectory=/path/to/publish
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
```

3. Enable and start the service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable myservice
sudo systemctl start myservice
```

4. Check service status:

```bash
sudo systemctl status myservice
```

5. View logs:

```bash
sudo journalctl -u myservice -f
```

## Advanced Patterns

### Long-Running Background Tasks with BackgroundService

For services that need to perform continuous work, inherit from `BackgroundService`:

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyService {
    public class MyWorkerService : BackgroundService {
        private readonly ILogger<MyWorkerService> logger;

        public MyWorkerService(ILogger<MyWorkerService> logger) {
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            logger.LogInformation("Worker started");

            while (!stoppingToken.IsCancellationRequested) {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // Do your work here
                await DoWorkAsync(stoppingToken);

                // Wait before next iteration
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            logger.LogInformation("Worker stopped");
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken) {
            // Your business logic here
            await Task.CompletedTask;
        }
    }
}
```

Update `Program.cs` to use the worker service:

```csharp
return new MySetup(args)
    .ConfigureServiceHost<MyWorkerService>()
    .RunAsService()
    .RunAsync();
```

### Multiple Hosted Services

You can run multiple hosted services by registering them in `MySetup.cs`:

```csharp
public override void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
    base.ConfigureServices(services, configuration);

    // Register additional hosted services
    services.AddHostedService<SecondaryWorkerService>();
    services.AddHostedService<HealthCheckService>();
}
```

### Dependency Injection in Hosted Services

Inject any registered service into your hosted service:

```csharp
public class MyHostedService : IHostedService {
    private readonly ILogger<MyHostedService> logger;
    private readonly IConfiguration configuration;
    private readonly IMyRepository repository;

    public MyHostedService(
        ILogger<MyHostedService> logger,
        IConfiguration configuration,
        IMyRepository repository) {
        this.logger = logger;
        this.configuration = configuration;
        this.repository = repository;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        var setting = configuration.GetValue<string>("MySetting");
        logger.LogInformation("Starting with setting: {setting}", setting);

        await repository.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
```

### Graceful Shutdown with Cleanup

Handle graceful shutdown with proper resource cleanup:

```csharp
public class MyHostedService : IHostedService, IDisposable {
    private readonly ILogger<MyHostedService> logger;
    private Timer? timer;

    public MyHostedService(ILogger<MyHostedService> logger) {
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Service starting");

        timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    private void DoWork(object? state) {
        logger.LogInformation("Timer tick at: {time}", DateTimeOffset.Now);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Service stopping");

        timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose() {
        timer?.Dispose();
    }
}
```

## Environment-Specific Configuration

Create environment-specific configuration files:

- `appsettings.Development.json`
- `appsettings.Production.json`
- `serilog.Development.json`
- `serilog.Production.json`

Set the environment using the `ASPNETCORE_ENVIRONMENT` environment variable:

```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development

# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Development
```

For Windows Services, set the environment variable in the service configuration:

```cmd
sc create MyService binPath="C:\path\to\MyService.exe"
reg add "HKLM\SYSTEM\CurrentControlSet\Services\MyService" /v Environment /t REG_MULTI_SZ /d "ASPNETCORE_ENVIRONMENT=Production"
```

For systemd, add to the service file:

```ini
[Service]
Environment=ASPNETCORE_ENVIRONMENT=Production
```

## Sample Project

For a complete working example, see the [Sample.Daemon](https://github.com/RushuiGuan/hosting/tree/main/Sample.Daemon) project.
