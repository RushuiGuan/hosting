# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build entire solution
dotnet build hosting.sln

# Build specific project
dotnet build Albatross.Hosting/Albatross.Hosting.csproj

# Run sample WebApi
dotnet run --project Sample.WebApi/Sample.WebApi.csproj

# Run sample daemon/service
dotnet run --project Sample.Daemon/Sample.Daemon.csproj

# Create NuGet package
dotnet pack Albatross.Hosting/Albatross.Hosting.csproj
```

## Architecture Overview

This is a .NET bootstrapping library (`Albatross.Hosting`) that simplifies creating ASP.NET Web API and background service applications. It targets .NET 10.

### Core Classes

**Setup** (`Albatross.Hosting/Setup.cs`) - The entry point builder for applications:
- `ConfigureWebHost<TStartup>()` - Configure as a web API application
- `ConfigureServiceHost<THostedService>()` - Configure as a background service
- `RunAsService()` - Enable Windows Service or systemd hosting
- `RunAsync()` - Start the application

**Startup** (`Albatross.Hosting/Startup.cs`) - Web application configuration base class:
- Override `ConfigureServices()` to register dependencies
- Override `Configure()` to customize the request pipeline
- Toggle features via properties: `OpenApi`, `WebApi`, `Spa`, `LogUsage`, `PlainTextFormatter`
- Authentication configured via `AuthenticationSettings` from config

### Application Patterns

**WebApi Application:**
```csharp
public static Task Main(string[] args) {
    return new Setup(args)
        .ConfigureWebHost<MyStartup>()
        .RunAsync();
}
```

**Background Service:**
```csharp
public static Task Main(string[] args) {
    return new MySetup(args)
        .ConfigureServiceHost<MyHostedService>()
        .RunAsService()
        .RunAsync();
}
```

### Configuration Files

- `appsettings.json` / `appsettings.{env}.json` - Application configuration
- `hostsettings.json` - Host URLs configuration
- `serilog.json` - Serilog logging configuration
- Environment determined by `ASPNETCORE_ENVIRONMENT`

### Key Dependencies

- `Albatross.Logging` - Serilog integration
- `Albatross.Config` - Configuration management
- `Albatross.Authentication.AspNetCore` - Authentication helpers

## Code Style

- Uses tabs for indentation (tab width: 4)
- Opening braces on same line (K&R style)
- Nullable reference types enabled
- No `this.` qualification preferred
