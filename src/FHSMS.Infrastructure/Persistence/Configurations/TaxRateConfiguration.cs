using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("TaxRates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rate).HasColumnType("numeric(5,2)");
        builder.HasIndex(r => new { r.TaxConfigurationId, r.EffectiveFrom });
    }
}
