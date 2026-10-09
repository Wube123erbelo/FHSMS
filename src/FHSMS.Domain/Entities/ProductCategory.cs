using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

public class ProductCategory : AuditableEntity
{
    /// <summary>Human-readable sequential code (e.g. "CAT-0001") shown in dropdowns/listings instead of the raw GUID.</summary>
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
