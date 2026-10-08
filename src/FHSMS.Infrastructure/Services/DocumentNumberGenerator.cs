using FHSMS.Application.Common.Interfaces;
using FHSMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Infrastructure.Services;

/// <summary>
/// Generates sequential document numbers per calendar year, e.g. ORD-2026-000123.
/// Uses a simple count-based approach; under heavy concurrent write load a DB
/// sequence would be a safer choice, but this keeps the sample self-contained.
/// </summary>
public class DocumentNumberGenerator : IDocumentNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public DocumentNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> NextOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Orders.CountAsync(o => o.OrderDate.Year == year, cancellationToken);
        return $"ORD-{year}-{(count + 1):D6}";
    }

    public async Task<string> NextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Invoices.CountAsync(i => i.InvoiceDate.Year == year, cancellationToken);
        return $"INV-{year}-{(count + 1):D6}";
    }

    /// <summary>The farmer/buying-side counterpart to NextInvoiceNumberAsync - a distinct prefix (FINV) keeps the two document series visually distinguishable everywhere a number is printed or searched.</summary>
    public async Task<string> NextFarmerInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.FarmerInvoices.CountAsync(i => i.CreatedAt.Year == year, cancellationToken);
        return $"FINV-{year}-{(count + 1):D6}";
    }

    public async Task<string> NextPaymentNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Payments.CountAsync(p => p.PaidAt.Year == year, cancellationToken);
        return $"PAY-{year}-{(count + 1):D6}";
    }

    public async Task<string> NextReceiptNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Receipts.CountAsync(r => r.IssuedAt.Year == year, cancellationToken);
        return $"RCPT-{year}-{(count + 1):D6}";
    }

    // These codes are NOT year-scoped like the document numbers above - a
    // category, unit, hotel, farmer, or agent code should stay the same
    // forever once assigned, since it's used as a lookup/reference ID in
    // dropdowns and forms, not a dated transaction record.

    public async Task<string> NextCategoryCodeAsync(CancellationToken cancellationToken = default)
    {
        var count = await _context.ProductCategories.CountAsync(cancellationToken);
        return $"CAT-{(count + 1):D4}";
    }

    public async Task<string> NextUnitCodeAsync(CancellationToken cancellationToken = default)
    {
        var count = await _context.Units.CountAsync(cancellationToken);
        return $"UNIT-{(count + 1):D4}";
    }

    public async Task<string> NextCustomerCodeAsync(CancellationToken cancellationToken = default)
    {
        var count = await _context.Customers.CountAsync(cancellationToken);
        return $"HTL-{(count + 1):D4}";
    }

    public async Task<string> NextFarmerCodeAsync(CancellationToken cancellationToken = default)
    {
        var count = await _context.Farmers.CountAsync(cancellationToken);
        return $"FARM-{(count + 1):D4}";
    }

    public async Task<string> NextDriverCodeAsync(CancellationToken cancellationToken = default)
    {
        var count = await _context.Drivers.CountAsync(cancellationToken);
        return $"DRV-{(count + 1):D4}";
    }

    public async Task<string> NextAgentCodeAsync(Domain.Enums.UserRole role, CancellationToken cancellationToken = default)
    {
        var prefix = role switch
        {
            Domain.Enums.UserRole.HotelAgent => "HAGT",
            Domain.Enums.UserRole.FarmerAgent => "FAGT",
            Domain.Enums.UserRole.SuperAdmin => "ADM",
            _ => "USR"
        };

        var count = await _context.Users.CountAsync(u => u.Role == role, cancellationToken);
        return $"{prefix}-{(count + 1):D4}";
    }
}
