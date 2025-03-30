using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Albatross.Hosting;

namespace Sample.Daemon {
	public class MySetup : Setup {
		public MySetup(string[] args) : base(args) { }
		public override void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
			base.ConfigureServices(services, configuration);
		}
	}
}