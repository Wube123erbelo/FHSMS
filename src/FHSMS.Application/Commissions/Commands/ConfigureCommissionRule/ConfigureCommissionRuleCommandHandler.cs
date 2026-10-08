using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Commands.ConfigureCommissionRule;

public class ConfigureCommissionRuleCommandHandler : IRequestHandler<ConfigureCommissionRuleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public ConfigureCommissionRuleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(ConfigureCommissionRuleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.CommissionRules
            .FirstOrDefaultAsync(r => r.AgentType == request.AgentType && r.IsActive, cancellationToken);

        if (existing != null)
        {
            existing.Update(request.Basis, request.Percentage, request.FlatRateAmount);
            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var rule = new Domain.Entities.CommissionRule(request.AgentType, request.Basis, request.Percentage, request.FlatRateAmount);
        _context.CommissionRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }
}
