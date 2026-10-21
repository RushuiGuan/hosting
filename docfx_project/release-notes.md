# Release Notes

## hosting-10.3.2 (2026-10-20)

### Breaking Changes

- **FluentValidation removed** - `Albatross.Hosting` no longer references `FluentValidation`. Validation errors are now `ValidationError` items from `Albatross.Exceptions`, each pairing an optional field with an error message.

- **`HasProblem` takes validation errors** - `ValidationExtensions.HasProblem` now extends `IEnumerable<ValidationError>` instead of FluentValidation's `ValidationResult`, and its `status` parameter has been removed. Write request validation as a method that returns `ValidationError` items and return the problem with `UnprocessableEntity(problem)`.

- **`FluentValidation.ValidationException` no longer maps to 422** - `GlobalExceptionHandler` no longer recognizes it, so an application that still throws it gets a 500 response, logged as a server error. This change does not cause a compilation error; throw `Albatross.Exceptions.FieldValidationException` for field errors, or `Albatross.Exceptions.ValidationException` for a single message.

### Changes

- **Field errors in validation responses** - A `FieldValidationException` is returned with status 422 as a `ValidationProblemDetails` by both `IExceptionHandler` and `GlobalExceptionHandler`, with the errors grouped by field under `errors`. An error without a field appears under the `""` key.

- **Validation errors are never masked** - Validation errors describe the caller's own input, so they are returned even when `MaskExceptionDetail` is `true`: the field errors of a `FieldValidationException`, and the message of a `ValidationException` or `System.ComponentModel.DataAnnotations.ValidationException` as `detail`. Both messages were previously masked. This change does not cause a compilation error; an application that puts sensitive information in a validation message should remove it.

- **`CreateProblemDetails` extensions** - New `ValidationExtensions.CreateProblemDetails` methods build the problem body from a set of `ValidationError` items or from a `FieldValidationException`.

- **`DefaultExceptionHandler` status codes aligned with `GlobalExceptionHandler`** - `System.ComponentModel.DataAnnotations.ValidationException` now returns 422, `AuthenticationException` returns 401, and `AccessViolationException` and `UnauthorizedAccessException` return 403. They previously returned 500 from `DefaultExceptionHandler`.

- **`GlobalExceptionHandler.Title`** - The generic title of the global handler is now exposed as a public constant.

- **`HasProblem` reads its errors once** - A validator written with `yield return` runs a single time.

### Dependencies

- Bumped `Albatross.Exceptions` to `1.0.6`
- Bumped `Microsoft.AspNetCore.*` and `Microsoft.Extensions.Hosting.*` package references to `10.0.13`
- Bumped `Swashbuckle.AspNetCore` to `10.3.1`
- Bumped `Microsoft.SourceLink.GitHub` to `10.0.402`
- Removed `FluentValidation`

### Infrastructure

- **Unit tests** - New `Albatross.Hosting.UnitTesting` project covering the exception to status code mapping of both handlers, detail masking, validation responses and `HasProblem`.

---

## hosting-10.2.4 (2026-10-08)

### Changes

- **Built-in heartbeat endpoint** - A new `HeartbeatController` serves `GET api/heartbeat`, returning the application name, environment and current UTC timestamp. The endpoint is `[AllowAnonymous]`, so it can be used by load balancers and monitoring probes without credentials. It is discovered automatically by any web host built on `Albatross.Hosting`.

- **`UseHeartbeat` toggle on `Startup`** - A new `protected bool UseHeartbeat` property (default `true`). Set it to `false` in a derived `Startup` to remove the heartbeat controller from the application.

---

## hosting-10.2.3 (2026-10-02)

### Changes

- **Optional Serilog request logging** - A new `protected bool LogRequests` property on `Startup` (default `false`). When `true`, `UseSerilogRequestLogging()` is added to the pipeline, enriched with a `User` property holding the authenticated user name (or `<anon>`). The middleware sits outside the exception handler, so each failed request is logged once with the status code of the handled response. Override the virtual `UseRequestLogging()` to customize the middleware options.

### Dependencies

- Added `Serilog.AspNetCore` `10.0.0`
- Bumped `Albatross.Config` to `8.0.2`

---

## hosting-10.2.2 (2026-09-13)

First stable release of the changes listed under [hosting-10.2.1](#hosting-1021-2026-09-13), which was published only as release candidates. No additional changes.

---

## hosting-10.2.1 (2026-09-13)

### Bug Fixes

- **`TransformAngularConfig.UpdateBaseHref()` no longer writes when the file is already correct** - The index html is now compared against the transformed content and written only when the base href actually changes. Previously the writer was opened on every startup regardless of whether the regex matched anything, which threw `UnauthorizedAccessException` and took the host down when the app was installed in a read-only directory (under `%ProgramFiles%`, or served by an IIS application pool or service account identity).

- **Base href write failures are no longer fatal** - A failure to write the index html is now logged as an error instead of propagating. In a deployed environment setting the base href is the installer's responsibility, so the app should still start.

### Dependencies

- Bumped `Microsoft.AspNetCore.*` and `Microsoft.Extensions.Hosting.*` package references to `10.0.12`
- Bumped `Albatross.Config` to `8.0.1`

### Infrastructure

- **SourceLink moved to `Albatross.Hosting.csproj`** - `Microsoft.SourceLink.GitHub` (updated to `10.0.401`) is now referenced by the package project directly rather than applied to every project through `Directory.Build.props`.

---

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
