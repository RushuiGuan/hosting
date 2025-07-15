using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using System;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class SchemaController : ControllerBase {

		[HttpGet("angular-config")]	
		public string AngularConfig() {
			return GetSchema(typeof(Albatross.Hosting.AngularConfig));
		}

		[HttpGet("authentication-settings")]
		public string AuthenticationSettings() {
			return GetSchema(typeof(Albatross.Hosting.AuthenticationSettings));
		}


		string GetSchema(Type type) {
			var settings = new NJsonSchema.Generation.SystemTextJsonSchemaGeneratorSettings() {
				FlattenInheritanceHierarchy = true,
				SerializerOptions = new System.Text.Json.JsonSerializerOptions {
					PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
					WriteIndented = true,
				},
			};
			var schema = NJsonSchema.Generation.JsonSchemaGenerator.FromType(type, settings);
			schema.Properties.Add("$schema", new JsonSchemaProperty {
				Type = JsonObjectType.String,
			});
			return schema.ToJson();
		}
	}
}
