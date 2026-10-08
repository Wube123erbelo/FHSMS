using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>Unit of measure, e.g. Kilogram (kg), Crate, Litre.</summary>
public class Unit : AuditableEntity
{
    /// <summary>Human-readable sequential code (e.g. "UNIT-0001") shown in dropdowns/listings instead of the raw GUID.</summary>
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Abbreviation { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
