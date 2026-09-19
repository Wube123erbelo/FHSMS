using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.BankReconciliation.Commands.RecordBankTransaction;

/// <summary>Imports one line from a bank statement (CSV import or manual entry) as Unmatched.</summary>
public record RecordBankTransactionCommand(
    Guid BankAccountId,
    BankTransactionType Type,
    decimal Amount,
    DateTime TransactionDate,
    string? Description) : IRequest<Guid>;
