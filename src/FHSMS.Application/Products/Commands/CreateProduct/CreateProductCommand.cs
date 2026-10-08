using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Products.Commands.CreateProduct;

/// <summary>
/// Registering a product now requires BOTH prices up front:
///   - InitialBuyingPrice: what the company pays a farmer per unit.
///   - InitialSellingPrice: what a hotel is charged per unit.
/// Both are scheduled independently via Product.SchedulePrice, exactly like
/// a later price change would be - there is no code path that creates a
/// product with only one of the two prices set.
/// </summary>
public record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    decimal InitialBuyingPrice,
    decimal InitialSellingPrice,
    TaxProfileType TaxProfile,
    decimal? LowStockThreshold) : IRequest<Guid>;
