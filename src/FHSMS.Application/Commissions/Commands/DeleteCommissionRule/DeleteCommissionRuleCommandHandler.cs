using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Commands.DeleteCommissionRule;

public class DeleteCommissionRuleCommandHandler : IRequestHandler<DeleteCommissionRuleCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteCommissionRuleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteCommissionRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.CommissionRules.FirstOrDefaultAsync(r => r.Id == request.CommissionRuleId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommissionRule), request.CommissionRuleId);

        rule.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
