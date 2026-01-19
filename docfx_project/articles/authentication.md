# Authentication

Albatross.Hosting provides built-in support for authentication using JWT Bearer tokens and Kerberos (Windows/Negotiate) authentication. Authentication is configured entirely through the `appsettings.json` file, requiring no code changes for most scenarios.

## Features

- **JWT Bearer Authentication** - Support for multiple OAuth/OIDC providers (Google, Azure AD, Auth0, etc.)
- **Kerberos Authentication** - Windows integrated authentication using the Negotiate scheme
- **Multiple Schemes** - Configure multiple authentication providers simultaneously
- **Configuration-Based** - All authentication settings are managed through appsettings.json
- **Automatic Setup** - Authentication middleware is automatically configured when settings are present

## How It Works

Authentication is automatically enabled when the `authentication` section is present in your configuration. The `Startup` base class reads the configuration and:

1. Registers the appropriate authentication schemes
2. Configures JWT Bearer validation for each provider
3. Enables Kerberos/Negotiate authentication if specified
4. Sets a default authentication scheme
5. Adds authentication and authorization middleware to the pipeline

## Configuration Reference

### AuthenticationSettings

The `authentication` configuration section supports these properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `default` | string | auto | The default authentication scheme name |
| `useKerberos` | bool | `false` | Enable Kerberos/Windows authentication |
| `bearerTokens` | array | `[]` | Array of JWT Bearer token configurations |

### JwtBearerTokenSettings

Each item in the `bearerTokens` array supports these properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `provider` | string | required | Unique name for this authentication scheme |
| `authority` | string | required | The URL of the identity provider (e.g., `https://accounts.google.com`) |
| `issuer` | string | required* | Expected token issuer (*required if `validateIssuer` is true) |
| `audience` | string[] | required* | Expected audience/client IDs (*required if `validateAudience` is true) |
| `validateIssuer` | bool | `true` | Validate the token issuer |
| `validateAudience` | bool | `true` | Validate the token audience |
| `validateLifetime` | bool | `true` | Validate the token expiration |
| `requireHttpsMetadata` | bool | `true` | Require HTTPS for metadata endpoints |

## JWT Bearer Authentication

### Basic Configuration

Configure a single JWT Bearer provider in `appsettings.json`:

```json
{
    "authentication": {
        "bearerTokens": [
            {
                "provider": "MyProvider",
                "authority": "https://your-identity-provider.com",
                "issuer": "https://your-identity-provider.com",
                "audience": ["your-client-id"],
                "validateIssuer": true,
                "validateAudience": true,
                "validateLifetime": true
            }
        ]
    }
}
```

### Google Authentication

Configure Google as an identity provider:

```json
{
    "authentication": {
        "bearerTokens": [
            {
                "provider": "Google",
                "authority": "https://accounts.google.com",
                "issuer": "https://accounts.google.com",
                "audience": ["your-google-client-id.apps.googleusercontent.com"],
                "validateIssuer": true,
                "validateAudience": true,
                "validateLifetime": true
            }
        ]
    }
}
```

### Azure AD Authentication

Configure Azure Active Directory:

```json
{
    "authentication": {
        "bearerTokens": [
            {
                "provider": "AzureAD",
                "authority": "https://login.microsoftonline.com/your-tenant-id/v2.0",
                "issuer": "https://login.microsoftonline.com/your-tenant-id/v2.0",
                "audience": ["your-client-id"],
                "validateIssuer": true,
                "validateAudience": true,
                "validateLifetime": true
            }
        ]
    }
}
```

### Auth0 Authentication

Configure Auth0:

```json
{
    "authentication": {
        "bearerTokens": [
            {
                "provider": "Auth0",
                "authority": "https://your-domain.auth0.com/",
                "issuer": "https://your-domain.auth0.com/",
                "audience": ["your-api-identifier"],
                "validateIssuer": true,
                "validateAudience": true,
                "validateLifetime": true
            }
        ]
    }
}
```

### Development Configuration

For development, you may want to relax some validation:

```json
{
    "authentication": {
        "bearerTokens": [
            {
                "provider": "Dev",
                "authority": "https://localhost:5001",
                "issuer": "https://localhost:5001",
                "audience": ["my-api"],
                "validateIssuer": true,
                "validateAudience": true,
                "validateLifetime": true,
                "requireHttpsMetadata": false
            }
        ]
    }
}
```

## Kerberos/Windows Authentication

### Basic Configuration

Enable Kerberos authentication:

```json
{
    "authentication": {
        "useKerberos": true
    }
}
```

When only Kerberos is configured, it becomes the default authentication scheme automatically.

### Protecting Endpoints with Windows Auth

Use the `Negotiate` authentication scheme:

```csharp
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
[ApiController]
public class WindowsAuthController : ControllerBase {
    [HttpGet]
    public string Get() {
        return $"Hello, {User.Identity?.Name}";
    }
}
```

## Multiple Authentication Schemes

### Configuration

