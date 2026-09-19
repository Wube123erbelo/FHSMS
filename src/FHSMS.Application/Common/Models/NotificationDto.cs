using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
