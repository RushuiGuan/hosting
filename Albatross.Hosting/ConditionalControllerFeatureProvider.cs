using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Albatross.Hosting {
	public class ConditionalControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature> {
		private readonly Func<TypeInfo, bool> isEnabled;
		public ConditionalControllerFeatureProvider(Func<TypeInfo, bool> isEnabled) {
			this.isEnabled = isEnabled;
		}
		public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature) {
			foreach (var removed in feature.Controllers.Where(c => !isEnabled(c)).ToList()) {
				feature.Controllers.Remove(removed);
			}
		}
	}
}