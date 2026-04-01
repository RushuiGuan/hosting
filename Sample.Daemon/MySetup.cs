using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Albatross.Hosting;
using System;

namespace Sample.Daemon {
	public class MySetup : Setup {
		public MySetup(string[] args) : base(args, AppContext.BaseDirectory) { }
	}
}