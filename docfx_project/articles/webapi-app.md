# Creating a Web API Application

This guide walks you through creating a new ASP.NET Web API application using `Albatross.Hosting`.

## Prerequisites

- .NET 10 SDK or later
- A code editor (Visual Studio, VS Code, or Rider)

## Step 1: Create the Project

Create a new console application:

```bash
dotnet new console -n MyWebApi
cd MyWebApi
```

Open the project file (`MyWebApi.csproj`) and change the SDK to `Microsoft.NET.Sdk.Web`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
    </PropertyGroup>
</Project>
```

## Step 2: Add the NuGet Package

```bash
dotnet add package Albatross.Hosting
```

## Step 3: Create Configuration Files

Create three configuration files in your project root:

### appsettings.json

```json
{
    "program": {
        "app": "My Web Api"
    }
}
```

The `program.app` property sets your application name, which appears in Swagger documentation and logs.

### hostsettings.json

```json
{
    "urls": "http://localhost:5000"
}
```

This configures the URLs your application listens on. Use `http://*:5000` to listen on all network interfaces.

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
                    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ssz} {SourceContext} [{Level:w3}] {Message:lj}{NewLine}{Exception}"
                }
            }
        },
        "Using": [
            "Albatross.Logging"
        ],
        "Enrich": [
            "FromLogContext",
            "WithThreadId",
            "WithMachineName"
        ]
    }
}
```

### Configure Files to Copy to Output

Update your `.csproj` to copy configuration files to the output directory:

```xml
<ItemGroup>
    <None Include="appsettings*.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Include="hostsettings.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Include="serilog*.json">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

## Step 4: Create the Startup Class

Create a file named `MyStartup.cs`:

```csharp
using Microsoft.Extensions.Configuration;

namespace MyWebApi {
    public class MyStartup : Albatross.Hosting.Startup {
        public MyStartup(IConfiguration configuration) : base(configuration) { }
    }
}
```

The `Startup` base class provides preconfigured middleware and services. You can override its properties and methods to customize behavior:

| Property | Default | Description |
|----------|---------|-------------|
| `OpenApi` | `true` | Enable Swagger/OpenAPI documentation |
| `WebApi` | `true` | Enable Web API controllers |
| `Spa` | `false` | Enable SPA hosting |
| `LogUsage` | `true` | Enable HTTP request logging |
| `PlainTextFormatter` | `true` | Enable plain text input formatter |

### Registering Services

Override `ConfigureServices` to register your own dependencies:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    public override void ConfigureServices(IServiceCollection services) {
        base.ConfigureServices(services);
        // Register your services here
        services.AddScoped<IMyService, MyService>();
    }
}
```

### Customizing the Pipeline

Override `Configure` to customize the request pipeline:

```csharp
public override void Configure(IApplicationBuilder app, ProgramSetting programSetting,
    EnvironmentSetting environmentSetting, ILogger<Startup> logger) {

    base.Configure(app, programSetting, environmentSetting, logger);
    // Add additional middleware here
}
```

## Step 5: Update Program.cs

Replace the contents of `Program.cs`:

```csharp
using Albatross.Hosting;
using System.Threading.Tasks;

namespace MyWebApi {
    public class Program {
        public static Task Main(string[] args) {
            return new Setup(args)
                .ConfigureWebHost<MyStartup>()
                .RunAsync();
        }
    }
}
```

## Step 6: Create a Controller

Create a `Controllers` folder and add a controller:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyWebApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase {
        private readonly ILogger logger;

        public ValuesController(ILogger logger) {
            this.logger = logger;
        }

        [HttpGet]
        public string Get() {
            logger.LogInformation("Get endpoint called");
            return "Hello, World!";
        }

        [HttpGet("{id}")]
        public string Get(int id) {
            return $"Value: {id}";
        }

        [HttpPost]
        public IActionResult Post([FromBody] string value) {
            logger.LogInformation("Received: {value}", value);
            return Ok(new { received = value });
        }
    }
}
```

## Step 7: Run the Application

```bash
dotnet run
```

Your API is now running at `http://localhost:5000`. Access the Swagger UI at `http://localhost:5000/swagger`.

## Adding Authentication

### JWT Bearer Authentication

Add JWT Bearer authentication by configuring `appsettings.json`:

```json
{
    "program": {
        "app": "My Web Api"
    },
    "authentication": {
        "bearerTokens": [
            {
                "provider": "MyProvider",
                "authority": "https://your-identity-provider.com",
                "validateIssuer": true,
                "issuer": "https://your-identity-provider.com",
                "validateAudience": true,
                "audience": ["your-client-id"],
                "validateLifetime": true,
                "requireHttpsMetadata": true
            }
        ]
    }
}
```

### Kerberos/Windows Authentication

Enable Kerberos authentication:

```json
{
    "program": {
        "app": "My Web Api"
    },
    "authentication": {
        "useKerberos": true
    }
}
```

### Multiple Authentication Schemes

You can combine multiple authentication schemes and set a default:

```json
{
    "authentication": {
        "default": "Google",
        "useKerberos": true,
        "bearerTokens": [
            {
                "provider": "Google",
                "authority": "https://accounts.google.com",
                "issuer": "https://accounts.google.com",
                "audience": ["your-google-client-id"]
            }
        ]
    }
}
```

### Protecting Endpoints

Use the `[Authorize]` attribute on controllers or actions:

```csharp
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SecureController : ControllerBase {
    [HttpGet]
    public string Get() => "Authenticated!";

    [AllowAnonymous]
    [HttpGet("public")]
    public string GetPublic() => "Public endpoint";

    [Authorize(AuthenticationSchemes = "Google")]
    [HttpGet("google-only")]
    public string GetGoogleOnly() => "Google authenticated!";
}
```

## Adding CORS

Configure allowed origins in `appsettings.json`:

```json
{
    "cors": [
        "http://localhost:4200",
        "https://myapp.example.com"
    ]
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

## Suppressing ArgumentException Logging

For public-facing APIs where clients may send invalid requests, you can suppress logging of `ArgumentException`:

```csharp
public static Task Main(string[] args) {
    return new Setup(args, supressUnhandledArgumentExceptionLogging: true)
        .ConfigureWebHost<MyStartup>()
        .RunAsync();
}
```

## Sample Project

For a complete working example, see the [Sample.WebApi](https://github.com/RushuiGuan/hosting/tree/main/Sample.WebApi) project.
