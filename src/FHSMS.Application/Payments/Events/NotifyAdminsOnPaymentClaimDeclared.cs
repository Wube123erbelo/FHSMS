using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Events;

/// <summary>
/// Tells every active SuperAdmin that a payment claim is waiting to be matched
/// against the bank statement. Without this the claim just sits in the
/// reconciliation queue until somebody happens to look, which is the whole
/// reason a customer's "I already paid" felt like it went nowhere.
/// </summary>
public class NotifyAdminsOnPaymentClaimDeclared : INotificationHandler<PaymentClaimDeclaredEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyAdminsOnPaymentClaimDeclared(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(PaymentClaimDeclaredEvent notification, CancellationToken cancellationToken)
    {
        var admins = await _context.Users
            .Where(u => u.Role == UserRole.SuperAdmin && u.IsActive)
            .ToListAsync(cancellationToken);

        var declaredBy = notification.DeclaredByEmail ?? "a portal user";
        var body =
            $"{declaredBy} reported paying {notification.Amount:N2} ETB toward invoice {notification.InvoiceNumber} " +
            $"(reference {notification.Reference}, claim {notification.PaymentNumber}). " +
            "The invoice balance hasn't changed - confirm it against the bank statement under Bank Reconciliation to apply it.";

        foreach (var admin in admins)
        {
            await _notificationService.QueueAsync(
                recipientUserId: admin.Id,
                recipientAddress: admin.Email,
                channel: NotificationChannel.Email,
                subject: "Payment claim awaiting confirmation",
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
