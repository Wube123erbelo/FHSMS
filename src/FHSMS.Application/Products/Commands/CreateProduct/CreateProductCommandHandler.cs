using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitId = request.UnitId,
            TaxProfile = request.TaxProfile,
            LowStockThreshold = request.LowStockThreshold
        };

        // Every product's price - even the very first one - goes through the
        // same SchedulePrice path a later price change would use, so there is
        // never a code path that sets a price without a ProductPrice record.
        // Buying and Selling are scheduled independently from day one.
        product.SchedulePrice(PriceKind.Buying, request.InitialBuyingPrice, DateTime.UtcNow, "Initial buying price", _currentUser.Email);
        product.SchedulePrice(PriceKind.Selling, request.InitialSellingPrice, DateTime.UtcNow, "Initial selling price", _currentUser.Email);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
