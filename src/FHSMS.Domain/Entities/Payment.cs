using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Entities;

public class Payment : AuditableEntity
{
    public string PaymentNumber { get; private set; } = default!;
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? Reference { get; private set; }
    public Guid? BankAccountId { get; private set; }
    public string? ProviderResponse { get; private set; }
    public DateTime PaidAt { get; private set; }

    private Payment() { } // EF Core

    public Payment(string paymentNumber, Guid invoiceId, decimal amount, PaymentMethod method, string? reference, Guid? bankAccountId = null, string? providerResponse = null)
    {
        PaymentNumber = paymentNumber;
        InvoiceId = invoiceId;
        Amount = amount;
        Method = method;
        Reference = reference;
        BankAccountId = bankAccountId;
        ProviderResponse = providerResponse;
        Status = PaymentStatus.Completed;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = PaymentStatus.Failed;
    public void MarkRefunded() => Status = PaymentStatus.Refunded;
}
