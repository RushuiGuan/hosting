using Albatross.EFCore;
using Albatross.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.WebApi.Models;
using Sample.WebApi.Repositories;
using Sample.WebApi.Services;

namespace Sample.WebApi {
	public class MyStartup : Albatross.Hosting.Startup {
		public MyStartup(IConfiguration configuration) : base(configuration) {
			LogRequests = true;
			Spa = true;
		}

		public override void ConfigureServices(IServiceCollection services) {
			base.ConfigureServices(services);
			services.AddDbContext<SampleDbSession>(options => options.UseInMemoryDatabase("Sample"));
			services.AddScoped<ISampleDbSession>(provider => provider.GetRequiredService<SampleDbSession>());
			services.AddSingleton<ISemanticExceptionConverter, EFCoreSemanticExceptionConverter>();
			services.AddScoped<ICompanyRepository, CompanyRepository>();
			services.AddScoped<ICompanyService, CompanyService>();
		}
	}
}
