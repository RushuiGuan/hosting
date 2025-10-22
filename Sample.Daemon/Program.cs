using System;
using System.Threading.Tasks;

namespace Sample.Daemon {
	public class Program {
		public static Task Main(string[] args) {
			Albatross.Logging.Extensions.RemoveLegacySlackSinkOptions();
			return new MySetup(args)
				.ConfigureServiceHost<MyHostedService>()
				.RunAsService()
				.RunAsync();
		}
	}
}
