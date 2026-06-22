using Albatross.EFCore;
using Albatross.Exceptions;
using Microsoft.EntityFrameworkCore;
using Sample.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.WebApi.Repositories {
	public interface ICompanyRepository : IRepository {
		Task<List<Company>> GetAll(CancellationToken cancellationToken);
		Task<Company> GetById(int id, CancellationToken cancellationToken);
		Task<Company?> GetByName(string name, CancellationToken cancellationToken);
	}

	public class CompanyRepository : Repository<ISampleDbSession>, ICompanyRepository {
		public CompanyRepository(ISampleDbSession session, ISemanticExceptionConverter exceptionConverter) : base(session, exceptionConverter) { }


		public Task<List<Company>> GetAll(CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>().ToListAsync(cancellationToken);

		public async Task<Company> GetById(int id, CancellationToken cancellationToken) {
			var entity = await session.DbContext.Set<Company>()
				.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
			if (entity == null) throw new NotFoundException<Company>(id);
			return entity;
		}

		public Task<Company?> GetByName(string name, CancellationToken cancellationToken) =>
			session.DbContext.Set<Company>()
				.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
	}
}
