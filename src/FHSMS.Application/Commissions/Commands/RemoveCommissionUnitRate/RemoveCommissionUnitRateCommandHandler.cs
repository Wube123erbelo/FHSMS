using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Commands.RemoveCommissionUnitRate;

public class RemoveCommissionUnitRateCommandHandler : IRequestHandler<RemoveCommissionUnitRateCommand>
{
    private readonly IApplicationDbContext _context;
    public RemoveCommissionUnitRateCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RemoveCommissionUnitRateCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.CommissionRules
            .Include(r => r.UnitRates)
            .FirstOrDefaultAsync(r => r.Id == request.CommissionRuleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.CommissionRule), request.CommissionRuleId);

        rule.RemoveUnitRate(request.UnitId);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
