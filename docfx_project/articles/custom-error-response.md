# Custom Error Response

Albatross.Hosting includes a global exception handler that serves as a fallback for unhandled exceptions. It converts them into consistent `ProblemDetails` HTTP error responses. Errors should be handled explicitly in controller actions using `ActionResult` — the global handler exists only as a safety net for unexpected failures.

## Features

- **Global Exception Handling** - Fallback handler converts unhandled exceptions to `ProblemDetails` responses
- **ProblemDetails Support** - Returns RFC 7807 compliant error responses with `application/problem+json` content type
- **ArgumentException Handling** - Automatically returns 400 Bad Request for argument validation errors
- **Logging Control** - Option to suppress logging for expected exceptions like ArgumentException

## Handling Errors with ActionResult

Errors should be handled explicitly by returning an `ActionResult`. This makes the error response intentional, self-documenting, and gives full control over the status code and response body.

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

[HttpPost]
public ActionResult CreateItem([FromBody] Item item) {
    if (!ModelState.IsValid) {
        return BadRequest(ModelState);
    }
    // Or use the Problem method for ProblemDetails format
    return Problem(
        detail: "Validation failed",
        statusCode: 400,
        title: "Bad Request"
    );
}
```

## Global Exception Handler (Fallback)

The global exception handler is a fallback that catches any unhandled exception that propagates out of a controller action. It is not intended as a primary error handling mechanism.

Albatross.Hosting uses ASP.NET Core's `IApplicationBuilder.UseExceptionHandler` middleware. When an unhandled exception occurs:

1. The `ExceptionHandlerMiddleware` catches the exception
2. The `GlobalExceptionHandler.Handle` method is invoked
3. The handler writes a `ProblemDetails` response to the `HttpContext`

### Fallback Response Format

The fallback handler returns errors using the standard `ProblemDetails` format with content type `application/problem+json`:

```json
{
    "status": 500,
    "title": "An error occurred while processing your request",
    "detail": "System.InvalidOperationException: The exception message",
    "traceId": "00-abc123..."
}
```

| Property | Description |
|----------|-------------|
| `status` | HTTP status code |
| `title` | Generic error title |
| `detail` | The exception type's full name followed by the exception message |
| `traceId` | Request trace identifier for debugging |

### Status Code Mapping

| Exception Type | Status Code |
|----------------|-------------|
| `ArgumentException` (and derived types) | 400 Bad Request |
| All other exceptions | 500 Internal Server Error |

## Suppressing ArgumentException Logging

By default, ASP.NET Core logs all unhandled exceptions as errors. For public-facing APIs where `ArgumentException` indicates invalid client input rather than a server error, you can suppress this logging:

```csharp
public class Program {
    public static Task Main(string[] args) {
        return new Setup(args, supressUnhandledArgumentExceptionLogging: true)
            .ConfigureWebHost<MyStartup>()
            .RunAsync();
    }
}
```

This prevents `ArgumentException` from being logged as errors, reducing noise in your logs while still returning appropriate 400 responses to clients.

## Best Practices

| Scenario | Recommended Approach |
|----------|---------------------|
| Known error conditions | Return `ActionResult` |
| Validation errors | Return `BadRequest()` |
| Not found errors | Return `NotFound()` |
| Unexpected errors | Let exceptions propagate (fallback returns 500) |

### Avoid Anti-Patterns

1. **Don't rely on the global handler for expected errors** - Handle errors explicitly with `ActionResult`
2. **Don't use exceptions for flow control** - Exceptions should be exceptional
3. **Don't expose sensitive information** - Be careful what you include in error messages

## Sample Code

See the [ExceptionTestCaseController](https://github.com/RushuiGuan/hosting/blob/main/Sample.WebApi/Controllers/ExceptionTestCaseController.cs) in the Sample.WebApi project for working examples:

```csharp
// Return ProblemDetails using ControllerBase.Problem()
[HttpGet("send-via-controllerBase.problem-method")]
public ActionResult UseProblem() {
    return Problem(
        detail: "error detail",
        title: "test",
        statusCode: 500,
        type: "ExceptionType"
    );
}

// Throw generic exception (fallback returns 500 with ProblemDetails)
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
