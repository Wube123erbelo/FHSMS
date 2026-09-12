using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A supply-side farmer, distinct from Customer (which represents the
/// buy-side hotels). Farmers are who FHSMS receives stock from - see
/// InventoryTransaction.Receive and the FarmerAgent role that places orders
/// on a farmer's behalf.
/// </summary>
public class Farmer : AuditableEntity
{
    /// <summary>Human-readable sequential code (e.g. "FARM-0001") shown in dropdowns instead of a raw GUID.</summary>
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? BankAccountNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
