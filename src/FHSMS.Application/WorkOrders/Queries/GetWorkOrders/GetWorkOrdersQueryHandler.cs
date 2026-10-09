using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, List<WorkOrderDto>>
{
    private readonly IApplicationDbContext _context;
    public GetWorkOrdersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<WorkOrderDto>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkOrders.AsQueryable();

        if (request.AssignedToUserId is { } userId)
            query = query.Where(w => w.AssignedToUserId == userId);

        if (request.Status is { } status)
            query = query.Where(w => w.Status == status);

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                OrderId = w.OrderId,
                AssignedToUserId = w.AssignedToUserId,
                Status = w.Status,
                Priority = w.Priority,
                DueDate = w.DueDate,
                CompletedAt = w.CompletedAt,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
