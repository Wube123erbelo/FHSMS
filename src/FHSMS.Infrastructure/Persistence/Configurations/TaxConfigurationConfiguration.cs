using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class TaxConfigurationConfiguration : IEntityTypeConfiguration<TaxConfiguration>
{
    public void Configure(EntityTypeBuilder<TaxConfiguration> builder)
    {
        builder.ToTable("TaxConfigurations");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.TaxType).HasConversion<string>().HasMaxLength(30);
        builder.Property(t => t.CalculationMode).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(t => t.Rates)
            .WithOne()
            .HasForeignKey(r => r.TaxConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Rates).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
