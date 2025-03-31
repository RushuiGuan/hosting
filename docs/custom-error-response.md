# Custom Error Response
## Intent
Allow controller endpoints to create an error response by throwing an unhandled exception.

## Reason
Aspnetcore expects controller endpoints to handle exceptions and create proper error response.  This can be done using the return type of `IActionResult`.  While this approach is preferred, small to medium enterprises often create error response by throwing unhandled exceptions.  Especially when the api is consumed by internal applications and the error response is not highly customized.  Aspnetcore can do this out of box but with limited capability.  `Albatross.Hosting` brings back the deprecated `HttpResponseException` as `HttpApiException` so that end point can throw an exception with specific status code and json content.  It also allows easy configuration of error response based on application exception types.

Please note that this is a quick way of creating an error response that is `good enough` for the consumer.  If a system needs proper error responses for all its endpoints, using `IActionResult` return type is the preferred method because it creates clear intent and the error content code directly at the source.  Doing it using a global error handler by introducing specific application level exceptions would be considered as an *antipattern*.

## How it works

By default `IApplicationBuilder.UseExceptionHandler` method is invoked.  By doing that, unhandled exceptions will be handled by `Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware`.  The middleware allows a custom exception handler which is supplied by `Albatross.Hosting.ExceptionHandling.DefaultGlobalExceptionHandler`.  The default handler can be changed by overwriting the `Startup.GlobalExceptionHandler` property.

## Different Ways of Sending Error Responses
1. Send an `ActionResult`
	
	The exception handler doesnot change the this behavior because there is no unhandled exceptions.

1. Throw an Exception of type `Albatross.Hosting.ExceptionHandling.HttpApiException`
	
	The exception handler will return an error response with the status code specified by the instance of `HttpApiException`.  It would return the content of the `HttpApiException.Error` object as text if it is a string.  Otherwise it would return its serialized json value.

1. Throw an Exception of other types

	* If the exception is of type `ArgumentException`, the default handler will return a 400 error.  Otherwise, the status code would be 500.
	* The content of the response would be a json object of type `Albatross.Hosting.ExceptionHandling.ErrorMessage`.  `ErrorMessage` class is similar to the `Microsoft.AspNetCore.Mvc.ProblemDetails` class.  It is used as default to support the legacy code base.  If the `ProblemDetails` type desired, overwrite the `Startup.GlobalExceptionHandler` property to use `Albatross.Hosting.ExceptionHandling.ProblemDetailsGlobalExceptionHandler`
	* Create your own custom global exception handler to create custom error responses based on application exception.  

1. By default, aspnetcore will log an error for any unhandled exceptions.  On an critical system, all errors could be notified and it might be desirable to filter out the `ArgumentException` error if it is used as indicator of invalid request.  This can be done automatically by setting the `Setup.SupressUnhandledArgumentExceptionLogging` flag to true.

