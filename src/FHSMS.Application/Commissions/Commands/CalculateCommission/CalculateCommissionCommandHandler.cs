using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Commands.CalculateCommission;

/// <summary>
/// Manual/administrative re-trigger of commission accrual for an already-issued
/// invoice (the normal path is automatic via AccrueCommissionOnInvoiceIssued).
/// Only applies to agent types whose rule is currently on a percentage-of-invoice
/// basis.
/// </summary>
public class CalculateCommissionCommandHandler : IRequestHandler<CalculateCommissionCommand, Guid?>
{
    private readonly IApplicationDbContext _context;
    public CalculateCommissionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid?> Handle(CalculateCommissionCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        var rule = await _context.CommissionRules
            .FirstOrDefaultAsync(r => r.AgentType == request.AgentType && r.IsActive, cancellationToken);

        // No rule configured, or the rule is on a flat-rate-per-quantity basis
        // (doesn't apply to invoices) -> no commission accrues. Deliberate, same
        // as tax: opt-in configuration, not a default.
        if (rule is null || rule.Basis != CommissionBasis.PercentageOfInvoice || rule.Percentage is not { } percentage)
            return null;

        var commission = Domain.Entities.Commission.ForInvoice(request.AgentUserId, invoice.Id, invoice.Subtotal, percentage);

        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync(cancellationToken);
        return commission.Id;
    }
}
