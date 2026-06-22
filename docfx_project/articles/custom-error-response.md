# Error Handling and Logging

`Albatross.Hosting` provides two layers of exception handling:

1. **`IExceptionHandler`** — an injectable service for controller-level exception handling. Controllers catch exceptions from services and repositories, then delegate to `IExceptionHandler.Handle` to produce a consistent `ActionResult`. This is the primary error handling mechanism.
2. **`GlobalExceptionHandler`** — a middleware-level fallback that catches any unhandled exception and converts it into an [RFC 7807](https://tools.ietf.org/html/rfc7807) `ProblemDetails` response. This is a safety net only.

Both layers share the same exception-to-status-code mapping and detail masking behavior.

## Controller-Level Exception Handling

Inject `IExceptionHandler` into your controller and use it to handle exceptions from service/repository calls. This keeps error handling explicit and gives you control over the response:

```csharp
[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase {
    private readonly ICompanyRepository companyRepository;
    private readonly IExceptionHandler exceptionHandler;

    public CompanyController(ICompanyRepository companyRepository, IExceptionHandler exceptionHandler) {
        this.companyRepository = companyRepository;
        this.exceptionHandler = exceptionHandler;
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(CreateCompanyRequest request, CancellationToken cancellationToken) {
        try {
            var company = await companyService.Create(request.Name, cancellationToken);
            await companyRepository.SaveChangesAsync(cancellationToken);
            return new OkObjectResult(company.CreateDto());
        } catch (Exception err) {
            return exceptionHandler.Handle(err, null);
        }
    }
}
```

The `Handle` method accepts an optional custom handler (`Func<Exception, bool, ActionResult?>`) for edge cases where the default mapping doesn't fit. Return `null` from the custom handler to fall through to the default behavior:

```csharp
return exceptionHandler.Handle(err, (ex, showDetail) => {
    if (ex is DbUpdateException dbEx && IsSpecificConstraint(dbEx)) {
        var detail = showDetail ? ex.Message : null;
        return new ConflictObjectResult(new ProblemDetails { Detail = detail });
    }
    return null; // fall through to default handling
});
```

### Logging

`DefaultExceptionHandler` logs every exception it handles:
- **4xx exceptions** — logged at `Warning` level
- **500 exceptions** — logged at `Error` level

This ensures that exceptions caught at the controller level are never silently swallowed.

## Global Exception Handler (Fallback)

The `GlobalExceptionHandler` catches any unhandled exception that propagates out of a controller action. It is wired into ASP.NET Core's `IApplicationBuilder.UseExceptionHandler` middleware. When an unhandled exception occurs:

1. The `ExceptionHandlerMiddleware` catches the exception.
2. `GlobalExceptionHandler.Handle` is invoked.
3. The handler maps the exception to a status code, builds a `ProblemDetails`, and writes it to the response with content type `application/problem+json`.

If the response has already started, the handler does nothing — the status code and body can no longer be changed at that point, and the framework logs the exception regardless.

## Status Code Mapping

Both `IExceptionHandler` and `GlobalExceptionHandler` use the same exception-to-status-code mapping:

| Exception Type | Status Code |
|----------------|-------------|
| `NotFoundException` | 404 Not Found |
| `ConflictException` | 409 Conflict |
| `ValidationException`, `FluentValidation.ValidationException`, `System.ComponentModel.DataAnnotations.ValidationException` | 422 Unprocessable Entity |
| `ArgumentException` | 400 Bad Request |
| `NotSupportedException` | 501 Not Implemented |
| `NotAuthenticatedException`, `AuthenticationException` | 401 Unauthorized |
| `ForbiddenException`, `AccessViolationException`, `UnauthorizedAccessException` | 403 Forbidden |
| `PreconditionFailedException` | 412 Precondition Failed |
| `TimeoutException` | 408 Request Timeout |
| All other exceptions | 500 Internal Server Error |

### Response Format

```json
{
    "status": 404,
    "title": "An error occurred while processing your request",
    "detail": "Item 42 not found",
    "traceId": "00-abc123..."
}
```

| Property | Description |
|----------|-------------|
| `status` | HTTP status code from the mapping above |
| `title` | Generic error title |
| `detail` | Exception detail — see [Masking Error Detail](#masking-error-detail) |
| `traceId` | Request trace identifier; quote it when correlating a response to a server log entry |

## Masking Error Detail

When `MaskExceptionDetail` is `true`, the `detail` field is set to `null` for all error responses. This prevents leaking internal information (SQL, file paths, connection strings) to the caller. When `false`, `detail` contains the exception message.

| `MaskExceptionDetail` | `detail` value |
|-----------------------|----------------|
| `false` | `exception.Message` — full diagnostics |
| `true` | `null` — no detail returned |

The full exception is always written to the server log, so masking the response never loses diagnostic information — use the `traceId` to find the logged entry.

Set `MaskExceptionDetail` on your `Startup` subclass. Enable masking for applications with high security requirements; leave it off for internal applications where the detail aids diagnostics:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) {
        MaskExceptionDetail = true;
    }
}
```

## Logging Behavior

`ExceptionHandlerMiddleware` logs every exception it handles as an error. Because `Albatross.Hosting` wires the handler as an `ExceptionHandler` delegate (rather than a DI-registered `IExceptionHandler`), the .NET 10 default of suppressing diagnostics for handled exceptions does **not** apply — without intervention, even 4xx client errors would be logged as server errors and create noise.

To control this, `Startup` configures the middleware's `SuppressDiagnosticsCallback` from the `SuppressLoggingOfKnownExceptions` property:

| `SuppressLoggingOfKnownExceptions` | Logging behavior |
|-------------------------------------|------------------|
| `true` | Only unexpected (500) errors are logged; known 4xx exceptions are suppressed |
| `false` | All handled exceptions are logged (framework default) |

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) {
        SuppressLoggingOfKnownExceptions = true;
    }
}
```

> Unhandled exceptions and exceptions thrown after the response has started are always logged by the framework, regardless of this setting.

For per-request logging (method, path, status, elapsed time) see [Http Request Logging](request-logging.md).

## Best Practices

| Scenario | Recommended Approach |
|----------|---------------------|
| Known error conditions | Catch and handle via `IExceptionHandler` |
| Validation errors | Throw `ValidationException` (→ 422) or `ArgumentException` (→ 400) |
| Not found errors | Throw `NotFoundException` (→ 404) |
| Edge cases | Use the custom handler callback on `IExceptionHandler.Handle` |
| Unexpected errors | Let exceptions propagate to the global fallback (→ 500) |

## Sample Code

See the [ExceptionTestCaseController](https://github.com/RushuiGuan/hosting/blob/main/Sample.WebApi/Controllers/ExceptionTestCaseController.cs) in the Sample.WebApi project for working examples:

```csharp
// Throw a generic exception (fallback returns 500 with ProblemDetails)
[HttpGet("send-by-throwing-exception")]
public void ThrowException() {
    throw new Exception("This is a test exception");
}

// Throw ArgumentException (fallback returns 400 with ProblemDetails)
[HttpGet("send-by-throwing-argument-exception")]
public void ThrowArgumentException() {
    throw new ArgumentException("This is a test exception");
}
```
