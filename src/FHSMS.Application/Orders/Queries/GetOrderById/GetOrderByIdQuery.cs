using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
