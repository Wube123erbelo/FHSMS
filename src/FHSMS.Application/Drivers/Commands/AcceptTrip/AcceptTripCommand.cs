using MediatR;

namespace FHSMS.Application.Drivers.Commands.AcceptTrip;

/// <summary>A driver claims an open trip from the board - the mockup's "Accept Trip" button. First to accept wins; see Delivery.AssignDriver.</summary>
public record AcceptTripCommand(Guid DeliveryId) : IRequest;
