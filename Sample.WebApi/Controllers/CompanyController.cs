using Albatross.Hosting;
using Albatross.Hosting.EFCore;
using Albatross.Input;
using Microsoft.AspNetCore.Mvc;
using Sample.Core.Requests;
using Sample.WebApi.Models;
using Sample.WebApi.Repositories;
using Sample.WebApi.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class CompanyController : ControllerBase {
		readonly ICompanyService companyService;
		readonly ICompanyRepository companyRepository;

		public CompanyController(ICompanyService companyService, ICompanyRepository companyRepository) {
			this.companyService = companyService;
			this.companyRepository = companyRepository;
		}

		[HttpPost]
		public async Task<ActionResult<Company>> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken) {
			if (request.Validate(out var sanitized).HasProblem(out var problem)) {
				return BadRequest(problem);
			}
			return await companyRepository.SaveAndReturn(
				async ct => await companyService.Create(sanitized.Name, ct),
				cancellationToken);
		}
	}
}
