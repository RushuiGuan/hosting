using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Core.Dtos;

namespace Sample.WebApi.Models {
	public class Company {
		public int Id { get; set; }
		public required string Name { get; set; }
		public CompanyDto CreateDto() {
			return new CompanyDto {
				Id = Id,
				Name = Name
			};
		}
	}

	public class CompanyEntityMap : EntityMap<Company> {
		public override void Map(EntityTypeBuilder<Company> builder) {
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.Name).IsUnique();
			builder.Property(x => x.Name).HasMaxLength(256);
		}
	}
}
