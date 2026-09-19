using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public OrderSourceType Source { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public decimal Subtotal { get; set; }
    public string? Notes { get; set; }
    public Guid? AgentUserId { get; set; }
    public string? AgentUserName { get; set; }
    /// <summary>What the agent who placed this order is projected to earn once it's invoiced, at the currently configured HotelAgent commission rate - shown on the admin's delivery-creation screen alongside driver trip price so the two payouts are visible separately. Not yet an accrued Commission record (that only happens on invoicing); this is an estimate.</summary>
    public decimal? EstimatedAgentCommission { get; set; }
    /// <summary>The truck/trip side of this order (see DeliveryDto) - null means no delivery has been posted for pickup yet, which is normal for a fresh order and not an error. Lets an order list double as a tracking view without a separate lookup per row.</summary>
    public string? DeliveryStatus { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
