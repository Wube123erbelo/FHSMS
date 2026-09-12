using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.TaxProfile).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.LowStockThreshold).HasColumnType("numeric(12,3)");

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Unit)
            .WithMany()
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Prices)
            .WithOne()
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Prices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
