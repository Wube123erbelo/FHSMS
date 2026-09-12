using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class BankTransactionDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public BankTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public Guid? MatchedPaymentId { get; set; }
}
