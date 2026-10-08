using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class PlatformCommissionConfigurationConfiguration : IEntityTypeConfiguration<PlatformCommissionConfiguration>
{
    public void Configure(EntityTypeBuilder<PlatformCommissionConfiguration> builder)
    {
        builder.ToTable("PlatformCommissionConfigurations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(c => c.Rates)
            .WithOne()
            .HasForeignKey(r => r.PlatformCommissionConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Rates).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
