using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.BankReconciliation.Commands.ReconcileTransaction;

public class ReconcileTransactionCommandHandler : IRequestHandler<ReconcileTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    public ReconcileTransactionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReconcileTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.BankTransactions
            .FirstOrDefaultAsync(t => t.Id == request.BankTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(BankTransaction), request.BankTransactionId);

        var paymentExists = await _context.Payments.AnyAsync(p => p.Id == request.PaymentId, cancellationToken);
        if (!paymentExists)
            throw new NotFoundException(nameof(Payment), request.PaymentId);

        transaction.MatchToPayment(request.PaymentId);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class MarkTransactionDisputedCommandHandler : IRequestHandler<MarkTransactionDisputedCommand>
{
    private readonly IApplicationDbContext _context;
    public MarkTransactionDisputedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(MarkTransactionDisputedCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.BankTransactions
            .FirstOrDefaultAsync(t => t.Id == request.BankTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(BankTransaction), request.BankTransactionId);

        transaction.MarkDisputed();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
