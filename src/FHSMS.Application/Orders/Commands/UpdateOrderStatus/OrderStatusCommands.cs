using MediatR;

namespace FHSMS.Application.Orders.Commands.UpdateOrderStatus;

public record ConfirmOrderCommand(Guid OrderId) : IRequest;
public record RejectOrderCommand(Guid OrderId, string? Reason) : IRequest;
public record PrepareOrderCommand(Guid OrderId) : IRequest;
public record ShipOrderCommand(Guid OrderId) : IRequest;
public record CompleteOrderCommand(Guid OrderId) : IRequest;
public record ReturnOrderCommand(Guid OrderId, string? Reason) : IRequest;
