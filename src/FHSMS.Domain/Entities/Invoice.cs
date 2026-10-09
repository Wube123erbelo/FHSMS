using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// Once an invoice is issued, every tax-related figure on it (rate, mode, amounts)
/// is a frozen snapshot. If the Admin later changes the VAT rate or turns VAT off,
/// this invoice is completely unaffected - exactly the "historical invoices must
/// keep the tax rate that was applied when they were created" requirement.
/// </summary>
public class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; private set; } = default!;
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public InvoiceStatus Status { get; private set; }

    // Snapshot of the tax configuration used, kept for audit/reporting even though
    // each line also stores its own figures.
    public Guid? TaxConfigurationId { get; private set; }
    public bool TaxWasEnabled { get; private set; }
    public TaxCalculationMode? TaxMode { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal TaxableAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Discount { get; private set; }

    // --- Company-side charges layered on top of the sale, frozen at issue time ---
    // Snapshot of the platform-commission configuration used, kept for audit/reporting.
    public Guid? PlatformCommissionConfigurationId { get; private set; }
    /// <summary>The % that applied when this invoice was issued (e.g. 2.00) - never recalculated even if Admin changes the rate later.</summary>
    public decimal PlatformCommissionRateApplied { get; private set; }
    /// <summary>Subtotal x PlatformCommissionRateApplied / 100, frozen. This is the company's own commission - what covers running costs, distinct from the agent's bonus below.</summary>
    public decimal PlatformCommissionAmount { get; private set; }
    /// <summary>The hotel agent's (or whichever agent placed this order's) bonus for this specific sale, frozen. Paid to the agent from the company - never deducted from the farmer or the hotel directly.</summary>
    public decimal HotelAgentBonusAmount { get; private set; }

    /// <summary>
    /// GrandTotal = Subtotal + TaxAmount + PlatformCommissionAmount +
    /// HotelAgentBonusAmount - Discount. This is what the hotel must pay to
    /// cover the sale itself plus the company's commission and the agent's
    /// bonus for placing the order - the company is never left short.
    /// </summary>
    public decimal GrandTotal { get; private set; }
    public decimal AmountPaid { get; private set; }

    private readonly List<InvoiceItem> _items = new();
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() { } // EF Core

    public Invoice(
        string invoiceNumber,
        Guid orderId,
        Guid customerId,
        Guid? taxConfigurationId,
        bool taxWasEnabled,
        Enums.TaxCalculationMode? taxMode)
    {
        InvoiceNumber = invoiceNumber;
        OrderId = orderId;
        CustomerId = customerId;
        InvoiceDate = DateTime.UtcNow;
        Status = InvoiceStatus.Draft;
        TaxConfigurationId = taxConfigurationId;
        TaxWasEnabled = taxWasEnabled;
        TaxMode = taxMode;
    }

    public void AddItem(InvoiceItem item) => _items.Add(item);

    /// <summary>Rolls up all line items into the invoice-level totals. Called once all lines are added, before ApplyCompanyCharges.</summary>
    public void Recalculate(decimal discount = 0)
    {
        Subtotal = _items.Sum(i => i.LineSubtotal);
        TaxableAmount = _items.Sum(i => i.TaxableAmount);
        TaxAmount = _items.Sum(i => i.TaxAmount);
        Discount = discount;
        // Provisional - ApplyCompanyCharges (always called next, by
        // GenerateInvoiceCommandHandler, even when both charges are zero)
        // layers the platform commission and agent bonus on top of this.
        GrandTotal = Math.Round(_items.Sum(i => i.LineTotal) - discount, 2);
    }

    /// <summary>
    /// Freezes the company's own commission and the placing agent's bonus for
    /// this sale, and finalizes GrandTotal to
    /// Subtotal + TaxAmount + PlatformCommissionAmount + HotelAgentBonusAmount - Discount.
    /// Called once, right after Recalculate, before Issue() - so GrandTotal
    /// is only ever set once matching this exact formula, never silently
    /// recomputed later from a "current" rate.
    /// </summary>
    public void ApplyCompanyCharges(Guid? platformCommissionConfigurationId, decimal platformCommissionRateApplied, decimal platformCommissionAmount, decimal hotelAgentBonusAmount)
    {
        PlatformCommissionConfigurationId = platformCommissionConfigurationId;
        PlatformCommissionRateApplied = platformCommissionRateApplied;
        PlatformCommissionAmount = platformCommissionAmount;
        HotelAgentBonusAmount = hotelAgentBonusAmount;
        GrandTotal = Math.Round(Subtotal + TaxAmount + PlatformCommissionAmount + HotelAgentBonusAmount - Discount, 2);
    }

    public void Issue()
    {
        if (!_items.Any())
            throw new DomainException("Cannot issue an invoice with no items.");
        Status = InvoiceStatus.Issued;
    }

    public void RegisterPayment(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");

        AmountPaid = Math.Round(AmountPaid + amount, 2);

        Status = AmountPaid >= GrandTotal
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;
    }

    public void Cancel() => Status = InvoiceStatus.Cancelled;

    public decimal BalanceDue => Math.Max(0, GrandTotal - AmountPaid);
}
