using Albatross.Config;
using Microsoft.Extensions.Configuration;

namespace Albatross.Hosting {
	public class AuthenticationSettings : ConfigBase {
		public override string Key => "authentication";
		public AuthenticationSettings(IConfiguration configuration) : base(configuration) {
		}

		public bool UseKerboros { get; set; }
		public JwtBearerTokenSettings[] BearerTokens { get; set; } = [];
		
		public bool HasAny => BearerTokens.Length > 0 || UseKerboros;

		public override void Validate() {
			foreach(var bearerToken in BearerTokens){
				bearerToken.Validate();
			}
		}
	}
}