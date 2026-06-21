using Albatross.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Albatross.Hosting {
	public class AuthenticationSettings : ConfigBase {
		public AuthenticationSettings(IConfiguration configuration) : base(configuration, "authentication") { }

		public string? Default { get; set; }
		public bool UseKerberos { get; set; }
		public JwtBearerTokenSettings[] BearerTokens { get; set; } = Array.Empty<JwtBearerTokenSettings>();

		public bool HasAnyAuthentication => BearerTokens.Length > 0 || UseKerberos;
		public bool UseAnyBearerToken => BearerTokens.Length > 0;

		public override void Validate() {
			foreach (var bearerToken in BearerTokens) {
				bearerToken.Validate();
			}
		}
		public string? GetDefault() {
			if (!string.IsNullOrEmpty(Default)) {
				return Default;
			} else if (UseKerberos) {
				if (BearerTokens.Length == 0) {
					return NegotiateDefaults.AuthenticationScheme;
				}
			} else if (BearerTokens.Length == 1) {
				return BearerTokens[0].Provider;
			}
			return null;
		}
	}
}