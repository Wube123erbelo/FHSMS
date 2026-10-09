using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Price).HasColumnType("numeric(12,2)");
        builder.Property(p => p.Reason).HasMaxLength(300);
        builder.Property(p => p.ChangedBy).HasMaxLength(150);
        builder.HasIndex(p => new { p.ProductId, p.Kind, p.EffectiveFrom });
    }
}
