using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Inventory.Queries.GetStockLevels;

public class GetStockLevelsQueryHandler : IRequestHandler<GetStockLevelsQuery, List<StockLevelDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockLevelsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockLevelDto>> Handle(GetStockLevelsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Include(p => p.Unit)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var totals = await _context.InventoryTransactions
            .Where(t => t.IsConfirmed)
            .GroupBy(t => t.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(t => t.QuantityChange) })
            .ToListAsync(cancellationToken);

        return products.Select(p => new StockLevelDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            UnitAbbreviation = p.Unit != null ? p.Unit.Abbreviation : null,
            QuantityOnHand = totals.FirstOrDefault(t => t.ProductId == p.Id)?.Total ?? 0m
        }).ToList();
    }
}
