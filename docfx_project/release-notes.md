# Release Notes

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
