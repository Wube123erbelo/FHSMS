using MediatR;

namespace FHSMS.Application.Deliveries.Queries.GetDeliverySignature;

/// <summary>Separate from the main Delivery DTO so signature image bytes aren't pulled into every list/summary response.</summary>
public record GetDeliverySignatureQuery(Guid DeliveryId) : IRequest<string?>;
