using Albatross.Config;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Albatross.Hosting {
	public interface ITransformAngularConfig {
		void Transform();
	}
	public class TransformAngularConfig : ITransformAngularConfig {
		private readonly IAngularConfig config;
		private readonly EnvironmentSetting environmentSetting;
		private readonly ILogger<TransformAngularConfig> logger;

		public TransformAngularConfig(IAngularConfig config, EnvironmentSetting environmentSetting, ILogger<TransformAngularConfig> logger) {
			this.config = config;
			this.environmentSetting = environmentSetting;
			this.logger = logger;
		}

		public void Transform() {
			if (config.ConfigFile.Length > 0 && !string.IsNullOrEmpty(environmentSetting.Value)) {
				string file = GetConfigFile(null);
				string change = GetConfigFile(environmentSetting.Value);
				if (!File.Exists(file)) {
					logger.LogError("Angular config file {file} doesn't exist", file);
				} else if (!File.Exists(change)) {
					logger.LogError("Angular config transformation file {file} doesn't exist", change);
				} else {
					var srcElem = ReadJsonFile(file);
					var changeElem = ReadJsonFile(change);
					var result = Albatross.Serialization.Json.Extensions.ApplyJsonValue(srcElem, changeElem);
					logger.LogInformation("Overriding config file {config} with values from {environment_config}", config.ConfigFile.Last(), Path.GetFileName(change));
					WriteJsonFile(file, result);
				}
			}
			UpdateBaseHref();
		}
		
		const string BaseUrlRegexPattern ="<\\s*base\\s+href\\s*=\\s*\"[^\"]*\"\\s*>";
		static readonly Regex BaseUrlRegex = new Regex(BaseUrlRegexPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
		
		/// <summary>
		/// Points the index html's base href at <see cref="IAngularConfig.BaseHref"/>, writing only when that
		/// changes something.
		/// </summary>
		/// <remarks>
		/// The comparison is what makes this safe to call at startup. This runs on every startup, and an app
		/// installed somewhere its own account cannot write to - under %ProgramFiles%, served by an IIS
		/// application pool identity or a service account - used to throw UnauthorizedAccessException here and
		/// take the whole host down with it, having opened the writer whether or not the regex matched anything.
		/// Once the base href is correct, and something with the rights to do it has to set it the first time,
		/// there is nothing left to write and a read-only install directory stops being fatal.
		/// </remarks>
		public void UpdateBaseHref() {
			if (config.BaseHrefFile.Length > 0) {
				var indexHtml = Path.Join(new string[] {
					AppContext.BaseDirectory,
				}.Union(config.BaseHrefFile).ToArray());
				if (File.Exists(indexHtml)) {
					string content;
					using (var reader = new StreamReader(indexHtml)) {
						content = reader.ReadToEnd();
					}
					string replacement = $"<base href=\"{config.BaseHref}\">";
					string updated = BaseUrlRegex.Replace(content, replacement);
					// ordinal: this is markup being matched byte for byte, not text being compared for meaning
					if (string.Equals(content, updated, StringComparison.Ordinal)) {
						logger.LogInformation("BaseHref of {file} is already {baseHref}, leaving the file alone", indexHtml, config.BaseHref);
					} else {
						try {
							logger.LogInformation("Replacing baseHref for {file}", indexHtml);
							using (var writer = new StreamWriter(indexHtml)) {
								writer.Write(updated);
							}
						} catch (Exception err) {
							// this exception should not cause a hard failure that stop the app from starting up!
							// in a deployed environment, it is the responsibility of the installer to set baseref.
							// in a dev environment, this should work.  if failed, user should check environment
							logger.LogError(err, "Error updating baseHref for {file}", indexHtml);
						}
					}
				} else {
					logger.LogError("Angular index html file {name} doesn't exist", indexHtml);
				}
			} else {
				logger.LogWarning("Angular config baseHrefFile property not specified, baseHref transformation skipped");
			}
		}
		string GetConfigFile(string? environment) {
			string name = config.ConfigFile.Last();
			if (!string.IsNullOrEmpty(environment)) {
				name = $"{Path.GetFileNameWithoutExtension(name)}.{environment}.json";
			}
			var location = new string[] {
				AppContext.BaseDirectory,
			}.Union(config.ConfigFile.SkipLast(1)).Union([name]).ToArray();
			return Path.Join(location);
		}

		JsonElement ReadJsonFile(string file) {
			using (var reader = new StreamReader(file)) {
				string text = reader.ReadToEnd();
				return JsonSerializer.Deserialize<JsonElement>(text);
			}
		}
		void WriteJsonFile(string file, JsonElement element) {
			string content = JsonSerializer.Serialize<JsonElement>(element, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });
			using (StreamWriter writer = new StreamWriter(file)) {
				writer.Write(content);
			}
		}
	}
}