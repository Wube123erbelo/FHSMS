using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Events;

/// <summary>Texts the customer once their order ships - SMS rather than email, since a customer (unlike an internal agent) isn't assumed to have a login or check email, but a phone number is on file for nearly every customer record.</summary>
public class NotifyCustomerOnOrderShipped : INotificationHandler<OrderShippedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyCustomerOnOrderShipped(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderShippedEvent notification, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);
        if (order is null) return;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);
        if (customer is null || string.IsNullOrWhiteSpace(customer.Phone))
            return;

        await _notificationService.QueueAsync(
            recipientUserId: null,
            recipientAddress: customer.Phone,
            channel: NotificationChannel.Sms,
            subject: "Order shipped",
            body: $"Hi {customer.Name}, your order {order.OrderNumber} has shipped.",
            cancellationToken: cancellationToken);
    }
}
