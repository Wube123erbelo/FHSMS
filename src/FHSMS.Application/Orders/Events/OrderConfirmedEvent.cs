using MediatR;

namespace FHSMS.Application.Orders.Events;

/// <summary>Published whenever an order transitions into Confirmed status (POST /orders/{id}/confirm), so the agent who placed it can be told without ConfirmOrderCommandHandler needing to know anything about notifications.</summary>
public record OrderConfirmedEvent(Guid OrderId) : INotification;
