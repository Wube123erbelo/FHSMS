using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Orders.Queries.GetOrdersAwaitingDelivery;

/// <summary>Orders that are paid and ready to move (Confirmed/Preparing/Shipped) but don't have a delivery/trip posted yet - the admin's "Create delivery" order picker, replacing free-text order-ID entry.</summary>
public record GetOrdersAwaitingDeliveryQuery : IRequest<List<OrderDto>>;
