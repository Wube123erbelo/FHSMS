using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.WorkOrders.Commands.CreateWorkOrder;

public record CreateWorkOrderCommand(
    string Title, string? Description, Guid? OrderId, Guid? AssignedToUserId,
    WorkOrderPriority Priority, DateTime? DueDate) : IRequest<Guid>;
