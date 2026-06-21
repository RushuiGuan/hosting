using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Albatross.Hosting {
	public interface IConfigureAuthentication {
    		bool HasAnyAuthentication { get; }
    		bool UseAnyBearerToken { get; }
    		bool Configure(IServiceCollection services);
    	}
	
	public class AuthenticationConfigurator : IConfigureAuthentication {
		private AuthenticationSettings settings;
		public AuthenticationConfigurator(IConfiguration configuration) {
			settings = new AuthenticationSettings(configuration);
			settings.Validate();
		}
		public bool HasAnyAuthentication => settings.HasAnyAuthentication;
		public bool UseAnyBearerToken => settings.UseAnyBearerToken;

		public bool Configure(IServiceCollection services) {
			if (this.HasAnyAuthentication) {
				var builder = services.AddAuthentication(option => {
					option.DefaultScheme = settings.GetDefault();
				});
				foreach (var token in settings.BearerTokens) {
					builder.AddJwtBearer(token.Provider, token.SetJwtBearerOptions);
				}
				if (settings.UseKerberos) {
					builder.AddNegotiate();
				}
				return true;
			} else {
				return false;
			}
		}
	}
}