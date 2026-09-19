using MediatR;

namespace FHSMS.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Cancels an order. Deliberately a state transition, not a delete - once an
/// order exists it stays in the system for audit purposes even if it never
/// gets fulfilled.
/// </summary>
public record CancelOrderCommand(Guid OrderId) : IRequest;
