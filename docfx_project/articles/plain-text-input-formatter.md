# Plain Text Input Formatter

Albatross.Hosting includes a built-in `PlainTextInputFormatter` that enables your Web API to accept plain text and HTML content in request bodies. By default, ASP.NET Core only accepts JSON-formatted request bodies, requiring clients to send strings as JSON strings (e.g., `"hello world"` with quotes). The plain text input formatter allows clients to send raw text without JSON encoding.

## Features

- **Plain Text Support** - Accept `text/plain` content type in request bodies
- **HTML Support** - Accept `text/html` content type in request bodies
- **Multiple Encodings** - Supports UTF-8 and UTF-16 (Little Endian) character encodings
- **Enabled by Default** - No additional configuration required

## How It Works

The formatter intercepts incoming requests with `Content-Type: text/plain` or `Content-Type: text/html` and reads the request body as a raw string, bypassing JSON deserialization.

### Supported Media Types

| Content-Type | Description |
|--------------|-------------|
| `text/plain` | Plain text content |
| `text/html` | HTML content |

### Supported Encodings

| Encoding | Description |
|----------|-------------|
| UTF-8 (without BOM) | Default encoding for most text content |
| UTF-16 LE | Little Endian UTF-16 encoding |

## Usage

### Basic Example

Create an endpoint that accepts plain text:

```csharp
[Route("api/[controller]")]
[ApiController]
public class TextController : ControllerBase {
    [HttpPost]
    public string Post([FromBody] string content) {
        // content contains the raw text from the request body
        return $"Received: {content}";
    }
}
```

### Client Request

Send a request with plain text content:

```bash
curl -X POST http://localhost:5000/api/text \
  -H "Content-Type: text/plain" \
  -d "Hello, World!"
```

**Response:**
```
Received: Hello, World!
```

### Without Plain Text Formatter

Without the plain text formatter, the same request would require JSON encoding:

```bash
curl -X POST http://localhost:5000/api/text \
  -H "Content-Type: application/json" \
  -d '"Hello, World!"'
```

Note the quotes around the string in the JSON request body.

## Configuration

### Enabled by Default

The plain text input formatter is enabled by default in Albatross.Hosting. No additional configuration is required.

### Disabling the Formatter

To disable the plain text input formatter, override the `PlainTextFormatter` property in your Startup class:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    // Disable plain text input formatter
    public override bool PlainTextFormatter => false;
}
```

## Use Cases

### Accepting Raw Text Input

Useful when clients need to submit unstructured text data:

```csharp
[HttpPost("notes")]
public IActionResult SaveNote([FromBody] string note) {
    // Save the raw text note
    _noteService.Save(note);
    return Ok();
}
```

### Processing HTML Content

Accept HTML content for processing or storage:

```csharp
[HttpPost("html")]
public IActionResult ProcessHtml([FromBody] string html) {
    // Sanitize and process HTML content
    var sanitized = _htmlSanitizer.Sanitize(html);
    return Ok(new { processed = sanitized });
}
```

**Request:**
```bash
curl -X POST http://localhost:5000/api/html \
  -H "Content-Type: text/html" \
  -d "<h1>Hello</h1><p>World</p>"
```

### Text Processing Endpoints

Build endpoints that transform or analyze text:

```csharp
[HttpPost("uppercase")]
public string ToUpperCase([FromBody] string text) {
    return text.ToUpperInvariant();
}

[HttpPost("wordcount")]
public int WordCount([FromBody] string text) {
    return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}
```

### Webhook Receivers

Accept webhook payloads that may come as plain text:

```csharp
[HttpPost("webhook")]
public IActionResult ReceiveWebhook([FromBody] string payload) {
    _logger.LogInformation("Received webhook: {payload}", payload);
    // Process the webhook payload
    return Ok();
}
```

## Combining with JSON

You can have endpoints that accept both plain text and JSON by using multiple actions or content negotiation:

```csharp
[Route("api/[controller]")]
[ApiController]
public class DataController : ControllerBase {
    // Accepts plain text
    [HttpPost("text")]
    public IActionResult PostText([FromBody] string content) {
        return Ok(new { source = "text", content });
    }

    // Accepts JSON
    [HttpPost("json")]
    public IActionResult PostJson([FromBody] DataModel model) {
        return Ok(new { source = "json", model });
    }
}
```

## Limitations

- **String Type Only** - The formatter only works with `string` parameters. Complex types still require JSON serialization.
- **Input Only** - This is an input formatter. Response formatting is handled separately by ASP.NET Core's content negotiation.

## Sample Code

See the [Sample.WebApi](https://github.com/RushuiGuan/hosting/tree/main/Sample.WebApi) project for working examples, including:

```csharp
[HttpPost("text-input-output")]
public string TextInputAndOutput([FromBody] string input) {
    logger.LogInformation("input: {input}", input);
    return input;
}
```
