using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Invoices.Events;

/// <summary>Queues an email notification to the customer whenever their invoice is cancelled.</summary>
public class NotifyCustomerOnInvoiceCancelled : INotificationHandler<InvoiceCancelledEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyCustomerOnInvoiceCancelled(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(InvoiceCancelledEvent notification, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == notification.CustomerId, cancellationToken);

        if (customer is null || string.IsNullOrWhiteSpace(customer.Email))
            return;

        await _notificationService.QueueAsync(
            recipientUserId: null,
            recipientAddress: customer.Email,
            channel: NotificationChannel.Email,
            subject: "Invoice cancelled - FHSMS",
            body: $"Hi {customer.Name}, invoice {notification.InvoiceNumber} has been cancelled. If you already paid toward it, our team will be in touch about next steps.",
            cancellationToken: cancellationToken);
    }
}
