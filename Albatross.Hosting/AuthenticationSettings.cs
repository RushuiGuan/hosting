using Albatross.Config;
using Microsoft.Extensions.Configuration;

namespace Albatross.Hosting {
	public class AuthenticationSettings : ConfigBase {
		public override string Key => "authentication";
		public AuthenticationSettings(IConfiguration configuration) : base(configuration) {
		}

		public string? Default { get; set; }
		public bool UseKerberos { get; set; }
		public JwtBearerTokenSettings[] BearerTokens { get; set; } = [];
		
		public bool HasAny => BearerTokens.Length > 0 || UseKerberos;

		public override void Validate() {
			foreach(var bearerToken in BearerTokens){
				bearerToken.Validate();
			}
		}
	}
}