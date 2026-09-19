using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOrdersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .AsQueryable();

        // A hotel agent only ever sees their own orders here - this is their
        // "my orders" tracking view, not a system-wide list (that's admin's
        // /orders, same endpoint, no filter applied to their role). Every
        // other authenticated role is unaffected.
        if (_currentUser.Role == "HotelAgent" && _currentUser.UserId is { } agentUserId)
            query = query.Where(o => o.AgentUserId == agentUserId);

        if (request.CustomerId is { } customerId)
            query = query.Where(o => o.CustomerId == customerId);

        if (request.Status is { } status)
            query = query.Where(o => o.Status == status);

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync(cancellationToken);

        // One extra query for the truck/trip side of every order returned,
        // rather than a per-row lookup - lets the list double as a tracking
        // view (see OrderDto.DeliveryStatus) without an N+1 fetch. Materialize
        // first, then project to string in memory - enum.ToString() isn't
        // reliably translatable to SQL across providers.
        var orderIds = orders.Select(o => o.Id).ToList();
        var deliveries = await _context.Deliveries
            .Where(d => orderIds.Contains(d.OrderId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
        // GroupBy + First rather than ToDictionary - an order can end up with
        // more than one Delivery row over its life (a failed trip reposted),
        // and ToDictionary would throw on the duplicate key. Ordered above by
        // newest first, so this keeps the most recent trip's status.
        var deliveryStatusByOrderId = deliveries
            .GroupBy(d => d.OrderId)
            .ToDictionary(g => g.Key, g => g.First().Status.ToString());

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name,
            Status = order.Status,
            Source = order.Source,
            OrderDate = order.OrderDate,
            RequestedDeliveryDate = order.RequestedDeliveryDate,
            Subtotal = order.GetSubtotal(),
            Notes = order.Notes,
            AgentUserId = order.AgentUserId,
            DeliveryStatus = deliveryStatusByOrderId.GetValueOrDefault(order.Id),
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        }).ToList();
    }
}
