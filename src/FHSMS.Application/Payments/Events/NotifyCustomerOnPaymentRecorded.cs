using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Events;

/// <summary>Queues an email receipt to the customer whenever a payment posts to their invoice, whichever of the payment paths (manual, online checkout, self-verify) triggered it.</summary>
public class NotifyCustomerOnPaymentRecorded : INotificationHandler<PaymentRecordedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyCustomerOnPaymentRecorded(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(PaymentRecordedEvent notification, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == notification.CustomerId, cancellationToken);

        if (customer is null || string.IsNullOrWhiteSpace(customer.Email))
            return;

        await _notificationService.QueueAsync(
            recipientUserId: null,
            recipientAddress: customer.Email,
            channel: NotificationChannel.Email,
            subject: "Payment received - FHSMS",
            body: $"Thank you, {customer.Name}. We've received your payment of {notification.Amount:N2} ETB (receipt {notification.ReceiptNumber}, payment {notification.PaymentNumber}).",
            cancellationToken: cancellationToken);
    }
}
