using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// Order state machine per the design spec:
///   Draft -> Pending -> Confirmed -> Preparing -> Shipped -> Delivered -> Completed
///   Pending -> Rejected
///   Draft/Pending/Confirmed/Preparing -> Cancelled
///   Shipped/Delivered -> Returned
/// Every transition method below enforces its own allowed-from states and
/// throws DomainException otherwise - there is no path to an invalid status
/// change that doesn't go through one of these methods.
/// </summary>
public class Order : AuditableEntity
{
    public string OrderNumber { get; private set; } = default!;
    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public Guid? AgentUserId { get; private set; }
    public OrderSourceType Source { get; private set; }
    public DateTime OrderDate { get; private set; }
    /// <summary>The date whoever placed the order asked for delivery by - shown to admin/dispatch when scheduling a driver, not a promise the system enforces on its own.</summary>
    public DateTime? RequestedDeliveryDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? StatusNote { get; private set; }
    /// <summary>Free-text note from whoever placed the order (e.g. delivery instructions) - distinct from StatusNote, which records why a status changed.</summary>
    public string? Notes { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // EF Core

    public Order(string orderNumber, Guid customerId, OrderSourceType source, Guid? agentUserId = null, string? notes = null, DateTime? requestedDeliveryDate = null)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Source = source;
        AgentUserId = agentUserId;
        Notes = notes;
        RequestedDeliveryDate = requestedDeliveryDate;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Draft;
    }

    public void AddItem(Guid productId, string productName, decimal quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Items can only be added while the order is still a draft.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        _items.Add(new OrderItem(Id, productId, productName, quantity, unitPrice));
    }

    /// <summary>Draft -> Pending. The customer/agent has submitted the order for review.</summary>
    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only a draft order can be submitted.");
        if (!_items.Any())
            throw new DomainException("Cannot submit an order with no items.");
        Status = OrderStatus.Pending;
    }

    /// <summary>Pending -> Confirmed. Operations accepts the order.</summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only a pending order can be confirmed.");
        Status = OrderStatus.Confirmed;
    }

    /// <summary>Pending -> Rejected. Operations declines the order (e.g. out of stock, invalid request).</summary>
    public void Reject(string? reason)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only a pending order can be rejected.");
        Status = OrderStatus.Rejected;
        StatusNote = reason;
    }

    /// <summary>Confirmed -> Preparing. Stock is being picked/packed.</summary>
    public void Prepare()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("Only a confirmed order can enter preparation.");
        Status = OrderStatus.Preparing;
    }

    /// <summary>Preparing -> Shipped. Dispatched for delivery.</summary>
    public void Ship()
    {
        if (Status != OrderStatus.Preparing)
            throw new DomainException("Only an order in preparation can be shipped.");
        Status = OrderStatus.Shipped;
    }

    /// <summary>Shipped -> Delivered. Customer received the goods.</summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Only a shipped order can be marked delivered.");
        Status = OrderStatus.Delivered;
    }

    /// <summary>Delivered -> Completed. The business transaction is closed.</summary>
    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new DomainException("Only a delivered order can be completed.");
        Status = OrderStatus.Completed;
    }

    /// <summary>Shipped/Delivered -> Returned. Goods came back - refund/credit workflow picks up from here.</summary>
    public void Return(string? reason)
    {
        if (Status is not (OrderStatus.Shipped or OrderStatus.Delivered))
            throw new DomainException("Only a shipped or delivered order can be returned.");
        Status = OrderStatus.Returned;
        StatusNote = reason;
    }

    /// <summary>Draft/Pending/Confirmed/Preparing -> Cancelled. Not allowed once shipped - use Return instead.</summary>
    public void Cancel()
    {
        if (Status is not (OrderStatus.Draft or OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Preparing))
            throw new DomainException("This order can no longer be cancelled - it has already shipped. Use Return instead.");
        Status = OrderStatus.Cancelled;
    }

    public decimal GetSubtotal() => _items.Sum(i => i.LineTotal);
}
