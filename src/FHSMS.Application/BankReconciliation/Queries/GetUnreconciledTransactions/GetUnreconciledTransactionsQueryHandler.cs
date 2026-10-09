using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.BankReconciliation.Queries.GetUnreconciledTransactions;

public class GetUnreconciledTransactionsQueryHandler
    : IRequestHandler<GetUnreconciledTransactionsQuery, List<BankTransactionDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUnreconciledTransactionsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BankTransactionDto>> Handle(GetUnreconciledTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BankTransactions
            .Where(t => t.ReconciliationStatus == ReconciliationStatus.Unmatched);

        if (request.BankAccountId is { } accountId)
            query = query.Where(t => t.BankAccountId == accountId);

        return await query
            .OrderBy(t => t.TransactionDate)
            .Select(t => new BankTransactionDto
            {
                Id = t.Id,
                BankAccountId = t.BankAccountId,
                Type = t.Type,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                Description = t.Description,
                ReconciliationStatus = t.ReconciliationStatus,
                MatchedPaymentId = t.MatchedPaymentId
            })
            .ToListAsync(cancellationToken);
    }
}
