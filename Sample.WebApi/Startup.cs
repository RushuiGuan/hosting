using Microsoft.Extensions.Configuration;

namespace Sample.WebApi {
	public class MyStartup : Albatross.Hosting.Startup {
		public MyStartup(IConfiguration configuration) : base(configuration) { }
	}
}