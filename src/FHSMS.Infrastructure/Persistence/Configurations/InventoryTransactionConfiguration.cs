using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.QuantityChange).HasColumnType("numeric(14,3)");
        builder.Property(t => t.Reference).HasMaxLength(100);
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.HasIndex(t => t.ProductId);
        builder.HasIndex(t => t.FarmerId);
        builder.HasIndex(t => t.AgentUserId);
    }
}
