using System.Reflection;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Common;
using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTime? _dateTime;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IDateTime? dateTime = null) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<PlatformCommissionConfiguration> PlatformCommissionConfigurations => Set<PlatformCommissionConfiguration>();
    public DbSet<PlatformCommissionRate> PlatformCommissionRates => Set<PlatformCommissionRate>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<FarmerInvoice> FarmerInvoices => Set<FarmerInvoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DriverPayment> DriverPayments => Set<DriverPayment>();
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
    public DbSet<CommissionUnitRate> CommissionUnitRates => Set<CommissionUnitRate>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTime?.UtcNow ?? DateTime.UtcNow;
        var user = _currentUserService?.Email ?? "system";
        var auditEntries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = user;
                    auditEntries.Add(BuildAuditLog(entry, "Created", user, now));
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = user;
                    auditEntries.Add(BuildAuditLog(entry, "Modified", user, now));
                    break;
                case EntityState.Deleted:
                    auditEntries.Add(BuildAuditLog(entry, "Deleted", user, now));
                    break;
            }
        }

        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static AuditLog BuildAuditLog(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry,
        string action, string performedBy, DateTime performedAt)
    {
        var changedProperties = entry.State == EntityState.Modified
            ? entry.Properties
                .Where(p => p.IsModified)
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
            : null;

        return new AuditLog
        {
            EntityName = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id,
            Action = action,
            PerformedBy = performedBy,
            PerformedAt = performedAt,
            Changes = changedProperties is null
                ? null
                : System.Text.Json.JsonSerializer.Serialize(changedProperties)
        };
    }
}
