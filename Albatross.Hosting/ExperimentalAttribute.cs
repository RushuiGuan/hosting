using System;

namespace Albatross.Hosting {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ExperimentalAttribute : Attribute{
		const string DefaultMessage = "This feature is experimental and may change or be removed in future releases.";
		public ExperimentalAttribute(string message = DefaultMessage) {
			Message = message;
		}
		public string Message { get; }
	}
}
