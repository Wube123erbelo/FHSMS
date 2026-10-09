using FHSMS.Application.Common.Extensions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Reports.Queries.GetTopProducts;

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, List<TopProductDto>>
{
    private readonly IApplicationDbContext _context;
    public GetTopProductsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<TopProductDto>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.AsUtc() ?? DateTime.UtcNow.AddMonths(-1);
        var to = request.To.AsUtc() ?? DateTime.UtcNow;

        var items = await _context.Invoices
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to && i.Status != InvoiceStatus.Cancelled)
            .SelectMany(i => i.Items)
            .ToListAsync(cancellationToken);

        return items
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(request.Top)
            .ToList();
    }
}
