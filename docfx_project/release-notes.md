# Release Notes

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
