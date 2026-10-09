using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public bool TaxWasEnabled { get; set; }
    public TaxCalculationMode? TaxMode { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal PlatformCommissionRateApplied { get; set; }
    public decimal PlatformCommissionAmount { get; set; }
    public decimal HotelAgentBonusAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineSubtotal { get; set; }
    public TaxProfileType TaxProfileApplied { get; set; }
    public decimal TaxRateApplied { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}
