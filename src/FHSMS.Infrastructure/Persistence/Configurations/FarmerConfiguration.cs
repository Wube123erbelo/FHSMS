using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class FarmerConfiguration : IEntityTypeConfiguration<Farmer>
{
    public void Configure(EntityTypeBuilder<Farmer> builder)
    {
        builder.ToTable("Farmers");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(f => f.Code).IsUnique();
        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
        builder.Property(f => f.ContactPerson).HasMaxLength(150);
        builder.Property(f => f.Phone).HasMaxLength(30);
        builder.Property(f => f.Location).HasMaxLength(200);
        builder.Property(f => f.BankAccountNumber).HasMaxLength(50);
    }
}
