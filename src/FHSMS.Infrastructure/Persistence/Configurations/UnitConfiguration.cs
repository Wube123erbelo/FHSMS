using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(u => u.Code).IsUnique();
        builder.Property(u => u.Name).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Abbreviation).IsRequired().HasMaxLength(10);
    }
}
