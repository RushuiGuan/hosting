using Microsoft.Extensions.Configuration;

namespace Sample.WebApi {
	public class MyStartup : Albatross.Hosting.Startup {
		protected override bool Spa => true;
		public MyStartup(IConfiguration configuration) : base(configuration) { }
	}
}