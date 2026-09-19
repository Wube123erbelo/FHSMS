using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A registered truck driver's profile - one per User account with role
/// Driver (admin-issued credentials, same pattern as HotelAgent/FarmerAgent;
/// see RegisterCommandHandler.SelfRegisterableRoles). A driver logs in, then
/// fills in or updates this profile themselves (name/phone/plate/truck type),
/// and uses it to see and accept trips on the board (see Delivery.DriverId /
/// AssignDriver) - this is what the client's "Driver Portal" mockup shows.
/// </summary>
public class Driver : AuditableEntity
{
    /// <summary>Human-readable sequential code (e.g. "DRV-0001") shown in dropdowns instead of a raw GUID.</summary>
    public string Code { get; set; } = default!;

    /// <summary>The linked login account - one driver profile per User, one User per driver.</summary>
    public Guid UserId { get; set; }

    public string FullName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? PlateNumber { get; set; }
    public TruckType TruckType { get; set; } = TruckType.Other;
    public bool IsActive { get; set; } = true;
}
