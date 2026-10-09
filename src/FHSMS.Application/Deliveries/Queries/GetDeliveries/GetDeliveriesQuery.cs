using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Deliveries.Queries.GetDeliveries;

/// <summary>
/// Lists deliveries directly - no order ID needed. Backs the Deliveries
/// admin table (previously the only way in was "type an order's GUID and
/// look it up", which is exactly the broken, undiscoverable flow this
/// replaces).
/// </summary>
public record GetDeliveriesQuery(DeliveryStatus? Status = null) : IRequest<List<DeliveryDto>>;
