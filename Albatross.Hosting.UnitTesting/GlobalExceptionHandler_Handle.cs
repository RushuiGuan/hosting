using Albatross.Exceptions;
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Xunit;

namespace Albatross.Hosting.UnitTesting {
	public class GlobalExceptionHandler_Handle {
		const string TraceId = "trace-1";

		static ValidationError[] Errors => [
			new("name", "Name is missing"),
			new("", "Request is inconsistent"),
		];

		static async Task<(HttpContext context, JsonElement body)> Handle(Exception exception, bool mask) {
			var context = new DefaultHttpContext { TraceIdentifier = TraceId };
			var stream = new MemoryStream();
			context.Response.Body = stream;
			context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature { Error = exception, Path = "/" });
			await new GlobalExceptionHandler(mask).Handle(context);
			using var document = JsonDocument.Parse(stream.ToArray());
			return (context, document.RootElement.Clone());
		}

		[Theory]
		[MemberData(nameof(StatusCodeCases.All), MemberType = typeof(StatusCodeCases))]
		public async Task ResponseAndBodyCarryTheSameStatus(Exception exception, int expected) {
			var (context, body) = await Handle(exception, false);
			Assert.Equal(expected, context.Response.StatusCode);
			Assert.Equal(expected, body.GetProperty("status").GetInt32());
			Assert.Equal("application/problem+json", context.Response.ContentType);
			Assert.Equal(GlobalExceptionHandler.Title, body.GetProperty("title").GetString());
			Assert.Equal(TraceId, body.GetProperty("traceId").GetString());
		}

		public static TheoryData<Exception, bool, string?> Details => new() {
			{ new NotFoundException("Item 42 not found"), false, "Item 42 not found" },
			{ new NotFoundException("Item 42 not found"), true, null },
			{ new Exception("Connection string 'x' is invalid"), false, "Connection string 'x' is invalid" },
			{ new Exception("Connection string 'x' is invalid"), true, null },
		};

		[Theory]
		[MemberData(nameof(Details))]
		public async Task DetailMasking(Exception exception, bool mask, string? expected) {
			var (_, body) = await Handle(exception, mask);
			if (expected == null) {
				Assert.False(body.TryGetProperty("detail", out _));
			} else {
				Assert.Equal(expected, body.GetProperty("detail").GetString());
			}
		}

		public static TheoryData<Exception, bool> ValidationMessages => new() {
			{ new ValidationException("Cen code is missing"), false },
			{ new ValidationException("Cen code is missing"), true },
			{ new System.ComponentModel.DataAnnotations.ValidationException("Cen code is missing"), false },
			{ new System.ComponentModel.DataAnnotations.ValidationException("Cen code is missing"), true },
		};

		[Theory]
		[MemberData(nameof(ValidationMessages))]
		public async Task ValidationMessage_NotMasked(Exception exception, bool mask) {
			var (_, body) = await Handle(exception, mask);
			Assert.Equal("Cen code is missing", body.GetProperty("detail").GetString());
		}

		// the body is written as its runtime type; written as ProblemDetails, the errors would be dropped
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ValidationErrors_WrittenAsFieldErrors(bool mask) {
			var (context, body) = await Handle(new FieldValidationException(Errors), mask);
			Assert.Equal(422, context.Response.StatusCode);
			var errors = body.GetProperty("errors");
			Assert.Equal("Name is missing", errors.GetProperty("name")[0].GetString());
			Assert.Equal("Request is inconsistent", errors.GetProperty("")[0].GetString());
			Assert.Equal(TraceId, body.GetProperty("traceId").GetString());
		}

		[Fact]
		public async Task NoException_WritesNothing() {
			var context = new DefaultHttpContext();
			var stream = new MemoryStream();
			context.Response.Body = stream;
			await new GlobalExceptionHandler(false).Handle(context);
			Assert.Equal(200, context.Response.StatusCode);
			Assert.Equal(0, stream.Length);
		}
	}
}
