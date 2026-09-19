using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

/// <summary>A self-declared "I already paid" claim awaiting (or having had) an admin's verdict.</summary>
public class PaymentClaimDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = default!;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public string? CustomerName { get; set; }
    public decimal Amount { get; set; }
    /// <summary>What's still outstanding on the invoice - what an approval would be checked against.</summary>
    public decimal InvoiceBalanceDue { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Reference { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public string? DeclaredBy { get; set; }
    public DateTime DeclaredAt { get; set; }
}
