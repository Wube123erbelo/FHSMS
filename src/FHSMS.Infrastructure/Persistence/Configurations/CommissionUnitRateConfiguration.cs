using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class CommissionUnitRateConfiguration : IEntityTypeConfiguration<CommissionUnitRate>
{
    public void Configure(EntityTypeBuilder<CommissionUnitRate> builder)
    {
        builder.ToTable("CommissionUnitRates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RateAmount).HasColumnType("numeric(10,2)");
        builder.HasOne(r => r.Unit).WithMany().HasForeignKey(r => r.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => new { r.CommissionRuleId, r.UnitId }).IsUnique();
    }
}
