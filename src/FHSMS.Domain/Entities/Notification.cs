using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Entities;

/// <summary>Queued outbound notification. Sending itself is an Infrastructure concern (SMS/Email/Telegram provider).</summary>
public class Notification : AuditableEntity
{
    public Guid? RecipientUserId { get; private set; }
    public string? RecipientAddress { get; private set; } // phone/email/chat id when there's no User record
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? FailureReason { get; private set; }
    /// <summary>Whether the recipient has opened/viewed this in their notifications list - distinct from Status, which tracks whether the system successfully sent it.</summary>
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { } // EF Core

    public Notification(Guid? recipientUserId, string? recipientAddress, NotificationChannel channel, string subject, string body)
    {
        RecipientUserId = recipientUserId;
        RecipientAddress = recipientAddress;
        Channel = channel;
        Subject = subject;
        Body = body;
        Status = NotificationStatus.Pending;
    }

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;
    }

    public void MarkRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
