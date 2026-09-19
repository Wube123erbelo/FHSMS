using FHSMS.Application.Common.Interfaces;
using MediatR;

namespace FHSMS.Application.BankReconciliation.Commands.RecordBankTransaction;

public class RecordBankTransactionCommandHandler : IRequestHandler<RecordBankTransactionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public RecordBankTransactionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(RecordBankTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Domain.Entities.BankTransaction(
            request.BankAccountId, request.Type, request.Amount, request.TransactionDate, request.Description);

        _context.BankTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction.Id;
    }
}
