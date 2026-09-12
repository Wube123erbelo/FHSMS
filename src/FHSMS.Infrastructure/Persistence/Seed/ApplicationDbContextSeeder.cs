using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using FHSMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds a minimal, realistic starting state: default units/categories, a
/// SuperAdmin login, and one VAT TaxConfiguration set to the traditional 15% -
/// but critically, ENABLED can be flipped off from Settings at any time, and this
/// seed is the ONLY place a rate literal like "15" appears in the whole solution.
/// </summary>
public static class ApplicationDbContextSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var hasher = new PasswordHasher();
            var admin = new User
            {
                Code = "ADM-0001",
                FullName = "System Administrator",
                Email = "admin@fhsms.local",
                Role = UserRole.SuperAdmin
            };
            admin.SetPasswordHash(hasher.Hash("ChangeMe123!"));
            context.Users.Add(admin);
        }

        if (!await context.Units.AnyAsync())
        {
            context.Units.AddRange(
                new Unit { Code = "UNIT-0001", Name = "Kilogram", Abbreviation = "kg" },
                new Unit { Code = "UNIT-0002", Name = "Crate", Abbreviation = "crate" },
                new Unit { Code = "UNIT-0003", Name = "Litre", Abbreviation = "L" },
                new Unit { Code = "UNIT-0004", Name = "Piece", Abbreviation = "pc" });
        }

        if (!await context.ProductCategories.AnyAsync())
        {
            context.ProductCategories.AddRange(
                new ProductCategory { Code = "CAT-0001", Name = "Vegetables" },
                new ProductCategory { Code = "CAT-0002", Name = "Fruits" },
                new ProductCategory { Code = "CAT-0003", Name = "Dairy" },
                new ProductCategory { Code = "CAT-0004", Name = "Grains" });
        }

        if (!await context.TaxConfigurations.AnyAsync())
        {
            var vat = new TaxConfiguration(
                name: "Value Added Tax",
                taxType: TaxType.Vat,
                isEnabled: true,
                calculationMode: TaxCalculationMode.Exclusive,
                exemptionAllowed: true,
                initialRate: 15m,
                effectiveFrom: DateTime.UtcNow.Date);

            context.TaxConfigurations.Add(vat);
        }

        if (!await context.PlatformCommissionConfigurations.AnyAsync())
        {
            // The company's own commission on hotel sales - 2% per the
            // client's stated requirement, fully adjustable afterwards from
            // Settings -> Total Commission via SchedulePlatformCommissionRate,
            // no redeploy needed. This is the only place the literal "2"
            // appears, same principle as the 15% VAT seed above.
            var platformCommission = new PlatformCommissionConfiguration(
                name: "Total Commission",
                isEnabled: true,
                initialRate: 2m,
                effectiveFrom: DateTime.UtcNow.Date);

            context.PlatformCommissionConfigurations.Add(platformCommission);
        }

        if (!await context.CommissionRules.AnyAsync())
        {
            // Client's stated defaults - both fully adjustable at any time afterwards
            // via PUT /api/commissions/rules, no redeploy needed.
            context.CommissionRules.AddRange(
                CommissionRule.PercentageRule(AgentType.HotelAgent, 2m),      // 2% of invoice grand total
                CommissionRule.FlatRateRule(AgentType.FarmerAgent, 0.50m));   // 0.50 birr per kg received
        }

        if (!await context.BankAccounts.AnyAsync())
        {
            context.BankAccounts.Add(new BankAccount
            {
                BankName = "Commercial Bank of Ethiopia",
                AccountName = "FHSMS Operations",
                AccountNumber = "1000000000000"
            });
        }

        await context.SaveChangesAsync();
    }
}
