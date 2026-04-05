using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Sample.WebApi.Models {
	public interface ISampleDbSession : IDbSession { }

	public class SampleDbSession : DbSession, ISampleDbSession {
		public SampleDbSession(DbContextOptions options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			new CompanyEntityMap().Build(modelBuilder);
		}
	}
}
