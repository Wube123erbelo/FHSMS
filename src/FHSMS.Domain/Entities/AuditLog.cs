using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single row describing "who changed what, when". Written automatically by
/// the SaveChanges interceptor in Infrastructure for every insert/update/delete
/// of an AuditableEntity, so individual command handlers never need to remember
/// to log anything themselves.
/// </summary>
public class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = default!; // Created / Modified / Deleted
    public string? PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? Changes { get; set; } // JSON snapshot of changed properties
}
