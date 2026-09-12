using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.DestinationAddress).IsRequired().HasMaxLength(300);
        builder.Property(d => d.OriginLocation).HasMaxLength(300);
        builder.Property(d => d.TripPrice).HasColumnType("numeric(12,2)");
        builder.Property(d => d.DriverName).HasMaxLength(150);
        builder.Property(d => d.VehicleInfo).HasMaxLength(150);
        builder.Property(d => d.Notes).HasMaxLength(500);
        builder.Property(d => d.ReceivedByName).HasMaxLength(150);
        builder.Property(d => d.SignatureImageBase64).HasColumnType("text");
        builder.Property(d => d.PhotoUrl).HasMaxLength(500);
        builder.HasIndex(d => d.OrderId);
        builder.HasIndex(d => d.DriverId);
    }
}
