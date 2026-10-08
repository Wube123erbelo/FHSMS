using MediatR;

namespace FHSMS.Application.WorkOrders.Commands.UpdateWorkOrderStatus;

public record StartWorkOrderCommand(Guid WorkOrderId) : IRequest;
public record CompleteWorkOrderCommand(Guid WorkOrderId) : IRequest;
public record CancelWorkOrderCommand(Guid WorkOrderId) : IRequest;
public record ReassignWorkOrderCommand(Guid WorkOrderId, Guid? AssignedToUserId) : IRequest;
