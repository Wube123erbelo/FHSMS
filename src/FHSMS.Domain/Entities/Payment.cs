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

    /// <summary>
    /// A customer/agent's self-declared "I already transferred the money" claim.
    /// Deliberately NOT a completed payment: it sits at PaymentStatus.Pending,
    /// leaves the invoice's balance untouched and has no Receipt behind it,
    /// until an admin confirms it against the bank statement. That's what makes
    /// it safe to let a non-admin create one - the thing an agent could abuse
    /// (marking their own invoice settled) simply doesn't happen here.
    /// </summary>
    public static Payment CreateClaim(
        string paymentNumber, Guid invoiceId, decimal amount, PaymentMethod method, string reference, Guid? bankAccountId)
    {
        var claim = new Payment
        {
            PaymentNumber = paymentNumber,
            InvoiceId = invoiceId,
            Amount = amount,
            Method = method,
            Reference = reference,
            BankAccountId = bankAccountId,
            Status = PaymentStatus.Pending,
            PaidAt = DateTime.UtcNow
        };
        return claim;
    }

    public bool IsPending => Status == PaymentStatus.Pending;

    /// <summary>
    /// Promotes a pending claim to a real, completed payment once an admin has
    /// matched it to the bank statement. Confirming happens in place rather than
    /// by inserting a second row, because Reference is uniquely indexed - the
    /// claim and the payment it becomes are the same record.
    /// </summary>
    public void Confirm(decimal? confirmedAmount = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Only a pending payment can be confirmed - this one is {Status}.");

        if (confirmedAmount is { } amount)
            Amount = amount;

        Status = PaymentStatus.Completed;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = PaymentStatus.Failed;
    public void MarkRefunded() => Status = PaymentStatus.Refunded;
}
