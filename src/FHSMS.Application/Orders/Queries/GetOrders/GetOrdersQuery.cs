using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery(Guid? CustomerId = null, OrderStatus? Status = null) : IRequest<List<OrderDto>>;
