using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Albatross.Hosting {
	public record class JwtBearerTokenSettings {
		public string Provider { get; set; } = string.Empty;
		public string Authority { get; set; } = string.Empty;
		public bool ValidateIssuer { get; set; } = true;
		public bool ValidateAudience { get; set; } = true;
		public bool ValidateLifetime { get; set; } = true;
		public string? Issuer { get; set; }
		public bool RequireHttpsMetadata { get; set; } = true;
		/// <summary>
		/// AKA ClientId
		/// </summary>
		public string[] Audience { get; set; } = [];

		public void Validate() {
			if (string.IsNullOrEmpty(Authority)) {
				throw new ArgumentException("Authority is required");
			}
			if (ValidateIssuer && string.IsNullOrEmpty(Issuer)) {
				throw new ArgumentException("Issuer is required");
			}
			if (ValidateAudience && Audience.Length == 0) {
				throw new ArgumentException("Audience is required");
			}
		}

		public void SetJwtBearerOptions(JwtBearerOptions options) {
			options.Authority = Authority;
			options.IncludeErrorDetails = true;
			options.RequireHttpsMetadata = RequireHttpsMetadata;
			options.TokenValidationParameters = new TokenValidationParameters {
				ValidateIssuer = ValidateIssuer,
				ValidIssuer = Issuer,
				ValidateAudience = ValidateAudience,
				ValidAudience = Audience.Length == 1 ? Audience[0] : null,
				ValidAudiences = Audience?.Length > 1 ? Audience : null,
				ValidateLifetime = ValidateLifetime,
			};
		}
	}
}