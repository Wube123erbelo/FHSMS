using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.TaxProfileApplied).HasConversion<string>().HasMaxLength(20);

        builder.Property(i => i.Quantity).HasColumnType("numeric(12,3)");
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(12,2)");
        builder.Property(i => i.LineSubtotal).HasColumnType("numeric(14,2)");
        builder.Property(i => i.TaxRateApplied).HasColumnType("numeric(5,2)");
        builder.Property(i => i.TaxableAmount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.LineTotal).HasColumnType("numeric(14,2)");
    }
}
