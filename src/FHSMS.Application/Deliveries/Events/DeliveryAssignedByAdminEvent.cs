using MediatR;

namespace FHSMS.Application.Deliveries.Events;

/// <summary>
/// Raised when an admin assigns a registered driver to a delivery directly
/// (Delivery.AssignDriverByAdmin), as opposed to a driver self-accepting an
/// open trip from the board - a driver who was picked for them, rather than
/// one who chose the trip themselves, needs to actually be told.
/// </summary>
public record DeliveryAssignedByAdminEvent(
    Guid DeliveryId,
    Guid DriverId,
    string DestinationAddress,
    decimal? TripPrice) : INotification;
