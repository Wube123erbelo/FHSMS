using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Inventory.Events;

/// <summary>
/// Confirms to the farmer agent that their stock receipt was logged - the
/// receiving-side counterpart to NotifyAgentOnOrderConfirmed on the sales side.
/// Silently skipped for receipts logged with no AgentUserId (e.g. by an admin
/// directly), same as AccrueCommissionOnStockReceived.
/// </summary>
public class NotifyAgentOnStockReceived : INotificationHandler<StockReceivedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyAgentOnStockReceived(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(StockReceivedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.AgentUserId is not { } agentUserId)
            return;

        var agent = await _context.Users.FirstOrDefaultAsync(u => u.Id == agentUserId, cancellationToken);
        if (agent is null || string.IsNullOrWhiteSpace(agent.Email))
            return;

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == notification.ProductId, cancellationToken);
        var productName = product?.Name ?? "product";

        await _notificationService.QueueAsync(
            recipientUserId: agent.Id,
            recipientAddress: agent.Email,
            channel: NotificationChannel.Email,
            subject: "Stock receipt logged - FHSMS",
            body: $"Your stock receipt of {notification.Quantity:N2} {productName} has been logged.",
            cancellationToken: cancellationToken);
    }
}
