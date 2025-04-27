using Albatross.Authentication.AspNetCore;
using Albatross.Config;
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Albatross.Hosting {
	/// <summary>
	/// Default Startup class that setups the web server.
	/// </summary>
	public class Startup {
		public const string DefaultApp_RootPath = "wwwroot";
		protected IConfiguration Configuration { get; }
		protected AuthenticationSettings AuthenticationSettings { get; }
		public virtual bool Swagger { get; } = true;
		public virtual bool WebApi { get; } = true;
		public virtual bool Spa { get; } = false;
		public virtual bool LogUsage { get; } = true;

		/// <summary>
		/// When true, a plain text formatter is used for response contents are of type string.  The content type of the response will be changed to 'text/html'
		/// the response will send back utf8 encoded text
		/// </summary>
		public virtual bool PlainTextFormatter { get; } = true;

		public Startup(IConfiguration configuration) {
			this.Configuration = configuration;
			this.AuthenticationSettings = new AuthenticationSettings(configuration);
			this.AuthenticationSettings.Validate();
			Log.Logger.Information("AspNetCore Startup configuration with authentication={secured}, spa={spa}, swagger={swagger}, webapi={webapi}, usage={usage}", this.AuthenticationSettings.HasAny, Spa, Swagger, WebApi, LogUsage);
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

		public virtual IServiceCollection AddSwagger(IServiceCollection services) {
			services.AddSwaggerGen(options => {
				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter your JWT token like this: Bearer {your token}"
				});
				options.AddSecurityRequirement(new OpenApiSecurityRequirement {
					{
						new OpenApiSecurityScheme {
							Reference = new OpenApiReference {
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new string[] { }
					}
				});
			});
			return services;
		}

		public virtual void UseSwagger(IApplicationBuilder app) {
			app.UseSwagger();
			app.UseSwaggerUI(c => c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object> {
				["activated"] = false
			});
		}

		#endregion

		#region authorization

		protected virtual void ConfigureAuthorization(AuthorizationOptions option) { }

		public virtual IServiceCollection AddAccessControl(IServiceCollection services) {
			if (this.AuthenticationSettings.UseKerboros) {
				services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
			}
			if(this.AuthenticationSettings.BearerTokens.Any()) {
				var builder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
				foreach (var token in this.AuthenticationSettings.BearerTokens) {
					builder.AddJwtBearer(token.Provider, options => {
						options.Authority = token.Authority;
						options.IncludeErrorDetails = true;
						options.TokenValidationParameters = new TokenValidationParameters {
							ValidateIssuer = token.ValidateIssuer,
							ValidIssuer = token.Issuer,
							ValidateAudience = token.ValidateAudience,
							ValidAudience = token.Audience,
							ValidateLifetime = token.ValidateLifetime,
						};
					});
				}
			}
			services.AddAuthorization(ConfigureAuthorization);
			return services;
		}

		#endregion

		public IServiceCollection AddSpa(IServiceCollection services) {
			services.AddSpaStaticFiles(cfg => cfg.RootPath = DefaultApp_RootPath);
			services.AddConfig<AngularConfig>(true);
			services.AddSingleton<ITransformAngularConfig, TransformAngularConfig>();
			return services;
		}

		public virtual void ConfigureJsonOption(JsonOptions options) { }

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
				if (Swagger) {
					// services.AddMvc();
					AddSwagger(services);
				}
			}
			if (Spa) { AddSpa(services); }
			if (this.AuthenticationSettings.HasAny) { AddAccessControl(services); }
		}

		public virtual void Configure(IApplicationBuilder app, ProgramSetting programSetting, EnvironmentSetting environmentSetting, ILogger<Startup> logger) {
			logger.LogInformation("Initializing {@program} with environment {environment}", programSetting, environmentSetting.Value);
			app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = GlobalExceptionHandler.Handle });
			app.UseRouting();
			if (WebApi) {
				app.UseCors();
				if (this.AuthenticationSettings.HasAny) { app.UseAuthentication().UseAuthorization(); }
				if (this.LogUsage) { app.UseMiddleware<HttpRequestLoggingMiddleware>(); }
				app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
			}
			if (WebApi && Swagger) { UseSwagger(app); }
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

		// protected virtual IGlobalExceptionHandler GlobalExceptionHandler{get; } = new ProblemDetailsGlobalExceptionHandler();
		protected virtual IGlobalExceptionHandler GlobalExceptionHandler { get; } = new DefaultGlobalExceptionHandler();
	}
}