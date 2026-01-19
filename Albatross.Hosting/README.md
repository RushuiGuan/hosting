# Albatross.Hosting

A .NET library that simplifies creating ASP.NET Web API and background service applications with preconfigured settings. It provides a streamlined bootstrapping experience with built-in Serilog logging, configuration management, authentication, and Swagger documentation while maintaining full access to ASP.NET Core's capabilities.

Designed for enterprise applications, Albatross.Hosting enforces consistent patterns for web APIs and background services with out-of-the-box support for JWT/Kerberos authentication, global exception handling, and request logging.

## Key Features
- **Minimal Boilerplate** - Fluent API for application bootstrapping with sensible defaults
- **Serilog Integration** - Pre-configured logging via [Albatross.Logging](https://www.nuget.org/packages/Albatross.Logging)
- **Configuration Management** - Simplified config handling via [Albatross.Config](https://www.nuget.org/packages/Albatross.Config)
- **Authentication Support** - Built-in JWT Bearer and Kerberos (Negotiate) authentication
- **OpenAPI/Swagger** - Pre-configured Swagger documentation endpoints
- **Global Exception Handling** - Consistent error responses with ProblemDetails support
- **SPA Hosting** - Built-in Angular application hosting support
- **Request Logging** - HTTP request/response logging middleware
- **Response Compression** - Gzip and Brotli compression support
- **Service Hosting** - Windows Service and systemd support for background services

## Quick Start
### Web API Application
Create a console app and change the SDK to `Microsoft.NET.Sdk.Web`. In the `Program.cs` file:
```csharp
public class Program {
    public static Task Main(string[] args) {
        return new Setup(args)
            .ConfigureWebHost<MyStartup>()
            .RunAsync();
    }
}
```

Create a startup class:
```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }
}
```

### Background Service Application
Create a console app. In the `Program.cs` file:
```csharp
public class Program {
    public static Task Main(string[] args) {
        Environment.CurrentDirectory = AppContext.BaseDirectory;
        return new MySetup(args)
            .ConfigureServiceHost<MyHostedService>()
            .RunAsService()
            .RunAsync();
    }
}
```

Create a setup class:
```csharp
public class MySetup : Setup {
    public MySetup(string[] args) : base(args) { }
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
        base.ConfigureServices(services, configuration);
    }
}
```

Create a hosted service:
```csharp
public class MyHostedService : IHostedService {
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

## Dependencies
- **Albatross.Logging 10.0.1+**
- **Albatross.Config 7.5.11+**
- **Microsoft.AspNetCore.Authentication.JwtBearer 10.0.1+**

## Prerequisites
- **.NET 10 SDK**

## 📖 Documentation

**📚 [GitHub Repository](https://github.com/RushuiGuan/hosting)**

### Links
- **[Web API Application Guide](https://github.com/RushuiGuan/hosting/blob/main/docs/webapi-app.md)**
- **[Service Application Guide](https://github.com/RushuiGuan/hosting/blob/main/docs/service-app.md)**
- **[SPA Hosting](https://github.com/RushuiGuan/hosting/blob/main/docs/spa-hosting.md)**
- **[Custom Error Response](https://github.com/RushuiGuan/hosting/blob/main/docs/custom-error-response.md)**
- **[Request Logging](https://github.com/RushuiGuan/hosting/blob/main/docs/request-logging.md)**
- **[Plain Text Input Formatter](https://github.com/RushuiGuan/hosting/blob/main/docs/plain-text-input-formatter.md)**
