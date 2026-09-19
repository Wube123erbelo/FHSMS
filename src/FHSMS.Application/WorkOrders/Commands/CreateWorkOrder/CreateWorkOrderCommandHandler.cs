using FHSMS.Application.Common.Interfaces;
using MediatR;

namespace FHSMS.Application.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateWorkOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = new Domain.Entities.WorkOrder(
            request.Title, request.Description, request.OrderId, request.AssignedToUserId, request.Priority, request.DueDate);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);
        return workOrder.Id;
    }
}
