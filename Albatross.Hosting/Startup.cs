using Albatross.Authentication.AspNetCore;
using Albatross.Config;
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Albatross.Hosting {
	/// <summary>
	/// Default Startup class that setups the web server.
	/// </summary>
	public class Startup {
		public const string DefaultApp_RootPath = "wwwroot";
		protected IConfiguration Configuration { get; }
		protected IConfigureAuthentication ConfigAuthentication { get; set; }
		protected bool OpenApi { get; set; } = true;
		protected bool WebApi { get; set; } = true;
		protected bool Spa { get; set; } = false;
		protected bool RazorPages { get; set; } = false;
		protected bool SuppressLoggingOfKnownExceptions { get; set; } = false;
		protected bool MaskExceptionDetail { get; set; } = true;
		protected IApplicationFeatureProvider[] FeatureProviders { get; set; } = [];

		/// <summary>
		/// When true, a plain text formatter is used for response contents are of type string.  The content type of the response will be changed to 'text/html'
		/// the response will send back utf8 encoded text
		/// </summary>
		protected bool PlainTextFormatter { get; set; } = true;

		public Startup(IConfiguration configuration) {
			this.Configuration = configuration;
			this.ConfigAuthentication = new AuthenticationConfigurator(configuration);
			var uri = configuration.GetValue<string>("urls");
			if (string.IsNullOrEmpty(uri)) {
				Log.Logger.ForContext<Startup>().Information("AspNetCore Starting up");
			} else {
				Log.Logger.ForContext<Startup>().Information("AspNetCore Starting up at {uri}", uri);
			}
		}

		protected virtual void ConfigureCors(CorsPolicyBuilder builder) {
			var cors = this.Configuration.GetSection("cors").Get<string[]>() ?? Array.Empty<string>();
			builder.WithOrigins(cors)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
			Log.Logger.ForContext<Startup>().Information("Cors configuration: {cors}", cors.Length == 0 ? "None" : String.Join(",", cors));
		}

		#region swagger
		public virtual IServiceCollection AddOpenApi(IServiceCollection services) {
			services.AddOpenApi(options => {
				if (this.ConfigAuthentication.UseAnyBearerToken) {
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
								new OpenApiSecuritySchemeReference("Bearer", doc, null), new List<string>()
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
			if (this.ConfigAuthentication.Configure(services)) {
				services.AddAuthorization(ConfigureAuthorization);
			}
			return services;
		}

		protected virtual void ConfigureAuthorization(AuthorizationOptions option) { }
		#endregion

		public IServiceCollection AddSpa(IServiceCollection services) {
			services.AddSpaStaticFiles(cfg => cfg.RootPath = DefaultApp_RootPath);
			services.AddConfig<IAngularConfig, AngularConfig>(true);
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
			services.TryAddSingleton<IExceptionHandler>(provider => new DefaultExceptionHandler(this.MaskExceptionDetail, provider.GetRequiredService<ILogger<DefaultExceptionHandler>>()));
			services.TryAddSingleton(provider => provider.GetRequiredService<ILoggerFactory>().CreateLogger("default"));
			services.AddHttpContextAccessor();
			if (WebApi) {
				var builder = services.AddControllers(options => {
					if (this.PlainTextFormatter) {
						options.InputFormatters.Add(new PlainTextInputFormatter());
					}
				});
				if (this.FeatureProviders.Any()) {
					builder.ConfigureApplicationPartManager(apm => {
						foreach (var provider in this.FeatureProviders) {
							apm.FeatureProviders.Add(provider);
						}
					});
				}
				builder.AddJsonOptions(ConfigureJsonOption);
				services.AddCors(opt => opt.AddDefaultPolicy(ConfigureCors));
				services.AddAspNetCorePrincipalProvider();
				if (OpenApi) {
					AddOpenApi(services);
				}
			}
			if (RazorPages) {
				services.AddRazorPages();
			}
			if (Spa) { AddSpa(services); }
			AddAccessControl(services);
			// add compression
			if (this.CompressionMimeTypes.Any()) {
				services.AddResponseCompression(options => {
					options.EnableForHttps = true;
					options.MimeTypes = CompressionMimeTypes;
					options.Providers.Add<GzipCompressionProvider>();
					options.Providers.Add<BrotliCompressionProvider>();
				});
				services.Configure<GzipCompressionProviderOptions>(options => {
					options.Level = System.IO.Compression.CompressionLevel.Optimal;
				});
				services.Configure<BrotliCompressionProviderOptions>(options => {
					options.Level = System.IO.Compression.CompressionLevel.Fastest;
				});
			}
		}

		public virtual void Configure(IApplicationBuilder app, ProgramSetting programSetting, EnvironmentSetting environmentSetting, ILogger<Startup> logger) {
			if (this.CompressionMimeTypes.Any()) {
				app.UseResponseCompression();
			}
			logger.LogInformation("Initializing {@program} with environment {environment}", programSetting.App, environmentSetting.Value);
			app.UseExceptionHandler(new ExceptionHandlerOptions {
				ExceptionHandler = new GlobalExceptionHandler(this.MaskExceptionDetail).Handle,
				// only let the middleware log server errors; suppress diagnostics for client 4xx errors
				SuppressDiagnosticsCallback = this.SuppressLoggingOfKnownExceptions ? context => GlobalExceptionHandler.GetStatusCode(context.Exception) < StatusCodes.Status500InternalServerError : null,
			});
			app.UseRouting();
			if (WebApi) {
				app.UseCors();
				if (this.ConfigAuthentication.HasAnyAuthentication) { app.UseAuthentication().UseAuthorization(); }
				app.UseEndpoints(endpoints => endpoints.MapControllers());
			}
			if (RazorPages) {
				app.UseEndpoints(endpoints => endpoints.MapRazorPages());
			}
			if (WebApi && OpenApi) { UseOpenApi(app); }
			if (Spa) { UseSpa(app, logger); }
		}

		public void UseSpa(IApplicationBuilder app, ILogger<Startup> logger) {
			var config = app.ApplicationServices.GetRequiredService<IAngularConfig>();
			logger.LogInformation("Initializing SPA with request path of '{requestPath}' and baseHref of '{baseRef}'", config.RequestPath, config.BaseHref);
			// Resolve the static file root relative to the application base directory instead of the
			// current working directory, which may differ when the app is launched as a service.
			var rootPath = Path.Combine(AppContext.BaseDirectory, DefaultApp_RootPath);
			var fileProvider = new PhysicalFileProvider(rootPath);
			var options = new StaticFileOptions {
				RequestPath = config.RequestPath,
				FileProvider = fileProvider,
			};
			app.UseSpaStaticFiles(options);
			app.Map(config.RequestPath,
				web => web.UseSpa(spa => {
					spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions {
						FileProvider = fileProvider,
					};
				}));
			app.ApplicationServices.GetRequiredService<ITransformAngularConfig>().Transform();
		}
	}
}