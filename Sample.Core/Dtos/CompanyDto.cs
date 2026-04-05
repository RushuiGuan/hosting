namespace Sample.Core.Dtos {
	public record class CompanyDto {
		public int Id { get; init; }
		public required string Name { get; init; }
	}
}