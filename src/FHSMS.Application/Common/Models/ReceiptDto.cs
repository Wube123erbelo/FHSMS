namespace FHSMS.Application.Common.Models;

public class ReceiptDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = default!;
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = default!;
    public string? TransactionReference { get; set; }
    public DateTime IssuedAt { get; set; }
    public string? IssuedBy { get; set; }
}
