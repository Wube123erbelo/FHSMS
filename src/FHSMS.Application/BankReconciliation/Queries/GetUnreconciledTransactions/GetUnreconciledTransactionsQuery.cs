using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.BankReconciliation.Queries.GetUnreconciledTransactions;

public record GetUnreconciledTransactionsQuery(Guid? BankAccountId = null) : IRequest<List<BankTransactionDto>>;
