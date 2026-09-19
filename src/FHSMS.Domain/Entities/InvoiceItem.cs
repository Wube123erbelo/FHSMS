using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single invoice line. Every tax figure here is computed once, at generation
/// time, by the TaxEngine and then frozen forever - it is never recalculated from
/// the "current" tax configuration.
/// </summary>
public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineSubtotal { get; private set; }

    public TaxProfileType TaxProfileApplied { get; private set; }
    public decimal TaxRateApplied { get; private set; }
    public decimal TaxableAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }

    private InvoiceItem() { } // EF Core

    public InvoiceItem(
        Guid invoiceId,
        Guid productId,
        string productName,
        decimal quantity,
        decimal unitPrice,
        decimal lineSubtotal,
        TaxProfileType taxProfileApplied,
        decimal taxRateApplied,
        decimal taxableAmount,
        decimal taxAmount,
        decimal lineTotal)
    {
        InvoiceId = invoiceId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineSubtotal = lineSubtotal;
        TaxProfileApplied = taxProfileApplied;
        TaxRateApplied = taxRateApplied;
        TaxableAmount = taxableAmount;
        TaxAmount = taxAmount;
        LineTotal = lineTotal;
    }
}
