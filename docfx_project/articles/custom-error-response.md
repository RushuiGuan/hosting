# Custom Error Response

Albatross.Hosting provides a global exception handling system that converts unhandled exceptions into consistent HTTP error responses. This allows controller endpoints to signal errors by throwing exceptions instead of manually constructing error responses.

## Features

- **Global Exception Handling** - Automatically converts unhandled exceptions to HTTP responses
- **HttpApiException** - Throw exceptions with specific status codes and custom error content
- **ProblemDetails Support** - Returns RFC 7807 compliant error responses by default
- **ArgumentException Handling** - Automatically returns 400 Bad Request for argument validation errors
- **Customizable** - Override the default handler to implement custom error response logic
- **Logging Control** - Option to suppress logging for expected exceptions like ArgumentException

## How It Works

Albatross.Hosting uses ASP.NET Core's `IApplicationBuilder.UseExceptionHandler` middleware. When an unhandled exception occurs:

1. The `ExceptionHandlerMiddleware` catches the exception
2. The configured `IGlobalExceptionHandler` converts the exception to an `HttpApiException`
3. The response status code and body are set based on the exception details
4. The response is returned to the client

## Default Error Response Format

The default handler returns errors using the `ProblemDetailsWithTraceId` format:

```json
{
    "status": 500,
    "title": "An error occurred while processing your request",
    "detail": "The exception message",
    "type": "System.InvalidOperationException",
    "traceId": "00-abc123..."
}
```

| Property | Description |
|----------|-------------|
| `status` | HTTP status code |
| `title` | Generic error title |
| `detail` | The exception message |
| `type` | The exception type's full name |
| `traceId` | Request trace identifier for debugging |

## Returning Error Responses

### Method 1: Return ActionResult (Recommended)

For explicit error responses, return an `ActionResult`:

```csharp
[HttpGet("{id}")]
public ActionResult<Item> GetItem(int id) {
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

This approach is preferred because it creates clear intent and documents the error response directly at the source.

### Method 2: Throw HttpApiException

Throw `HttpApiException` to return a specific status code with custom content:

```csharp
using Albatross.Hosting.ExceptionHandling;

[HttpGet("{id}")]
public Item GetItem(int id) {
    var item = _repository.Find(id);
    if (item == null) {
        // Return plain text error
        throw new HttpApiException(404, $"Item {id} not found");
    }
    return item;
}

[HttpPost]
public void CreateItem([FromBody] Item item) {
    if (_repository.Exists(item.Id)) {
        // Return JSON error object
        throw new HttpApiException(409, new {
            code = "DUPLICATE_ITEM",
            message = $"Item {item.Id} already exists",
            itemId = item.Id
        });
    }
    _repository.Add(item);
}
```

#### HttpApiException Constructors

```csharp
// Status code with error object (string or object)
new HttpApiException(int statusCode, object? error)

// Status code with title and error object
new HttpApiException(int statusCode, string title, object? error)
```

#### Response Content Type

- If `error` is a `string`, the response content type is `text/plain`
- If `error` is an object, it's serialized as JSON with content type `application/json`

### Method 3: Throw Standard Exceptions

Throw standard .NET exceptions for automatic conversion:

```csharp
[HttpGet("{id}")]
public Item GetItem(int id) {
    if (id <= 0) {
        // Returns 400 Bad Request
        throw new ArgumentException("Id must be positive", nameof(id));
    }

    var item = _repository.Find(id);
    if (item == null) {
        // Returns 500 Internal Server Error
        throw new InvalidOperationException($"Item {id} not found");
    }
    return item;
}
```

#### Default Status Code Mapping

| Exception Type | Status Code |
|----------------|-------------|
| `ArgumentException` (and derived types) | 400 Bad Request |
| All other exceptions | 500 Internal Server Error |

## Custom Exception Handlers

### Creating a Custom Handler

Create a custom handler by implementing `IGlobalExceptionHandler` or extending `DefaultGlobalExceptionHandler`:

```csharp
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Http;

