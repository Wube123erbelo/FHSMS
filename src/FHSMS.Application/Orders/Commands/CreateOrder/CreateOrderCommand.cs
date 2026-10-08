using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Orders.Commands.CreateOrder;

public record CreateOrderItemDto(Guid ProductId, decimal Quantity);

/// <summary>
/// Creates and immediately confirms an order. Telegram, the PWA, and any future
/// mobile app all funnel through this exact same command - there is no separate
/// "Telegram order" code path with its own rules.
/// </summary>
public record CreateOrderCommand(
    Guid CustomerId,
    OrderSourceType Source,
    Guid? AgentUserId,
    List<CreateOrderItemDto> Items,
    string? Notes = null,
    DateTime? RequestedDeliveryDate = null) : IRequest<Guid>;
