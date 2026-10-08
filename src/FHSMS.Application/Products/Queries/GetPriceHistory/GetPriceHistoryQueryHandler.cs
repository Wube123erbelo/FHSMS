using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Products.Queries.GetPriceHistory;

public class GetPriceHistoryQueryHandler : IRequestHandler<GetPriceHistoryQuery, List<ProductPriceHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    public GetPriceHistoryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ProductPriceHistoryDto>> Handle(GetPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProductPrices.Where(p => p.ProductId == request.ProductId);
        if (request.Kind is { } kind)
            query = query.Where(p => p.Kind == kind);

        return await query
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => new ProductPriceHistoryDto
            {
                Id = p.Id,
                Price = p.Price,
                Kind = p.Kind,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                Reason = p.Reason,
                ChangedBy = p.ChangedBy
            })
            .ToListAsync(cancellationToken);
    }
}
