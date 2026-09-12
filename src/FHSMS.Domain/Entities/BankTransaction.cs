using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A raw line from a bank statement import. Reconciliation is the process of
/// matching these against Payment records - kept separate from Payment itself
/// because a bank line might arrive days after the payment was recorded, or
/// might never match anything (bank fees, unrelated transfers, etc).
/// </summary>
public class BankTransaction : AuditableEntity
{
    public Guid BankAccountId { get; private set; }
    public BankTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public string? Description { get; private set; }
    public ReconciliationStatus ReconciliationStatus { get; private set; }
    public Guid? MatchedPaymentId { get; private set; }

    private BankTransaction() { } // EF Core

    public BankTransaction(Guid bankAccountId, BankTransactionType type, decimal amount, DateTime transactionDate, string? description)
    {
        BankAccountId = bankAccountId;
        Type = type;
        Amount = amount;
        TransactionDate = transactionDate;
        Description = description;
        ReconciliationStatus = ReconciliationStatus.Unmatched;
    }

    public void MatchToPayment(Guid paymentId)
    {
        MatchedPaymentId = paymentId;
        ReconciliationStatus = ReconciliationStatus.Matched;
    }

    public void MarkDisputed() => ReconciliationStatus = ReconciliationStatus.Disputed;

    public void ClearMatch()
    {
        MatchedPaymentId = null;
        ReconciliationStatus = ReconciliationStatus.Unmatched;
    }
}
