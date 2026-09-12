using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProductsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.Prices)
            .AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var products = await query.ToListAsync(cancellationToken);

        // Role-based price visibility (same algorithm everywhere a product
        // list is served - web, mobile, and the Telegram bot, since all of
        // them call this one endpoint):
        //   SuperAdmin / Driver -> see both buying and selling
        //   FarmerAgent          -> buying price only (what they log stock at)
        //   HotelAgent / HotelCustomer / PublicPortalUser -> selling price only
        var (showBuying, showSelling) = ProductPriceVisibility.For(_currentUser.Role);

        return products.Select(p =>
        {
            var buyingPrice = p.GetBuyingPriceAsOf();
            var sellingPrice = p.GetSellingPriceAsOf();
            return new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryCode = p.Category?.Code,
                CategoryName = p.Category?.Name,
                UnitId = p.UnitId,
                UnitCode = p.Unit?.Code,
                UnitName = p.Unit?.Name,
                UnitAbbreviation = p.Unit?.Abbreviation,
                CurrentBuyingPrice = showBuying ? buyingPrice?.Price : null,
                BuyingPriceEffectiveFrom = showBuying ? buyingPrice?.EffectiveFrom : null,
                CurrentSellingPrice = showSelling ? sellingPrice?.Price : null,
                SellingPriceEffectiveFrom = showSelling ? sellingPrice?.EffectiveFrom : null,
                LowStockThreshold = p.LowStockThreshold,
                TaxProfile = p.TaxProfile,
                IsActive = p.IsActive
            };
        }).ToList();
    }
}
