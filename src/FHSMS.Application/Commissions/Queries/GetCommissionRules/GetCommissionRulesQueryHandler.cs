using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Queries.GetCommissionRules;

public class GetCommissionRulesQueryHandler : IRequestHandler<GetCommissionRulesQuery, List<CommissionRuleDto>>
{
    private readonly IApplicationDbContext _context;
    public GetCommissionRulesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<CommissionRuleDto>> Handle(GetCommissionRulesQuery request, CancellationToken cancellationToken)
    {
        return await _context.CommissionRules
            .OrderBy(r => r.AgentType)
            .Select(r => new CommissionRuleDto
            {
                Id = r.Id,
                AgentType = r.AgentType,
                Basis = r.Basis,
                Percentage = r.Percentage,
                FlatRateAmount = r.FlatRateAmount,
                IsActive = r.IsActive,
                UnitRates = r.UnitRates.Select(ur => new CommissionUnitRateDto
                {
                    UnitId = ur.UnitId,
                    UnitName = ur.Unit!.Name,
                    UnitAbbreviation = ur.Unit.Abbreviation,
                    RateAmount = ur.RateAmount
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
