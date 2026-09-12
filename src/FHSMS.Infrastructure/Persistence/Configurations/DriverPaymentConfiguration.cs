using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class DriverPaymentConfiguration : IEntityTypeConfiguration<DriverPayment>
{
    public void Configure(EntityTypeBuilder<DriverPayment> builder)
    {
        builder.ToTable("DriverPayments");
        builder.HasKey(d => d.Id);
        // The hard guard against generating a driver payment twice for the
        // same trip - belt and braces alongside the fact both are created
        // together in one handler call (MarkDeliveredCommandHandler).
        builder.HasIndex(d => d.DeliveryId).IsUnique();
        builder.Property(d => d.Amount).HasColumnType("numeric(12,2)");
        builder.Property(d => d.DriverName).HasMaxLength(150);
    }
}
