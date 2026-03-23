# Release Notes

## hosting-10.2.0 (2026-03-23)

### Breaking Changes

- **`Setup` constructor signature changed** - The constructor parameter `bool supressUnhandledArgumentExceptionLogging` has been replaced with `string? environmentPrefix`. Callers must update their constructor call (e.g., `new Setup(args, null)` or pass a prefix string for environment variable filtering).

- **Removed `SupressUnhandledArgumentExceptionLogging` and `ConfigureLogging()`** - The `SupressUnhandledArgumentExceptionLogging` property and the virtual `ConfigureLogging()` override point have been removed. Serilog filtering must now be configured directly via the `serilog.json` configuration file.

- **`hostsettings.json` no longer loaded** - Host URL configuration via `hostsettings.json` is no longer part of the default configuration pipeline.

- **Removed `LogUsage` and `HttpRequestLoggingMiddleware`** - The `LogUsage` property on `Startup`, the `HttpRequestLoggingMiddleware`, `UsageWriter`, and `UsageData` classes have all been removed. Request logging should now be implemented using Serilog's built-in middleware or custom enrichers.

### Changes

- **Added `ClientIPEnricher`** - A new Serilog `ILogEventEnricher` that automatically adds the client IP address as a `ClientIP` property to log events. It uses `IHttpContextAccessor` to retrieve the remote IP from the current HTTP context.

- **`IHttpContextAccessor` registered by default** - `services.AddHttpContextAccessor()` is now called in `Startup.ConfigureServices()`, making `IHttpContextAccessor` available for injection without additional setup.

- **Serilog configured from `serilog.json`** - Serilog is now configured by reading `serilog.json` (and `serilog.{environment}.json`) directly via `Configuration` in the `Setup` constructor, replacing the previous `SetupSerilog.UseConfigFile()` approach.

- **`config-root` command line argument** - A new `config-root` command line/environment variable argument allows overriding the base directory from which configuration files are loaded. Defaults to `AppContext.BaseDirectory`.

- **`RunAsync()` uses `await using`** - The bootstrap logger is now disposed asynchronously with `await using` for correct async teardown.

---

## hosting-10.1.0 (2026-01-27)

### Breaking Changes

- **Simplified global exception handling** - Removed the `IGlobalExceptionHandler` interface, `HttpApiException`, `ErrorMessage`, `ProblemDetailsWithTraceId`, `LegacyGobalExceptionHandler`, and `ExceptionHandlerSerializationOptions` classes. The global exception handler is now a sealed `GlobalExceptionHandler` class that serves as a fallback for unhandled exceptions. Errors should be handled explicitly using `ActionResult` in controller actions.

- **Removed `GlobalExceptionHandler` property from `Startup`** - The virtual `GlobalExceptionHandler` property has been removed. The handler is no longer customizable via override.

---

## hosting-10.0.1 (2026-01-20)

### Documentation

- **Added comprehensive documentation** - Migrated and expanded documentation to DocFX format:
  - Authentication guide covering Kerberos and JWT Bearer token configuration
  - Custom error response handling guide
  - Plain text input formatter usage
  - Service/daemon application development guide
  - SPA hosting configuration guide
  - Web API application development guide
  - Request logging documentation

- **Enhanced README** - Expanded the `Albatross.Hosting` README with more detailed usage examples and configuration options

### Infrastructure

- **Added GitHub Actions workflow** - Added GitHub Pages workflow for automated documentation publishing

---

## hosting-10.0.0 (2025-12-10)

### Breaking Changes

- **Upgraded to .NET 10** - The target framework has been upgraded from .NET 9 to .NET 10. All consuming projects must target .NET 10 or later.

### Changes

- **Fixed default authentication scheme** - Changed authentication configuration to use `AuthenticationSettings.GetDefault()` method instead of the `DefaultAuthenticationScheme` property for more reliable scheme resolution.

- **Updated OpenAPI integration** - Updated to the latest OpenAPI types:
  - Changed `Dictionary<string, OpenApiSecurityScheme>` to `Dictionary<string, IOpenApiSecurityScheme>`
  - Changed `doc.SecurityRequirements` to `doc.Security`
  - Updated to use `OpenApiSecuritySchemeReference` for security requirement references

### Dependencies

Updated all dependencies to .NET 10 compatible versions:
- `Albatross.Logging` 10.0.1
- `Albatross.Serialization.Json` 10.0.0
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.1
- `Microsoft.AspNetCore.Authentication.Negotiate` 10.0.1
- `Microsoft.AspNetCore.SpaServices.Extensions` 10.0.1
- `Microsoft.Extensions.Hosting.Systemd` 10.0.1
- `Microsoft.Extensions.Hosting.WindowsServices` 10.0.1
- `Microsoft.AspNetCore.OpenApi` 10.0.1
- `Swashbuckle.AspNetCore` 10.0.1
