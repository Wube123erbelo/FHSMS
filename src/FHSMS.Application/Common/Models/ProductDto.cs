using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public Guid UnitId { get; set; }
    public string? UnitCode { get; set; }
    public string? UnitName { get; set; }
    public string? UnitAbbreviation { get; set; }
    /// <summary>What the company pays a farmer per unit. Null in the response when the caller's role isn't allowed to see it (e.g. HotelAgent, HotelPortal) - see ProductVisibility.</summary>
    public decimal? CurrentBuyingPrice { get; set; }
    public DateTime? BuyingPriceEffectiveFrom { get; set; }
    /// <summary>What a hotel is charged per unit. Null in the response when the caller's role isn't allowed to see it (e.g. FarmerAgent) - see ProductVisibility.</summary>
    public decimal? CurrentSellingPrice { get; set; }
    public DateTime? SellingPriceEffectiveFrom { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public TaxProfileType TaxProfile { get; set; }
    public bool IsActive { get; set; }
}
