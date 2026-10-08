using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        // DB-level backstop for the one-invoice-per-order rule enforced in
        // GenerateInvoiceCommandHandler - catches the race a simple app-level
        // check can miss (two near-simultaneous requests for the same order).
        // Filtered so a Cancelled invoice doesn't block a genuine replacement.
        builder.HasIndex(i => i.OrderId)
            .HasFilter("\"Status\" <> 'Cancelled'")
            .IsUnique();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.TaxMode).HasConversion<string>().HasMaxLength(20);

        builder.Property(i => i.Subtotal).HasColumnType("numeric(14,2)");
        builder.Property(i => i.TaxableAmount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.Discount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.PlatformCommissionRateApplied).HasColumnType("numeric(5,2)");
        builder.Property(i => i.PlatformCommissionAmount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.HotelAgentBonusAmount).HasColumnType("numeric(14,2)");
        builder.Property(i => i.GrandTotal).HasColumnType("numeric(14,2)");
        builder.Property(i => i.AmountPaid).HasColumnType("numeric(14,2)");
        builder.Ignore(i => i.BalanceDue);

        builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
