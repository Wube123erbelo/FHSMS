using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Extensions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentUserService _currentUser;

    public CreateOrderCommandHandler(
        IApplicationDbContext context, IDocumentNumberGenerator numberGenerator, ICurrentUserService currentUser)
    {
        _context = context;
        _numberGenerator = numberGenerator;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // A farmer agent's job is logging stock received from farmers
        // (RecordInventoryTransactionCommand), not placing hotel orders -
        // enforced here, not just hidden in the UI, since the UI restriction
        // alone doesn't stop a direct API call.
        if (_currentUser.Role == "FarmerAgent")
            throw new UnauthorizedAccessException("Farmer agents record stock received, not hotel orders - use Inventory / stock-in instead.");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Include(p => p.Prices)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        // Who actually gets credit/commission for this order is derived from the
        // authenticated caller, never from a client-supplied AgentUserId - a hotel
        // agent (web, mobile, or Telegram, all calling this same command) cannot
        // place an order "as" a different agent by editing a request body. Callers
        // who are not agents (SuperAdmin creating on someone's behalf, an
        // unauthenticated/public order, a hotel customer ordering for themselves)
        // fall back to whatever the caller explicitly passed - which for a hotel
        // customer's own order is correctly null.
        var isAgent = _currentUser.Role is "HotelAgent" or "FarmerAgent";
        var agentUserId = isAgent ? _currentUser.UserId : request.AgentUserId;
        var source = isAgent
            ? (_currentUser.Role == "HotelAgent" ? OrderSourceType.HotelAgent : OrderSourceType.FarmerAgent)
            : request.Source;

        var orderNumber = await _numberGenerator.NextOrderNumberAsync(cancellationToken);
        var order = new Order(orderNumber, customer.Id, source, agentUserId, request.Notes, request.RequestedDeliveryDate.AsUtc());

        foreach (var item in request.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId)
                ?? throw new NotFoundException(nameof(Product), item.ProductId);

            var currentPrice = product.GetSellingPriceAsOf()
                ?? throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        nameof(item.ProductId), $"Product '{product.Name}' has no active selling price and cannot be ordered.")
                });

            order.AddItem(product.Id, product.Name, item.Quantity, currentPrice.Price);
        }

        order.Submit();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
