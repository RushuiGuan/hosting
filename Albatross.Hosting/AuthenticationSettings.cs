using Albatross.Config;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.Configuration;

namespace Albatross.Hosting {
	public class AuthenticationSettings : ConfigBase {
		public AuthenticationSettings(IConfiguration configuration) : base(configuration, "authentication") { }

		public string? Default { get; set; }
		public bool UseKerberos { get; set; }
		public JwtBearerTokenSettings[] BearerTokens { get; set; } = [];

		public bool HasAny => BearerTokens.Length > 0 || UseKerberos;

		public override void Validate() {
			foreach (var bearerToken in BearerTokens) {
				bearerToken.Validate();
			}
		}

		public string? DefaultAuthenticationScheme {
			get {
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
}