using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single dated price belonging to a Product, mirroring TaxRate's pattern
/// exactly: immutable once created, effective-dated, never overwritten. This
/// is what makes "price cannot be silently overwritten" and "historical
/// orders retain their original unit price" true - CreateOrder always reads
/// the price that is/was in force as of the order date, not a bare mutable
/// field.
///
/// Kind separates this into two independent timelines per product (Buying vs
/// Selling) - each is scheduled and closed off independently via
/// Product.SchedulePrice(kind, ...), so changing the selling price never
/// touches the buying price's history and vice versa.
/// </summary>
public class ProductPrice : BaseEntity
{
    public Guid ProductId { get; private set; }
    public PriceKind Kind { get; private set; }
    public decimal Price { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public string? Reason { get; private set; }
    public string? ChangedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ProductPrice() { } // EF Core

    public ProductPrice(Guid productId, PriceKind kind, decimal price, DateTime effectiveFrom, string? reason, string? changedBy)
    {
        ProductId = productId;
        Kind = kind;
        Price = price;
        EffectiveFrom = effectiveFrom;
        Reason = reason;
        ChangedBy = changedBy;
        CreatedAt = DateTime.UtcNow;
    }

    internal void Close(DateTime effectiveTo) => EffectiveTo = effectiveTo;
}
