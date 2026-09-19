using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext so the Application layer (commands,
/// queries, handlers) never references Infrastructure/EF Core directly. Only
/// the Infrastructure project implements this.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductPrice> ProductPrices { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Unit> Units { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<TaxConfiguration> TaxConfigurations { get; }
    DbSet<TaxRate> TaxRates { get; }
    DbSet<PlatformCommissionConfiguration> PlatformCommissionConfigurations { get; }
    DbSet<PlatformCommissionRate> PlatformCommissionRates { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<FarmerInvoice> FarmerInvoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<User> Users { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<Delivery> Deliveries { get; }
    DbSet<DriverPayment> DriverPayments { get; }
    DbSet<CommissionRule> CommissionRules { get; }
    DbSet<CommissionUnitRate> CommissionUnitRates { get; }
    DbSet<Commission> Commissions { get; }
    DbSet<BankAccount> BankAccounts { get; }
    DbSet<BankTransaction> BankTransactions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Farmer> Farmers { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<Receipt> Receipts { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
