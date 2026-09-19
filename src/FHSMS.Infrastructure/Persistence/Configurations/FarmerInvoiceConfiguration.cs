using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class FarmerInvoiceConfiguration : IEntityTypeConfiguration<FarmerInvoice>
{
    public void Configure(EntityTypeBuilder<FarmerInvoice> builder)
    {
        builder.ToTable("FarmerInvoices");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.InvoiceNumber).IsRequired().HasMaxLength(40);
        builder.HasIndex(f => f.InvoiceNumber).IsUnique();
        // The hard guard against generating a farmer invoice twice for the
        // same stock-in - belt and braces alongside the fact that both are
        // created together in one handler call.
        builder.HasIndex(f => f.InventoryTransactionId).IsUnique();
        builder.Property(f => f.Quantity).HasColumnType("numeric(14,3)");
        builder.Property(f => f.BuyingPriceApplied).HasColumnType("numeric(12,2)");
        builder.Property(f => f.TotalAmount).HasColumnType("numeric(14,2)");
    }
}
