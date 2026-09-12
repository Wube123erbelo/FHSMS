using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Products.Commands.SchedulePrice;

public class SchedulePriceCommandHandler : IRequestHandler<SchedulePriceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SchedulePriceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(SchedulePriceCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // The company must never be left in a sell-at-a-loss position:
        // whichever kind is being changed, check it against the OTHER kind's
        // price that will be in force from the same effective date. Buying
        // and Selling are still scheduled completely independently - this is
        // only a guard, not a coupling of the two timelines.
        var otherKind = request.Kind == PriceKind.Buying ? PriceKind.Selling : PriceKind.Buying;
        var otherCurrent = product.GetPriceAsOf(otherKind, request.EffectiveFrom)?.Price;
        if (otherCurrent is not null)
        {
            var buying = request.Kind == PriceKind.Buying ? request.Price : otherCurrent.Value;
            var selling = request.Kind == PriceKind.Selling ? request.Price : otherCurrent.Value;
            if (selling < buying)
                throw new DomainException("Selling price cannot be lower than buying price - the company would lose money on every unit sold.");
        }

        var price = product.SchedulePrice(request.Kind, request.Price, request.EffectiveFrom, request.Reason, _currentUser.Email);
        await _context.SaveChangesAsync(cancellationToken);
        return price.Id;
    }
}
