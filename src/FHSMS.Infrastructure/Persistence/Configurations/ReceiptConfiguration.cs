using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ReceiptNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(r => r.ReceiptNumber).IsUnique();
        builder.Property(r => r.Amount).HasColumnType("numeric(14,2)");
        builder.Property(r => r.Method).HasMaxLength(20);
        builder.Property(r => r.TransactionReference).HasMaxLength(100);
        builder.Property(r => r.IssuedBy).HasMaxLength(150);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
