using MediatR;

namespace FHSMS.Application.Orders.Events;

/// <summary>
/// Published exactly once, whenever an order actually transitions into
/// Shipped status - whether that happened via POST /orders/{id}/ship
/// directly, or indirectly by dispatching the order's Delivery (see
/// DispatchDeliveryCommandHandler). Both paths publish this so there is one
/// single place ("what happens when an order ships") rather than two
/// slightly-different copies of the same logic.
/// </summary>
public record OrderShippedEvent(Guid OrderId) : INotification;
