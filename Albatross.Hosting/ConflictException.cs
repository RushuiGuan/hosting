using System;

namespace Albatross.Hosting {
	public class ConflictException : Exception {
		public ConflictException() { }
		public ConflictException(string msg) : base(msg) { }
	}
}