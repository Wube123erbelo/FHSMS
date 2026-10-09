using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Queries.GetOrdersAwaitingDelivery;

public class GetOrdersAwaitingDeliveryQueryHandler : IRequestHandler<GetOrdersAwaitingDeliveryQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    public GetOrdersAwaitingDeliveryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<OrderDto>> Handle(GetOrdersAwaitingDeliveryQuery request, CancellationToken cancellationToken)
    {
        var eligibleStatuses = new[] { OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Shipped };

        var deliveredOrderIds = await _context.Deliveries.Select(d => d.OrderId).ToListAsync(cancellationToken);

        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => eligibleStatuses.Contains(o.Status) && !deliveredOrderIds.Contains(o.Id))
            .OrderBy(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
        var customers = await _context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var agentIds = orders.Where(o => o.AgentUserId.HasValue).Select(o => o.AgentUserId!.Value).Distinct().ToList();
        var agents = await _context.Users
            .Where(u => agentIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        // Informational only (see OrderDto.EstimatedAgentCommission remarks) -
        // mirrors AccrueCommissionOnInvoiceIssued exactly (both bases) so the
        // admin sees the same number here that will actually accrue once the
        // order is invoiced, not a stale percentage-only estimate.
        var hotelAgentRule = await _context.CommissionRules
            .FirstOrDefaultAsync(r => r.AgentType == AgentType.HotelAgent && r.IsActive, cancellationToken);

        var allProductIds = orders.SelectMany(o => o.Items).Select(i => i.ProductId).Distinct().ToList();
        var productUnits = hotelAgentRule is { Basis: CommissionBasis.FlatRatePerQuantity }
            ? await _context.Products.Where(p => allProductIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.UnitId, cancellationToken)
            : new Dictionary<Guid, Guid>();

        return orders.Select(order =>
        {
            var agent = order.AgentUserId is { } agentId && agents.TryGetValue(agentId, out var a) ? a : null;
            decimal? estimatedCommission = null;

            if (agent is { Role: UserRole.HotelAgent } && hotelAgentRule is not null)
            {
                if (hotelAgentRule is { Basis: CommissionBasis.PercentageOfInvoice, Percentage: { } pct })
                {
                    estimatedCommission = Math.Round(order.GetSubtotal() * pct / 100m, 2);
                }
                else if (hotelAgentRule.Basis == CommissionBasis.FlatRatePerQuantity)
                {
                    decimal total = 0m;
                    foreach (var item in order.Items)
                    {
                        if (!productUnits.TryGetValue(item.ProductId, out var unitId)) continue;
                        if (hotelAgentRule.GetEffectiveFlatRate(unitId) is not { } rate) continue;
                        total += Math.Round(item.Quantity * rate, 2);
                    }
                    estimatedCommission = total > 0 ? total : null;
                }
            }

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = customers.TryGetValue(order.CustomerId, out var c) ? c.Name : null,
                Status = order.Status,
                OrderDate = order.OrderDate,
                RequestedDeliveryDate = order.RequestedDeliveryDate,
                Subtotal = order.GetSubtotal(),
                Notes = order.Notes,
                AgentUserId = order.AgentUserId,
                AgentUserName = agent?.FullName,
                EstimatedAgentCommission = estimatedCommission,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            };
        }).ToList();
    }
}
