using Albatross.Hosting;
using Albatross.Hosting.ExceptionHandling;
using Albatross.Input;
using Microsoft.AspNetCore.Mvc;
using Sample.Core.Dtos;
using Sample.Core.Requests;
using Sample.WebApi.Repositories;
using Sample.WebApi.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.WebApi.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class CompanyController : ControllerBase {
		readonly ICompanyService companyService;
		private readonly IExceptionHandler errHandler;
		readonly ICompanyRepository companyRepository;

		public CompanyController(ICompanyService companyService, IExceptionHandler errHandler, ICompanyRepository companyRepository) {
			this.companyService = companyService;
			this.errHandler = errHandler;
			this.companyRepository = companyRepository;
		}

		[HttpPost]
		public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken) {
			if (request.Validate().HasProblem(out var problem)) {
				return BadRequest(problem);
			}
			try {
				var company = await companyService.Create(request.Name, cancellationToken);
				await this.companyRepository.SaveChangesAsync(cancellationToken);
				return company.CreateDto();
			} catch (Exception err) {
				return errHandler.Handle(err, null);
			}
		}
	}
}
