using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.WorkOrders.Commands.UpdateWorkOrderStatus;

public class StartWorkOrderCommandHandler : IRequestHandler<StartWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public StartWorkOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(StartWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);
        workOrder.Start();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CompleteWorkOrderCommandHandler : IRequestHandler<CompleteWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public CompleteWorkOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);
        workOrder.Complete();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CancelWorkOrderCommandHandler : IRequestHandler<CancelWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public CancelWorkOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CancelWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);
        workOrder.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class ReassignWorkOrderCommandHandler : IRequestHandler<ReassignWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public ReassignWorkOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReassignWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);
        workOrder.Reassign(request.AssignedToUserId);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
