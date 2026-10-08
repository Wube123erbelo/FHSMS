namespace FHSMS.Application.Common.Models;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = default!;
    public string? PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? Changes { get; set; }
}
