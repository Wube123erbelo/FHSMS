using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.WorkOrders.Queries.GetWorkOrders;

public record GetWorkOrdersQuery(Guid? AssignedToUserId = null, WorkOrderStatus? Status = null) : IRequest<List<WorkOrderDto>>;
