using FHSMS.Application.Common.Extensions;
using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Reports.Queries.GetCommissionSummary;

/// <summary>Commission totals grouped by the agent's role (HotelAgent / FarmerAgent).</summary>
public class GetCommissionSummaryQueryHandler : IRequestHandler<GetCommissionSummaryQuery, List<CommissionSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    public GetCommissionSummaryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<CommissionSummaryDto>> Handle(GetCommissionSummaryQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.AsUtc() ?? DateTime.UtcNow.AddMonths(-1);
        var to = request.To.AsUtc() ?? DateTime.UtcNow;

        var commissions = await _context.Commissions
            .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var agentIds = commissions.Select(c => c.AgentUserId).Distinct().ToList();
        var agentRoles = await _context.Users
            .Where(u => agentIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Role.ToString(), cancellationToken);

        return commissions
            .GroupBy(c => agentRoles.TryGetValue(c.AgentUserId, out var role) ? role : "Unknown")
            .Select(g => new CommissionSummaryDto
            {
                AgentType = g.Key,
                CommissionCount = g.Count(),
                TotalCommission = g.Sum(c => c.CommissionAmount)
            })
            .OrderByDescending(c => c.TotalCommission)
            .ToList();
    }
}
