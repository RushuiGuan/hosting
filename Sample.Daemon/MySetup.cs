using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Albatross.Hosting;

namespace Sample.Daemon {
	public class MySetup : Setup {
		public MySetup(string[] args) : base(args, null) { }
	}
}