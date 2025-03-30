using System;
using System.Threading.Tasks;

namespace Sample.Daemon {
	public class Program {
		public static Task Main(string[] args) {
			System.Environment.CurrentDirectory = AppContext.BaseDirectory;
			return new MySetup(args)
				.ConfigureServiceHost<MyHostedService>()
				.RunAsService()
				.RunAsync();
		}
	}
}
