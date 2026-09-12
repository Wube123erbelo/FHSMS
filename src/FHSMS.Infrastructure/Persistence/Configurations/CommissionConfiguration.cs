using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("Commissions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.SourceType).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Basis).HasConversion<string>().HasMaxLength(25);
        builder.Property(c => c.BaseAmount).HasColumnType("numeric(14,2)");
        builder.Property(c => c.Percentage).HasColumnType("numeric(5,2)");
        builder.Property(c => c.FlatRateAmount).HasColumnType("numeric(10,2)");
        builder.Property(c => c.CommissionAmount).HasColumnType("numeric(14,2)");
        builder.HasIndex(c => c.AgentUserId);
        builder.HasIndex(c => c.InvoiceId);
        builder.HasIndex(c => c.InventoryTransactionId);
    }
}
