using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Products.Queries.GetPriceHistory;

/// <summary>Kind is optional - omit it to get both timelines (admin's full audit view); pass it to see just one.</summary>
public record GetPriceHistoryQuery(Guid ProductId, FHSMS.Domain.Enums.PriceKind? Kind = null) : IRequest<List<ProductPriceHistoryDto>>;
