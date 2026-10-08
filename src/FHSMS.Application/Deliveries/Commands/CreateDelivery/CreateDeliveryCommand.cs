using MediatR;

namespace FHSMS.Application.Deliveries.Commands.CreateDelivery;

/// <summary>
/// Posts a delivery/trip for an order. DriverId is optional: leave it null
/// to post an open trip any registered driver can accept from the board
/// (Delivery.AssignDriver), or set it to have an admin book a specific
/// registered driver directly (Delivery.AssignDriverByAdmin) - vehicle info
/// then comes from that driver's own profile, never typed by hand, and the
/// driver still has to confirm it themselves before it's final.
/// </summary>
public record CreateDeliveryCommand(
    Guid OrderId,
    string DestinationAddress,
    string? OriginLocation = null,
    decimal? TripPrice = null,
    Guid? DriverId = null) : IRequest<Guid>;
