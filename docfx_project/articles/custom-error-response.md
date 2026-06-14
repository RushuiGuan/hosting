# Error Handling and Logging

`Albatross.Hosting` registers a global exception handler that acts as a fallback for unhandled exceptions, converting them into consistent [RFC 7807](https://tools.ietf.org/html/rfc7807) `ProblemDetails` responses. It is a safety net only — expected error conditions should be handled explicitly in controller actions with `ActionResult`. The global handler also controls two cross-cutting concerns: which exceptions are logged, and how much detail is returned to the caller.

## Handling Errors with ActionResult

Handle known errors explicitly by returning an `ActionResult`. This makes the error response intentional, self-documenting, and gives full control over the status code and body.

```csharp
[HttpGet("{id}")]
public ActionResult<Item> GetItem(int id) {
    if (id <= 0) {
        return BadRequest(new { message = "Id must be positive" });
    }
    var item = _repository.Find(id);
    if (item == null) {
        return NotFound(new { message = $"Item {id} not found" });
    }
    return item;
}
```

## Global Exception Handler (Fallback)

The handler catches any unhandled exception that propagates out of a controller action. It is wired into ASP.NET Core's `IApplicationBuilder.UseExceptionHandler` middleware. When an unhandled exception occurs:

1. The `ExceptionHandlerMiddleware` catches the exception.
2. `GlobalExceptionHandler.Handle` is invoked.
3. The handler maps the exception to a status code, builds a `ProblemDetails`, and writes it to the response with content type `application/problem+json`.

If the response has already started, the handler does nothing — the status code and body can no longer be changed at that point, and the framework logs the exception regardless.

### Status Code Mapping

The exception type determines the HTTP status code via `GlobalExceptionHandler.GetStatusCode`:

| Exception Type | Status Code |
|----------------|-------------|
| `NotFoundException` | 404 Not Found |
| `ArgumentException` (and derived), `System.ComponentModel.DataAnnotations.ValidationException`, `FluentValidation.ValidationException` | 400 Bad Request |
| `AuthenticationException` | 401 Unauthorized |
| `UnauthorizedAccessException` | 403 Forbidden |
| `ConflictException` | 409 Conflict |
| All other exceptions | 500 Internal Server Error |

> Note on naming: HTTP 401 is really *unauthenticated* and 403 is *unauthorized*. `AuthenticationException` (failure to authenticate) maps to 401, while `UnauthorizedAccessException` (insufficient permission) maps to 403.

Exceptions mapped to a 4xx status are referred to below as **known** exceptions; an exception mapped to 500 is an **unexpected** server error.

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

The `detail` field is populated differently for known and unexpected errors:

- **Known (4xx) exceptions** — `detail` is always the `exception.Message`. These messages are intentional and part of the API contract (validation failures, "not found", conflict reasons), so the client always receives them.
- **Unexpected (500) errors** — the message is uncontrolled and may contain internals (SQL, file paths, connection strings). Its detail depends on the `MaskExceptionDetail` property:

| `MaskExceptionDetail` | 500 `detail` value |
|-----------------------|--------------------|
| `false` | `{ExceptionType.FullName}: {message}` — full diagnostics |
| `true` | Generic message: `"An unexpected error occurred. Please provide the trace id to support for assistance."` |

Either way the full exception is written to the server log, so masking the response never loses diagnostic information — use the `traceId` to find the logged entry.

Override `MaskExceptionDetail` on your `Startup` to choose the posture. Enable masking for applications with high security requirements; leave it off for internal applications where the detail aids diagnostics:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    // hide internal 500 detail from callers
    protected override bool MaskExceptionDetail => true;
}
```

## Logging Behavior

`ExceptionHandlerMiddleware` logs every exception it handles as an error. Because `Albatross.Hosting` wires the handler as an `ExceptionHandler` delegate (rather than a DI-registered `IExceptionHandler`), the .NET 10 default of suppressing diagnostics for handled exceptions does **not** apply — without intervention, even 4xx client errors would be logged as server errors and create noise.

To control this, `Startup` configures the middleware's `SuppressDiagnosticsCallback` from the `SupressLoggingOfKnowExceptions` property:

| `SupressLoggingOfKnowExceptions` | Logging behavior |
|----------------------------------|------------------|
| `true` | Only unexpected (500) errors are logged; known 4xx exceptions are suppressed |
| `false` | All handled exceptions are logged (framework default) |

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    // keep the log focused on real server errors
    protected override bool SupressLoggingOfKnowExceptions => true;
}
```

> Unhandled exceptions and exceptions thrown after the response has started are always logged by the framework, regardless of this setting.

For per-request logging (method, path, status, elapsed time) see [Http Request Logging](request-logging.md).

## Best Practices

| Scenario | Recommended Approach |
|----------|---------------------|
| Known error conditions | Return `ActionResult` |
| Validation errors | Return `BadRequest()` or throw a validation exception (→ 400) |
| Not found errors | Return `NotFound()` or throw `NotFoundException` (→ 404) |
| Unexpected errors | Let exceptions propagate (fallback returns 500) |

### Avoid Anti-Patterns

1. **Don't rely on the global handler for expected errors** — handle them explicitly with `ActionResult`.
2. **Don't use exceptions for flow control** — exceptions should be exceptional.
3. **Don't expose sensitive information** — enable `MaskExceptionDetail` when 500 messages may contain internals.

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
