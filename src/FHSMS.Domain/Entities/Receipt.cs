using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A verified payment receipt - distinct from Payment itself. Payment records
/// "money moved"; Receipt is the formal, numbered document proving it, only
/// ever generated after a payment is confirmed. Never delete a receipt: if a
/// payment is reversed, that's a new transaction (refund), not an erased one.
/// </summary>
public class Receipt : AuditableEntity
{
    public string ReceiptNumber { get; private set; } = default!;
    public Guid PaymentId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string Method { get; private set; } = default!;
    public string? TransactionReference { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public string? IssuedBy { get; private set; }

    private Receipt() { } // EF Core

    public Receipt(string receiptNumber, Guid paymentId, Guid invoiceId, decimal amount, string method, string? transactionReference, string? issuedBy)
    {
        ReceiptNumber = receiptNumber;
        PaymentId = paymentId;
        InvoiceId = invoiceId;
        Amount = amount;
        Method = method;
        TransactionReference = transactionReference;
        IssuedAt = DateTime.UtcNow;
        IssuedBy = issuedBy;
    }
}
