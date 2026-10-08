using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BankName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.AccountName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.AccountNumber).IsRequired().HasMaxLength(50);
    }
}
