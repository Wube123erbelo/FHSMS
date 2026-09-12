using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

public class Delivery : AuditableEntity
{
    public Guid OrderId { get; private set; }
    public string? DriverName { get; private set; }
    public string? VehicleInfo { get; private set; }
    public string DestinationAddress { get; private set; } = default!;
    public DeliveryStatus Status { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? Notes { get; private set; }

    // --- Trip board (Driver Portal) fields ---
    /// <summary>Free-text pickup description (e.g. "Adama Farm") - shown on the trip board alongside DestinationAddress. Not a formal link to a Farmer record, since a single delivery can consolidate produce from more than one.</summary>
    public string? OriginLocation { get; private set; }
    /// <summary>What the accepting/assigned driver is paid for this trip. Set by an admin when the delivery is created; null means "not yet priced" and the trip won't appear enticing on the board, but can still be listed.</summary>
    public decimal? TripPrice { get; private set; }
    /// <summary>The Driver (not the free-text DriverName above) attached to this trip - either self-accepted from the open board (see AssignDriver) or pre-assigned by an admin (see AssignDriverByAdmin), in which case DriverConfirmed starts false until the driver acts on it.</summary>
    public Guid? DriverId { get; private set; }
    /// <summary>
    /// True once the driver themselves has agreed to this trip - either by
    /// accepting it from the open board (immediate) or by confirming a trip
    /// an admin assigned to them directly (see ConfirmAssignment). A driver
    /// never has a trip silently forced onto their day; assignment always
    /// ends with their own explicit yes.
    /// </summary>
    public bool DriverConfirmed { get; private set; }

    // --- Recipient confirmation - closes the loop after MarkDelivered ---
    /// <summary>True once someone on the hotel side (the hotel agent who placed the order, or an admin) has confirmed the goods actually arrived - separate from the driver's own MarkDelivered claim. Order.Complete() requires this.</summary>
    public bool RecipientConfirmed { get; private set; }
    public Guid? RecipientConfirmedByUserId { get; private set; }
    public DateTime? RecipientConfirmedAt { get; private set; }

    // Proof of delivery - captured only at the moment of MarkDelivered, never
    // editable afterwards, so it stands as evidence of what actually happened
    // at handoff rather than a field anyone can revise later.
    public string? ReceivedByName { get; private set; }
    public string? SignatureImageBase64 { get; private set; }
    public string? PhotoUrl { get; private set; }
    public double? DeliveryLatitude { get; private set; }
    public double? DeliveryLongitude { get; private set; }

    private Delivery() { } // EF Core

    public Delivery(Guid orderId, string destinationAddress, string? driverName, string? vehicleInfo, string? originLocation = null, decimal? tripPrice = null)
    {
        OrderId = orderId;
        DestinationAddress = destinationAddress;
        DriverName = driverName;
        VehicleInfo = vehicleInfo;
        OriginLocation = originLocation;
        TripPrice = tripPrice;
        Status = DeliveryStatus.Pending;
    }

    /// <summary>
    /// A driver claims this trip from the open board. Only possible while it's
    /// still Pending and unclaimed - first come, first served, same as the
    /// mockup's "Accept Trip" button implies. DriverName is also filled in
    /// from the driver's profile so existing delivery views (which display
    /// DriverName as free text) show something meaningful without needing to
    /// join to Driver everywhere. Self-accepting counts as confirming - there's
    /// no separate step, unlike an admin-preassigned trip.
    /// </summary>
    public void AssignDriver(Guid driverId, string driverFullName, string? plateNumber)
    {
        if (Status != DeliveryStatus.Pending)
            throw new DomainException("Only a pending, unassigned trip can be accepted.");
        if (DriverId is not null)
            throw new DomainException("This trip has already been accepted by another driver.");

        DriverId = driverId;
        DriverName = driverFullName;
        VehicleInfo = plateNumber ?? VehicleInfo;
        DriverConfirmed = true;
    }

    /// <summary>
    /// An admin assigns a specific registered driver directly when creating
    /// or editing the delivery (picked from the Drivers list, not typed in) -
    /// vehicle info comes from that driver's own profile, never re-typed.
    /// Left unconfirmed until the driver themselves accepts it - see
    /// ConfirmAssignment - so the driver still has the final say over their
    /// own schedule even when an admin books it for them.
    /// </summary>
    public void AssignDriverByAdmin(Guid driverId, string driverFullName, string? plateNumber)
    {
        if (Status != DeliveryStatus.Pending)
            throw new DomainException("Only a pending delivery can have a driver assigned.");

        DriverId = driverId;
        DriverName = driverFullName;
        VehicleInfo = plateNumber ?? VehicleInfo;
        DriverConfirmed = false;
    }

    /// <summary>The driver accepts a trip an admin assigned to them - the "My Trips" confirm action.</summary>
    public void ConfirmAssignment(Guid confirmingDriverId)
    {
        if (DriverId != confirmingDriverId)
            throw new DomainException("This trip isn't assigned to you.");
        if (DriverConfirmed)
            throw new DomainException("This trip is already confirmed.");
        DriverConfirmed = true;
    }

    public void Dispatch()
    {
        if (Status != DeliveryStatus.Pending)
            throw new DomainException("Only a pending delivery can be dispatched.");
        Status = DeliveryStatus.InTransit;
        DispatchedAt = DateTime.UtcNow;
    }

    public void MarkDelivered(
        string? notes,
        string? receivedByName = null,
        string? signatureImageBase64 = null,
        string? photoUrl = null,
        double? latitude = null,
        double? longitude = null)
    {
        if (Status != DeliveryStatus.InTransit)
            throw new DomainException("Only an in-transit delivery can be marked delivered.");
        Status = DeliveryStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        Notes = notes;
        ReceivedByName = receivedByName;
        SignatureImageBase64 = signatureImageBase64;
        PhotoUrl = photoUrl;
        DeliveryLatitude = latitude;
        DeliveryLongitude = longitude;
    }

    public void MarkFailed(string? notes)
    {
        Status = DeliveryStatus.Failed;
        Notes = notes;
    }

    /// <summary>
    /// Someone on the hotel side (whoever placed the order, or an admin)
    /// confirms the goods actually arrived in good order - a check on the
    /// driver's own MarkDelivered claim, not a rubber stamp of it. Only
    /// possible once the driver has already marked the delivery Delivered.
    /// Order.Complete() requires this to be true first.
    /// </summary>
    public void ConfirmReceipt(Guid confirmedByUserId)
    {
        if (Status != DeliveryStatus.Delivered)
            throw new DomainException("Only a delivered trip can be confirmed as received.");
        if (RecipientConfirmed)
            throw new DomainException("This delivery has already been confirmed as received.");

        RecipientConfirmed = true;
        RecipientConfirmedByUserId = confirmedByUserId;
        RecipientConfirmedAt = DateTime.UtcNow;
    }
}
