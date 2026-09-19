using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PaymentNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(p => p.PaymentNumber).IsUnique();
        builder.Property(p => p.Amount).HasColumnType("numeric(14,2)");
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.ProviderResponse).HasColumnType("text");

        builder.HasOne<BankAccount>()
            .WithMany()
            .HasForeignKey(p => p.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Idempotency: the same provider reference can never be posted twice as
        // a completed payment - see RecordPaymentCommandHandler for the
        // application-level pre-check that surfaces this as a clean validation
        // error rather than a raw database exception.
        builder.HasIndex(p => p.Reference).IsUnique().HasFilter("\"Reference\" IS NOT NULL");

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
