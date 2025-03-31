using System;
using System.Text.Json;

namespace Albatross.Hosting.ExceptionHandling {
	public class ExceptionHandlerSerializationOptions {
		static readonly Lazy<JsonSerializerOptions> lazy = new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
		});
		public static JsonSerializerOptions Value => lazy.Value;
	}
}
