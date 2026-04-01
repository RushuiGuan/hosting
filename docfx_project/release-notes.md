# Release Notes

## hosting-10.2.0 (2026-03-31)

### Breaking Changes

- **`Setup` constructor signature changed** - The constructor parameter `bool supressUnhandledArgumentExceptionLogging` has been replaced with `string configRoot`. Callers must pass the configuration root directory explicitly (e.g., `new Setup(args, AppContext.BaseDirectory)`).

- **Removed `SupressUnhandledArgumentExceptionLogging` and `ConfigureLogging()`** - The `SupressUnhandledArgumentExceptionLogging` property and the virtual `ConfigureLogging()` override point have been removed. Serilog filtering must now be configured directly via the `serilog.json` configuration file.

- **`hostsettings.json` no longer loaded** - Host URL configuration via `hostsettings.json` is no longer part of the default configuration pipeline.

- **Removed `LogUsage` and `HttpRequestLoggingMiddleware`** - The `LogUsage` property on `Startup`, the `HttpRequestLoggingMiddleware`, `UsageWriter`, and `UsageData` classes have all been removed. Request logging should now be implemented using Serilog's built-in middleware or custom enrichers.

### Changes

- **`IHttpContextAccessor` registered by default** - `services.AddHttpContextAccessor()` is now called in `Startup.ConfigureServices()`, making `IHttpContextAccessor` available for injection without additional setup.

- **Serilog configured from `serilog.json`** - Serilog is now configured by reading `serilog.json` (and `serilog.{environment}.json`) directly via `Configuration` in the `Setup` constructor, replacing the previous `SetupSerilog.UseConfigFile()` approach.

- **`config-root` constructor parameter** - The base directory from which configuration files are loaded is now passed directly as the `configRoot` constructor argument to `Setup`, defaulting to `AppContext.BaseDirectory`.

- **`RazorPages` toggle on `Startup`** - A new `protected virtual bool RazorPages` property (default `false`) can be overridden to enable Razor Pages support. When `true`, `AddRazorPages()` is registered and `MapRazorPages()` is wired into the endpoint pipeline.

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
