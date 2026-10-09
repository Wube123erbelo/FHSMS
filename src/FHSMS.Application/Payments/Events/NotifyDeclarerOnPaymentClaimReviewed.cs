using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Events;

/// <summary>
/// Closes the loop for whoever filed the claim. The claim's CreatedBy is the
/// audit interceptor's record of the acting user's email, so this resolves it
/// back to a User to land in their in-app notifications as well as their inbox;
/// a claim filed by someone with no matching user record is skipped rather than
/// guessed at.
/// </summary>
public class NotifyDeclarerOnPaymentClaimReviewed : INotificationHandler<PaymentClaimReviewedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyDeclarerOnPaymentClaimReviewed(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(PaymentClaimReviewedEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.DeclaredBy))
            return;

        var declarer = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == notification.DeclaredBy, cancellationToken);

        if (declarer is null)
            return;

        var body = notification.Approved
            ? $"Your payment of {notification.Amount:N2} ETB for invoice {notification.InvoiceNumber} has been confirmed against our bank statement and applied. A receipt is now available on the invoice."
            : $"We couldn't match your reported payment of {notification.Amount:N2} ETB for invoice {notification.InvoiceNumber} to our bank statement, so the invoice is still showing as outstanding.";

        if (!string.IsNullOrWhiteSpace(notification.Note))
            body += $" Note from the team: {notification.Note}";

        await _notificationService.QueueAsync(
            recipientUserId: declarer.Id,
            recipientAddress: declarer.Email,
            channel: NotificationChannel.Email,
            subject: notification.Approved ? "Payment confirmed" : "Payment could not be confirmed",
            body: body,
            cancellationToken: cancellationToken);
    }
}
