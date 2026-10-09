using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Interfaces;

/// <summary>
/// Queues and (via Infrastructure) dispatches a notification over a given channel.
/// Handlers depend on this interface only - swapping the Email/SMS/Telegram
/// provider never touches Application code.
/// </summary>
public interface INotificationService
{
    Task QueueAsync(
        Guid? recipientUserId,
        string? recipientAddress,
        NotificationChannel channel,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
