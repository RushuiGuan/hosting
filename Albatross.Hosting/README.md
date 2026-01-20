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

**📚 [Complete Documentation](https://rushuiguan.github.io/hosting/)**

### Links
- **[Web API Application Guide](https://rushuiguan.github.io/hosting/articles/webapi-app.html)**
- **[Service Application Guide](https://rushuiguan.github.io/hosting/articles/service-app.html)**
- **[Authentication](https://rushuiguan.github.io/hosting/articles/authentication.html)**
- **[SPA Hosting](https://rushuiguan.github.io/hosting/articles/spa-hosting.html)**
- **[Custom Error Response](https://rushuiguan.github.io/hosting/articles/custom-error-response.html)**
- **[Request Logging](https://rushuiguan.github.io/hosting/articles/request-logging.html)**
- **[Plain Text Input Formatter](https://rushuiguan.github.io/hosting/articles/plain-text-input-formatter.html)**
