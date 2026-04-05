using Sample.WebApi.Models;
using Sample.WebApi.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.WebApi.Services {
	public interface ICompanyService {
		Task<Company> Create(string name, CancellationToken cancellationToken);
		Task<Company> Update(int id, string name, CancellationToken cancellationToken);
		Task Delete(int id, CancellationToken cancellationToken);
	}

	public class CompanyService : ICompanyService {
		readonly ICompanyRepository companyRepository;

		public CompanyService(ICompanyRepository companyRepository) {
			this.companyRepository = companyRepository;
		}

		public Task<Company> Create(string name, CancellationToken cancellationToken) {
			var company = new Company { Name = name };
			companyRepository.Add(company);
			return Task.FromResult(company);
		}

		public async Task<Company> Update(int id, string name, CancellationToken cancellationToken) {
			var company = await companyRepository.GetById(id, cancellationToken);
			company.Name = name;
			return company;
		}

		public async Task Delete(int id, CancellationToken cancellationToken) {
			var company = await companyRepository.GetById(id, cancellationToken);
			companyRepository.Delete(company);
		}
	}
}