Configure multiple authentication providers:

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
                "audience": ["your-google-client-id.apps.googleusercontent.com"]
            },
            {
                "provider": "AzureAD",
                "authority": "https://login.microsoftonline.com/your-tenant-id/v2.0",
                "issuer": "https://login.microsoftonline.com/your-tenant-id/v2.0",
                "audience": ["your-azure-client-id"]
            }
        ]
    }
}
```

### Default Scheme Selection

The default authentication scheme is determined as follows:

1. If `default` is explicitly set, use that scheme
2. If only Kerberos is enabled (no bearer tokens), use Negotiate
3. If only one bearer token is configured, use that provider
4. Otherwise, no default is set (must specify scheme per endpoint)

### Protecting Endpoints by Scheme

Specify which authentication scheme to use for each endpoint:

```csharp
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase {
    // Uses the default scheme
    [Authorize]
    [HttpGet("default")]
    public string Default() => $"Authenticated via default: {User.Identity?.Name}";

    // Requires Google authentication
    [Authorize(AuthenticationSchemes = "Google")]
    [HttpGet("google")]
    public string Google() => $"Authenticated via Google: {User.Identity?.Name}";

    // Requires Azure AD authentication
    [Authorize(AuthenticationSchemes = "AzureAD")]
    [HttpGet("azure")]
    public string AzureAD() => $"Authenticated via Azure AD: {User.Identity?.Name}";

    // Requires Windows authentication
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    [HttpGet("windows")]
    public string Windows() => $"Authenticated via Windows: {User.Identity?.Name}";

    // Accepts multiple schemes
    [Authorize(AuthenticationSchemes = "Google,AzureAD")]
    [HttpGet("multi")]
    public string Multi() => $"Authenticated: {User.Identity?.Name}";
}
```

## Accessing User Information

### Using Built-in Services

Albatross.Hosting provides services for accessing the current user:

```csharp
using Albatross.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class UserController : ControllerBase {
    private readonly IGetCurrentLogin _getCurrentLogin;
    private readonly IGetCurrentUser _getCurrentUser;

    public UserController(IGetCurrentLogin getCurrentLogin, IGetCurrentUser getCurrentUser) {
        _getCurrentLogin = getCurrentLogin;
        _getCurrentUser = getCurrentUser;
    }

    [HttpGet("login")]
    public ILogin? GetLogin() => _getCurrentLogin.Get();

    [HttpGet("username")]
    public string GetUsername() => _getCurrentUser.Get();
}
```

### Accessing Claims

Access user claims directly from the HttpContext:

```csharp
[HttpGet("claims")]
public IEnumerable<string> GetClaims() {
    return HttpContext.User?.Claims?
        .Select(c => $"{c.Type}: {c.Value}")
        ?? Enumerable.Empty<string>();
}
```

### Common JWT Claims

| Claim Type | Description |
|------------|-------------|
| `sub` | Subject (user ID) |
| `email` | User's email address |
| `name` | User's display name |
| `iat` | Issued at (timestamp) |
| `exp` | Expiration (timestamp) |
| `aud` | Audience |
| `iss` | Issuer |

## Custom Authorization Policies

### Defining Policies

Override `ConfigureAuthorization` in your Startup class to define custom policies:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    protected override void ConfigureAuthorization(AuthorizationOptions options) {
        // Require specific claim
        options.AddPolicy("RequireEmail", policy =>
            policy.RequireClaim("email"));

        // Require specific domain
        options.AddPolicy("RequireCompanyEmail", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    c.Type == "email" &&
                    c.Value.EndsWith("@company.com"))));

        // Require specific role
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));
    }
}
```

### Using Policies

Apply policies to controllers or actions:

```csharp
[Authorize(Policy = "RequireCompanyEmail")]
[HttpGet("internal")]
public string InternalEndpoint() => "For company employees only";

[Authorize(Policy = "AdminOnly")]
[HttpGet("admin")]
public string AdminEndpoint() => "For admins only";
```

## Swagger Integration

When JWT Bearer authentication is configured, Albatross.Hosting automatically adds security definitions to Swagger/OpenAPI documentation, allowing you to test authenticated endpoints directly from the Swagger UI.

### Using Swagger with Authentication

1. Navigate to the Swagger UI at `/swagger`
2. Click the "Authorize" button
3. Enter your JWT token (without the "Bearer" prefix)
4. Click "Authorize" to apply the token to all requests

## Environment-Specific Configuration

Use environment-specific configuration files for different authentication settings:

**appsettings.Development.json:**
```json
{
    "authentication": {
        "bearerTokens": [
            {
                "provider": "Dev",
                "authority": "https://localhost:5001",
                "requireHttpsMetadata": false
            }
        ]
    }
}
```

**appsettings.Production.json:**
```json
{
    "authentication": {
        "bearerTokens": [
            {
                "provider": "Production",
                "authority": "https://auth.company.com",
                "requireHttpsMetadata": true
            }
        ]
    }
}
```

## Sample Project

See the [Sample.WebApi](https://github.com/RushuiGuan/hosting/tree/main/Sample.WebApi) project for working examples:

- [LoginByGoogleController.cs](https://github.com/RushuiGuan/hosting/blob/main/Sample.WebApi/Controllers/LoginByGoogleController.cs) - JWT Bearer authentication with Google
- [LoginByWindowsController.cs](https://github.com/RushuiGuan/hosting/blob/main/Sample.WebApi/Controllers/LoginByWindowsController.cs) - Kerberos/Windows authentication
- [appsettings.json](https://github.com/RushuiGuan/hosting/blob/main/Sample.WebApi/appsettings.json) - Authentication configuration example

## Troubleshooting

### Token Validation Failed

Check that:
1. The `authority` URL is correct and accessible
2. The `issuer` matches the token's `iss` claim exactly
3. The `audience` includes the token's `aud` claim
4. The token has not expired (if `validateLifetime` is true)

### HTTPS Metadata Error

In development, set `requireHttpsMetadata: false` if your identity provider doesn't use HTTPS.

### No Default Scheme

If you have multiple authentication schemes without a default, you must specify the scheme on each `[Authorize]` attribute:

```csharp
[Authorize(AuthenticationSchemes = "YourScheme")]
```

### Kerberos Not Working

Ensure:
1. The server is joined to the domain
2. SPNs are configured correctly
3. The client browser supports Negotiate authentication
