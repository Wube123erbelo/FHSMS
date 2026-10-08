using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Events;

/// <summary>Emails the agent who placed an order once admin confirms it. Orders placed directly (no AgentUserId, or an agent record with no email) are silently skipped - not every order has an agent to notify.</summary>
public class NotifyAgentOnOrderConfirmed : INotificationHandler<OrderConfirmedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyAgentOnOrderConfirmed(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderConfirmedEvent notification, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);

        if (order?.AgentUserId is not { } agentUserId)
            return;

        var agent = await _context.Users.FirstOrDefaultAsync(u => u.Id == agentUserId, cancellationToken);
        if (agent is null || string.IsNullOrWhiteSpace(agent.Email))
            return;

        await _notificationService.QueueAsync(
            recipientUserId: agent.Id,
            recipientAddress: agent.Email,
            channel: NotificationChannel.Email,
            subject: "Order confirmed - FHSMS",
            body: $"Order {order.OrderNumber} has been confirmed and is moving to preparation.",
            cancellationToken: cancellationToken);
    }
}
