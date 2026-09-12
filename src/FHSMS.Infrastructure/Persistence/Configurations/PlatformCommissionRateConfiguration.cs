using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class PlatformCommissionRateConfiguration : IEntityTypeConfiguration<PlatformCommissionRate>
{
    public void Configure(EntityTypeBuilder<PlatformCommissionRate> builder)
    {
        builder.ToTable("PlatformCommissionRates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rate).HasColumnType("numeric(5,2)");
        builder.HasIndex(r => new { r.PlatformCommissionConfigurationId, r.EffectiveFrom });
    }
}
