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
		/// <summary>
		/// AKA ClientId
		/// </summary>
		public string? Audience { get; set; }

		public void Validate() {
			if (string.IsNullOrEmpty(Authority)) {
				throw new ArgumentException("Authority is required");
			}
			if (ValidateIssuer && string.IsNullOrEmpty(Issuer)) {
				throw new ArgumentException("Issuer is required");
			}
			if (ValidateAudience && string.IsNullOrEmpty(Audience)) {
				throw new ArgumentException("Audience is required");
			}
		}

		public void SetJwtBearerOptions(JwtBearerOptions options) {
			options.Authority = Authority;
			options.IncludeErrorDetails = true;
			options.TokenValidationParameters = new TokenValidationParameters {
				ValidateIssuer = ValidateIssuer,
				ValidIssuer = Issuer,
				ValidateAudience = ValidateAudience,
				ValidAudience = Audience,
				ValidateLifetime = ValidateLifetime,
			};
		}
	}
}