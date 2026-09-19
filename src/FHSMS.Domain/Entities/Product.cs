using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A sellable item. Price is NEVER a bare mutable field - see ProductPrice.
/// This mirrors the Tax engine's own rule exactly: "price cannot be silently
/// overwritten" and "historical orders retain their original unit price" are
/// only true because every price change goes through SchedulePrice, which
/// closes off the previous price rather than replacing it.
///
/// TaxProfile is per-product (not global), which is what lets two products
/// under the same active TaxConfiguration be taxed differently - e.g.
/// Tomato = Standard VAT while a subsidised staple = Tax Exempt.
/// </summary>
public class Product : AuditableEntity
{
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }
    public TaxProfileType TaxProfile { get; set; } = TaxProfileType.StandardVat;
    public bool IsActive { get; set; } = true;
    public decimal? LowStockThreshold { get; set; }

    private readonly List<ProductPrice> _prices = new();
    public IReadOnlyCollection<ProductPrice> Prices => _prices.AsReadOnly();

    public Product() { } // EF Core + object initializer usage elsewhere

    /// <summary>
    /// Schedules a new price of the given kind (Buying or Selling) effective
    /// from a future (or immediate) date, closing off the previously
    /// open-ended price of THAT SAME KIND so the two never overlap -
    /// identical pattern to TaxConfiguration.ScheduleRate. Buying and Selling
    /// timelines are independent: scheduling a new Selling price never closes
    /// or is affected by the Buying price's history.
    /// </summary>
    public ProductPrice SchedulePrice(PriceKind kind, decimal price, DateTime effectiveFrom, string? reason, string? changedBy)
    {
        if (price < 0)
            throw new DomainException("Price cannot be negative.");

        var currentlyOpen = _prices
            .Where(p => p.Kind == kind && p.EffectiveTo == null)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefault();

        if (currentlyOpen != null)
        {
            if (effectiveFrom <= currentlyOpen.EffectiveFrom)
            {
                var isPending = currentlyOpen.EffectiveFrom > DateTime.UtcNow;
                throw new DomainException(isPending
                    ? $"A {kind} price of {currentlyOpen.Price} is already scheduled to take effect on {currentlyOpen.EffectiveFrom:yyyy-MM-dd}. Choose a later date, or cancel that scheduled change first."
                    : $"New price must take effect after the currently active price started on {currentlyOpen.EffectiveFrom:yyyy-MM-dd}.");
            }
            currentlyOpen.Close(effectiveFrom);
        }

        var newPrice = new ProductPrice(Id, kind, price, effectiveFrom, reason, changedBy);
        _prices.Add(newPrice);
        return newPrice;
    }

    /// <summary>Returns the price of the given kind that was/is in force on a given date (defaults to now). Null if none scheduled yet.</summary>
    public ProductPrice? GetPriceAsOf(PriceKind kind, DateTime? asOf = null)
    {
        var date = asOf ?? DateTime.UtcNow;
        return _prices
            .Where(p => p.Kind == kind && p.EffectiveFrom <= date && (p.EffectiveTo == null || p.EffectiveTo > date))
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefault();
    }

    /// <summary>Convenience: the price the company pays a farmer per unit, as of a given date (or now).</summary>
    public ProductPrice? GetBuyingPriceAsOf(DateTime? asOf = null) => GetPriceAsOf(PriceKind.Buying, asOf);

    /// <summary>Convenience: the price a hotel is charged per unit, as of a given date (or now).</summary>
    public ProductPrice? GetSellingPriceAsOf(DateTime? asOf = null) => GetPriceAsOf(PriceKind.Selling, asOf);
}
