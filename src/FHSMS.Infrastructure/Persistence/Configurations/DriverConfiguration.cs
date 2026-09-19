using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(d => d.Code).IsUnique();
        builder.Property(d => d.FullName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Phone).HasMaxLength(30);
        builder.Property(d => d.PlateNumber).HasMaxLength(30);
        builder.Property(d => d.TruckType).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(d => d.UserId).IsUnique(); // one profile per driver account
    }
}
