using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using FHSMS.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace FHSMS.Infrastructure.Services;

/// <summary>
/// Default notification implementation: persists the notification as Sent
/// immediately after logging it. This is a deliberate stub - swap the body of
/// DispatchAsync for real Email (SMTP/SendGrid), SMS (a local gateway), or
/// Telegram Bot API calls without touching any Application-layer code, since
/// everything upstream only ever talks to INotificationService.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task QueueAsync(
        Guid? recipientUserId,
        string? recipientAddress,
        NotificationChannel channel,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var notification = new Domain.Entities.Notification(recipientUserId, recipientAddress, channel, subject, body);
        _context.Notifications.Add(notification);

        try
        {
            await DispatchAsync(channel, recipientAddress, subject, body, cancellationToken);
            notification.MarkSent();
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);
            _logger.LogWarning(ex, "Failed to dispatch {Channel} notification to {Recipient}", channel, recipientAddress);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Replace this with real provider calls (SMTP client, SMS gateway HTTP call, Telegram Bot API) as each is integrated.</summary>
    private Task DispatchAsync(NotificationChannel channel, string? recipientAddress, string subject, string body, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[STUB {Channel} SEND] To: {Recipient} | Subject: {Subject} | Body: {Body}",
            channel, recipientAddress ?? "(no address)", subject, body);
        return Task.CompletedTask;
    }
}
