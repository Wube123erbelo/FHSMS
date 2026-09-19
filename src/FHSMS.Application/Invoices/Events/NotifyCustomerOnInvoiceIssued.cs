using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Invoices.Events;

/// <summary>Queues an email notification to the customer whenever an invoice is issued.</summary>
public class NotifyCustomerOnInvoiceIssued : INotificationHandler<InvoiceIssuedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyCustomerOnInvoiceIssued(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(InvoiceIssuedEvent notification, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == notification.CustomerId, cancellationToken);

        if (customer is null || string.IsNullOrWhiteSpace(customer.Email))
            return;

        await _notificationService.QueueAsync(
            recipientUserId: null,
            recipientAddress: customer.Email,
            channel: NotificationChannel.Email,
            subject: "New invoice from FHSMS",
            body: $"A new invoice totalling {notification.GrandTotal:N2} ETB has been issued to {customer.Name}.",
            cancellationToken: cancellationToken);
    }
}
