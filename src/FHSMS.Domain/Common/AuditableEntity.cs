namespace FHSMS.Domain.Common;

/// <summary>
/// Entities that need to record who created/modified them and when.
/// Populated automatically by a SaveChanges interceptor in the Infrastructure layer.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