public class CustomExceptionHandler : DefaultGlobalExceptionHandler {
    public override HttpApiException Convert(HttpContext context, Exception exception) {
        // Handle specific exception types
        if (exception is NotFoundException notFound) {
            return new HttpApiException(404, new {
                error = "NOT_FOUND",
                message = notFound.Message,
                resourceType = notFound.ResourceType,
                resourceId = notFound.ResourceId
            });
        }

        if (exception is ValidationException validation) {
            return new HttpApiException(400, new {
                error = "VALIDATION_ERROR",
                message = validation.Message,
                errors = validation.Errors
            });
        }

        if (exception is UnauthorizedException) {
            return new HttpApiException(401, "Unauthorized");
        }

        // Fall back to default handling
        return base.Convert(context, exception);
    }
}
```

### Registering the Custom Handler

Override the `GlobalExceptionHandler` property in your Startup class:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    protected override IGlobalExceptionHandler GlobalExceptionHandler { get; }
        = new CustomExceptionHandler();
}
```

### Customizing the Error Object

Override `ConvertToObject` to change the error response structure:

```csharp
public class ApiErrorHandler : DefaultGlobalExceptionHandler {
    public override object ConvertToObject(HttpContext context, int statusCode, Exception exception) {
        return new {
            success = false,
            error = new {
                code = exception.GetType().Name,
                message = exception.Message,
                timestamp = DateTime.UtcNow,
                requestId = context.TraceIdentifier
            }
        };
    }
}
```

## Legacy Error Format

For backward compatibility with legacy systems, use `LegacyGlobalExceptionHandler` which returns the `ErrorMessage` format:

```csharp
protected override IGlobalExceptionHandler GlobalExceptionHandler { get; }
    = new LegacyGlobalExceptionHandler();
```

**Legacy ErrorMessage format:**
```json
{
    "statusCode": 500,
    "type": "System.InvalidOperationException",
    "message": "The exception message",
    "innerError": {
        "statusCode": 500,
        "type": "System.Exception",
        "message": "Inner exception message",
        "innerError": null
    }
}
```

> **Note**: `LegacyGlobalExceptionHandler` and `ErrorMessage` are marked as obsolete. Use the default `ProblemDetails` format for new applications.

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

### When to Use Each Approach

| Scenario | Recommended Approach |
|----------|---------------------|
| Known error conditions | Return `ActionResult` |
| Validation errors | Return `BadRequest()` or throw `ArgumentException` |
| Not found errors | Return `NotFound()` or throw `HttpApiException(404, ...)` |
| Custom status codes | Throw `HttpApiException` |
| Unexpected errors | Let exceptions propagate (default 500 response) |

### Avoid Anti-Patterns

1. **Don't create exception types for every error case** - Use `HttpApiException` for ad-hoc errors
2. **Don't use exceptions for flow control** - Exceptions should be exceptional
3. **Don't expose sensitive information** - Be careful what you include in error messages

### Error Response Guidelines

```csharp
// Good: Clear, actionable error message
throw new HttpApiException(400, new {
    code = "INVALID_DATE_RANGE",
    message = "End date must be after start date",
    field = "endDate"
});

// Bad: Vague error message
throw new HttpApiException(400, "Invalid input");

// Bad: Exposing internal details
throw new HttpApiException(500, new {
    stackTrace = exception.StackTrace,
    connectionString = _config.ConnectionString
});
```

## Sample Code

See the [ExceptionTestCaseController](https://github.com/RushuiGuan/hosting/blob/main/Sample.WebApi/Controllers/ExceptionTestCaseController.cs) in the Sample.WebApi project for working examples:

```csharp
// Return ProblemDetails using ControllerBase.Problem()
[HttpGet("problem")]
public ActionResult UseProblem() {
    return Problem(
        detail: "error detail",
        title: "test",
        statusCode: 500,
        type: "ExceptionType"
    );
}

// Throw generic exception (returns 500 with ProblemDetails)
[HttpGet("exception")]
public void ThrowException() {
    throw new Exception("This is a test exception");
}

// Throw HttpApiException with text content
[HttpGet("text-error")]
public void ThrowTextError() {
    throw new HttpApiException(400, "This is a test exception");
}

// Throw HttpApiException with JSON content
[HttpGet("json-error")]
public void ThrowJsonError() {
    throw new HttpApiException(400, new {
        id = 100,
        message = "This is a test exception"
    });
}

// Throw ArgumentException (returns 400)
[HttpGet("argument-error")]
public void ThrowArgumentError() {
    throw new ArgumentException("Invalid parameter value");
}
```
