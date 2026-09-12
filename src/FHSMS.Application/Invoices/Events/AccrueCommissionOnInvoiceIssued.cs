using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Invoices.Events;

/// <summary>
/// Accrues a commission (bonus) for the placing agent, if any, whenever an
/// invoice is issued. The bonus amount itself is NOT recomputed here - it was
/// already computed once by HotelAgentBonusCalculator inside
/// GenerateInvoiceCommandHandler and frozen onto the Invoice
/// (Invoice.HotelAgentBonusAmount) before this handler ever runs. This
/// handler's only job is to turn that already-decided number into the
/// agent's payable Commission record.
/// </summary>
public class AccrueCommissionOnInvoiceIssued : INotificationHandler<InvoiceIssuedEvent>
{
    private readonly IApplicationDbContext _context;

    public AccrueCommissionOnInvoiceIssued(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(InvoiceIssuedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.AgentUserId is not { } agentUserId || notification.Bonus.Amount <= 0)
            return; // no agent involved, or nothing accrued for this sale

        var b = notification.Bonus;
        var commission = b.Basis switch
        {
            CommissionBasis.PercentageOfInvoice =>
                Domain.Entities.Commission.ForInvoice(agentUserId, notification.InvoiceId, notification.Subtotal, b.Percentage!.Value),
            CommissionBasis.FlatRatePerQuantity =>
                Domain.Entities.Commission.ForInvoiceFlatRate(agentUserId, notification.InvoiceId, b.Quantity!.Value, b.Amount, b.FlatRateApplied!.Value),
            _ => null
        };

        if (commission is null)
            return;

        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
