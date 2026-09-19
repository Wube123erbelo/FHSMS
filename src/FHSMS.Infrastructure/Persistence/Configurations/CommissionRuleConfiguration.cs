using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
{
    public void Configure(EntityTypeBuilder<CommissionRule> builder)
    {
        builder.ToTable("CommissionRules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.AgentType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Basis).HasConversion<string>().HasMaxLength(25);
        builder.Property(r => r.Percentage).HasColumnType("numeric(5,2)");
        builder.Property(r => r.FlatRateAmount).HasColumnType("numeric(10,2)");

        builder.HasMany(r => r.UnitRates)
            .WithOne()
            .HasForeignKey(ur => ur.CommissionRuleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.UnitRates).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
