using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery(string? EntityName = null, Guid? EntityId = null, int Page = 1, int PageSize = 50)
    : IRequest<List<AuditLogDto>>;
