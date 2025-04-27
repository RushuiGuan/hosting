using System;

namespace Albatross.Hosting {
	public record class JwtBearerTokenSettings {
		public JwtBearerTokenSettings(string provider, string authority){
			this.Provider = provider;
			Authority = authority;
		}

		public string Provider { get; }
		public bool ValidateIssuer { get; set; } = true;
		public bool ValidateAudience { get; set; } = true;
		public bool ValidateLifetime { get; set; } = true;
		public string Authority { get; set; }
		public string? Issuer { get; set; }
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
	}
}