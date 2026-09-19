using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class ProductPriceHistoryDto
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public PriceKind Kind { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Reason { get; set; }
    public string? ChangedBy { get; set; }
}
