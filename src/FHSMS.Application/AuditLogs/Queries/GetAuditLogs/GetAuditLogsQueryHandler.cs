using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAuditLogsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(a => a.EntityName == request.EntityName);

        if (request.EntityId is { } id)
            query = query.Where(a => a.EntityId == id);

        return await query
            .OrderByDescending(a => a.PerformedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action,
                PerformedBy = a.PerformedBy,
                PerformedAt = a.PerformedAt,
                Changes = a.Changes
            })
            .ToListAsync(cancellationToken);
    }
}
