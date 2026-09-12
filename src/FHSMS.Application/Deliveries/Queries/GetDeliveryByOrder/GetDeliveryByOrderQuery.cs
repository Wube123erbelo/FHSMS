using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Deliveries.Queries.GetDeliveryByOrder;

public record GetDeliveryByOrderQuery(Guid OrderId) : IRequest<DeliveryDto?>;
