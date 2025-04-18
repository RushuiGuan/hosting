﻿using Albatross.Authentication.AspNetCore;
using Albatross.Config;
using Albatross.Hosting.ExceptionHandling;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;

namespace Albatross.Hosting {
	/// <summary>
	/// Default Startup class that setups the web server.
	/// </summary>
	public class Startup {
		public const string DefaultApp_RootPath = "wwwroot";

		protected AuthorizationSetting AuthorizationSetting { get; }
		protected IConfiguration Configuration { get; }

		public virtual bool Swagger { get; } = true;
		public virtual bool WebApi { get; } = true;
		public virtual bool Secured { get; } = false;
		public virtual bool Spa { get; } = false;
		public virtual bool LogUsage { get; } = true;
		/// <summary>
		/// When true, a plain text formatter is used for response contents are of type string.  The content type of the response will be changed to 'text/html'
		/// the response will send back utf8 encoded text
		/// </summary>
		public virtual bool PlainTextFormatter { get; } = true;
		
		public Startup(IConfiguration configuration) {
			this.Configuration = configuration;
			Log.Logger.Information("AspNetCore Startup configuration with secured={secured}, spa={spa}, swagger={swagger}, webapi={webapi}, usage={usage}", Secured, Spa, Swagger, WebApi, LogUsage);
			AuthorizationSetting = new AuthorizationSetting(configuration);
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
			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo { Title = new ProgramSetting(this.Configuration).App, Version = "v1" });
			});
			return services;
		}
		public virtual void UseSwagger(IApplicationBuilder app, ProgramSetting programSetting) {
			app.UseSwagger();
			app.UseSwaggerUI(c => c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object> {
				["activated"] = false
			});
		}
		#endregion

		#region authorization
		protected virtual void ConfigureAuthorization(AuthorizationOptions option) {
			foreach (var policy in this.AuthorizationSetting.Policies ?? new AuthorizationPolicy[0]) {
				option.AddPolicy(policy.Name, builder => BuildAuthorizationPolicy(builder, policy));
			}
		}

		private void BuildAuthorizationPolicy(AuthorizationPolicyBuilder builder, AuthorizationPolicy policy) {
			if (policy.Roles?.Length > 0) {
				builder.RequireRole(policy.Roles);
			}
		}

		public virtual IServiceCollection AddAccessControl(IServiceCollection services) {
			services.AddConfig<AuthorizationSetting>();
			services.AddAuthorization(ConfigureAuthorization);
			AuthenticationBuilder builder = services.AddAuthentication(AuthorizationSetting.Authentication);

			if (AuthorizationSetting.IsKerborosAuthentication) {
				builder.AddNegotiate();
			} else if (AuthorizationSetting.IsBearerAuthentication) {
				builder.AddJwtBearer(AuthorizationSetting.BearerAuthenticationScheme, options => {
					options.Authority = AuthorizationSetting.Authority;
					options.Audience = AuthorizationSetting.Audience;
					options.RequireHttpsMetadata = false;
					options.IncludeErrorDetails = true;
					options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
						ValidateIssuerSigningKey = false,
					};
				});
			}
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
					services.AddMvc();
					AddSwagger(services);
				}
			}
			if (Spa) { AddSpa(services); }
			if (Secured) { AddAccessControl(services); }
		}

		public virtual void Configure(IApplicationBuilder app, ProgramSetting programSetting, EnvironmentSetting environmentSetting, ILogger<Startup> logger) {
			logger.LogInformation("Initializing {@program} with environment {environment}", programSetting, environmentSetting.Value);
			app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = GlobalExceptionHandler.Handle });
			app.UseRouting();
			if (WebApi) {
				app.UseCors();
				if (Secured) { app.UseAuthentication().UseAuthorization(); }
				if (this.LogUsage) { app.UseMiddleware<HttpRequestLoggingMiddleware>(); }
				app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
			}
			if (WebApi && Swagger) { UseSwagger(app, programSetting); }
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