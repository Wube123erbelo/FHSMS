using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Commands.SetCommissionUnitRate;

public class SetCommissionUnitRateCommandHandler : IRequestHandler<SetCommissionUnitRateCommand>
{
    private readonly IApplicationDbContext _context;
    public SetCommissionUnitRateCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(SetCommissionUnitRateCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.CommissionRules
            .Include(r => r.UnitRates)
            .FirstOrDefaultAsync(r => r.Id == request.CommissionRuleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.CommissionRule), request.CommissionRuleId);

        var unitExists = await _context.Units.AnyAsync(u => u.Id == request.UnitId, cancellationToken);
        if (!unitExists)
            throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId);

        rule.SetUnitRate(request.UnitId, request.RateAmount);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
