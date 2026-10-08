using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Updates everything about a product EXCEPT its price - price changes only
/// ever happen through SchedulePriceCommand, never as a silent field edit
/// here, so every price change is dated and reasoned.
/// </summary>
public record UpdateProductCommand(
    Guid ProductId,
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    TaxProfileType TaxProfile,
    decimal? LowStockThreshold,
    bool IsActive) : IRequest;
