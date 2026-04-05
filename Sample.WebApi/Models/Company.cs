using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sample.WebApi.Models {
	public class Company {
		public int Id { get; set; }
		public required string Name { get; set; }
	}

	public class CompanyEntityMap : EntityMap<Company> {
		public override void Map(EntityTypeBuilder<Company> builder) {
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.Name).IsUnique();
			builder.Property(x => x.Name).HasMaxLength(256);
		}
	}
}
