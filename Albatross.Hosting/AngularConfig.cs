using Albatross.Config;
using Microsoft.Extensions.Configuration;
using System;

namespace Albatross.Hosting {
	public interface IAngularConfig {
		string[] ConfigFile { get; }
		string[] BaseHrefFile { get; }
		/// <summary>
		/// Use by the hosting aspnetcore web app.  Default is String.Empty.  This property should reflect the relative path for the angular app to the aspnetcore web app.  It should start with a / but never end with one.
		/// For example, if the angular path is: http://localhost/demo/ui and the aspnetcore web app path is: http://localhost/demo, the request path should be /ui
		/// </summary>
		string RequestPath { get; }

		/// <summary>
		/// Use by angular.  This property should reflect the relative path for the angular app to the host.  It should always start with a / and end with a /.
		/// For example, if the angular path is: http://localhost/demo/ui  The BaseHref should be /demo/ui/
		/// </summary>
		string BaseHref { get; }
	}
	public class AngularConfig : ConfigBase, IAngularConfig {
		public AngularConfig(IConfiguration configuration) : base(configuration, "angular") {
		}
		public string[] ConfigFile { get; set; } = Array.Empty<string>();
		public string[] BaseHrefFile { get; set; } = Array.Empty<string>();
		public string RequestPath { get; set; } = string.Empty;
		public string BaseHref { get; set; } = "/";
	}
}
