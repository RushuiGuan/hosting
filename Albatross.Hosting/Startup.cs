using Albatross.Authentication.AspNetCore;
using Albatross.Config;
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Albatross.Hosting {
	/// <summary>
	/// Default Startup class that setups the web server.
	/// </summary>
	public class Startup {
		public const string DefaultApp_RootPath = "wwwroot";
		protected IConfiguration Configuration { get; }
		protected AuthenticationSettings AuthenticationSettings { get; }
		protected virtual bool OpenApi { get; } = true;
		protected virtual bool WebApi { get; } = true;
		protected virtual bool Spa { get; } = false;
		protected virtual bool LogUsage { get; } = true;

		/// <summary>
		/// When true, a plain text formatter is used for response contents are of type string.  The content type of the response will be changed to 'text/html'
		/// the response will send back utf8 encoded text
		/// </summary>
		public virtual bool PlainTextFormatter { get; } = true;

		public Startup(IConfiguration configuration) {
			this.Configuration = configuration;
			this.AuthenticationSettings = new AuthenticationSettings(configuration);
			this.AuthenticationSettings.Validate();
			Log.Logger.Information("AspNetCore Startup configuration with authentication={secured}, spa={spa}, swagger={swagger}, webapi={webapi}, usage={usage}", this.AuthenticationSettings.HasAny, Spa, OpenApi, WebApi, LogUsage);
		}

		protected virtual void ConfigureCors(CorsPolicyBuilder builder) {
			var cors = this.Configuration.GetSection("cors").Get<string[]>() ?? new string[0];
			builder.WithOrigins(cors)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
			Log.Logger.Information("Cors configuration: {cors}", cors.Length == 0 ? "None" : String.Join(",", cors));
		}

		#region swagger

		public virtual IServiceCollection AddOpenApi(IServiceCollection services) {
			services.AddOpenApi(options => {
				if (this.AuthenticationSettings.BearerTokens.Any()) {
					options.AddDocumentTransformer((doc, context, cancellationToken) => {
						doc.Components = doc.Components ?? new OpenApiComponents();
						doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
						doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme {
							Name = "Authorization",
							Type = SecuritySchemeType.Http,
							Scheme = "Bearer",
							BearerFormat = "JWT",
							In = ParameterLocation.Header,
							Description = "Enter your JWT token. Do not prefix the token with 'Bearer: '"
						};
						doc.Security ??= new List<OpenApiSecurityRequirement>();
						doc.Security.Add(new OpenApiSecurityRequirement {
							{
								new OpenApiSecuritySchemeReference("Bearer", doc, null),
								new List<string>()
							}
						});
						return Task.CompletedTask;
					});
				}
			});
			return services;
		}

		protected virtual void UseOpenApi(IApplicationBuilder app) {
			app.UseEndpoints(endpoints => {
				endpoints.MapOpenApi("/swagger/v1.json");
			});
			app.UseSwaggerUI(config => {
				var program = app.ApplicationServices.GetRequiredService<ProgramSetting>();
				config.SwaggerEndpoint("v1.json", program.App);
				config.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object> {
					["activated"] = false
				};
			});
		}

		#endregion

		#region authentication and authorization
		protected virtual IServiceCollection AddAccessControl(IServiceCollection services) {
			if (this.AuthenticationSettings.UseKerberos || this.AuthenticationSettings.BearerTokens.Any()) {
				var builder = services.AddAuthentication(option => {
					if (!string.IsNullOrEmpty(AuthenticationSettings.Default)) {
						option.DefaultScheme = this.AuthenticationSettings.Default;
					}
				});
				foreach (var token in this.AuthenticationSettings.BearerTokens) {
					builder.AddJwtBearer(token.Provider, token.SetJwtBearerOptions);
				}
				if (this.AuthenticationSettings.UseKerberos) {
					builder.AddNegotiate();
				}
			}
			services.AddAuthorization(ConfigureAuthorization);
			return services;
		}
		protected virtual void ConfigureAuthorization(AuthorizationOptions option) { }
		#endregion

		public IServiceCollection AddSpa(IServiceCollection services) {
			services.AddSpaStaticFiles(cfg => cfg.RootPath = DefaultApp_RootPath);
			services.AddConfig<AngularConfig>(true);
			services.AddSingleton<ITransformAngularConfig, TransformAngularConfig>();
			return services;
		}

		public virtual void ConfigureJsonOption(JsonOptions options) { }

		public virtual string[] CompressionMimeTypes => [
			"application/json",
			"application/xml",
			"text/plain",
			"text/html",
			"text/css",
			"text/csv",
			"text/javascript",
			"application/javascript",
			"application/x-javascript",
			"image/svg+xml"
		];

		public virtual void ConfigureServices(IServiceCollection services) {
			services.TryAddSingleton(provider => provider.GetRequiredService<ILoggerFactory>().CreateLogger("default"));
			services.TryAddSingleton(provider => new UsageWriter(provider.GetRequiredService<ILoggerFactory>().CreateLogger("usage")));
			if (WebApi) {
				services.AddControllers(options => {
					if (this.PlainTextFormatter) {
						options.InputFormatters.Add(new PlainTextInputFormatter());
					}
				}).AddJsonOptions(ConfigureJsonOption);
				services.AddCors(opt => opt.AddDefaultPolicy(ConfigureCors));
				services.AddAspNetCorePrincipalProvider();
				if (OpenApi) {
					AddOpenApi(services);
				}
			}
			if (Spa) { AddSpa(services); }
			if (this.AuthenticationSettings.HasAny) { AddAccessControl(services); }
			// add compression
			if (this.CompressionMimeTypes.Any()) {
				services.AddResponseCompression(
					options => {
						options.EnableForHttps = true;
						options.MimeTypes = CompressionMimeTypes;
						options.Providers.Add<GzipCompressionProvider>();
						options.Providers.Add<BrotliCompressionProvider>();
					});
				services.Configure<GzipCompressionProviderOptions>(
					options => {
						options.Level = System.IO.Compression.CompressionLevel.Optimal;
					});
				services.Configure<BrotliCompressionProviderOptions>(
					options => {
						options.Level = System.IO.Compression.CompressionLevel.Fastest;
					});
			}
		}

		public virtual void Configure(IApplicationBuilder app, ProgramSetting programSetting, EnvironmentSetting environmentSetting, ILogger<Startup> logger) {
			if (this.CompressionMimeTypes.Any()) {
				app.UseResponseCompression();
			}
			logger.LogInformation("Initializing {@program} with environment {environment}", programSetting, environmentSetting.Value);
			app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = GlobalExceptionHandler.Handle });
			app.UseRouting();
			if (WebApi) {
				app.UseCors();
				if (this.AuthenticationSettings.HasAny) { app.UseAuthentication().UseAuthorization(); }
				if (this.LogUsage) { app.UseMiddleware<HttpRequestLoggingMiddleware>(); }
				app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
			}
			if (WebApi && OpenApi) { UseOpenApi(app); }
			if (Spa) { UseSpa(app, logger); }
		}

		public void UseSpa(IApplicationBuilder app, ILogger<Startup> logger) {
			var config = app.ApplicationServices.GetRequiredService<AngularConfig>();
			logger.LogInformation("Initializing SPA with request path of '{requestPath}' and baseHref of '{baseRef}'", config.RequestPath, config.BaseHref);
			var options = new StaticFileOptions {
				RequestPath = config.RequestPath,
			};
			app.UseSpaStaticFiles(new StaticFileOptions { RequestPath = config.RequestPath });
			app.Map(config.RequestPath ?? string.Empty, web => web.UseSpa(spa => { }));
			app.ApplicationServices.GetRequiredService<ITransformAngularConfig>().Transform();
		}

		/// <summary>
		/// for legacy systems, override this method to use <see cref="LegacyGobalExceptionHandler"/>
		/// </summary>
		protected virtual IGlobalExceptionHandler GlobalExceptionHandler { get; } = new DefaultGlobalExceptionHandler();
	}
}