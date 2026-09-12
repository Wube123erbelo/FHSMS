using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single accrued commission for an agent. Created via one of the two
/// factory methods below depending on what generated it - an invoice
/// (hotel agents, percentage basis) or a logged stock receipt (farmer
/// agents, flat-rate-per-quantity basis). Both paths freeze the rate that
/// applied at the moment of creation, same principle as the tax engine: a
/// later change to CommissionRule never retroactively touches commissions
/// that already accrued.
/// </summary>
public class Commission : AuditableEntity
{
    public Guid AgentUserId { get; private set; }
    public CommissionSourceType SourceType { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public Guid? InventoryTransactionId { get; private set; }

    public CommissionBasis Basis { get; private set; }
    public decimal BaseAmount { get; private set; } // invoice subtotal (Invoice) or quantity (StockReceipt)
    public decimal? Percentage { get; private set; }
    public decimal? FlatRateAmount { get; private set; }
    public decimal CommissionAmount { get; private set; }
    public CommissionStatus Status { get; private set; }

    private Commission() { } // EF Core

    private Commission(
        Guid agentUserId, CommissionSourceType sourceType, Guid? invoiceId, Guid? inventoryTransactionId,
        CommissionBasis basis, decimal baseAmount, decimal? percentage, decimal? flatRateAmount, decimal commissionAmount)
    {
        AgentUserId = agentUserId;
        SourceType = sourceType;
        InvoiceId = invoiceId;
        InventoryTransactionId = inventoryTransactionId;
        Basis = basis;
        BaseAmount = baseAmount;
        Percentage = percentage;
        FlatRateAmount = flatRateAmount;
        CommissionAmount = commissionAmount;
        Status = CommissionStatus.Accrued;
    }

    /// <summary>
    /// Hotel-agent-style commission: a percentage of the invoice's Subtotal
    /// (selling-price total before tax/platform-commission/bonus are layered
    /// on - never GrandTotal, since GrandTotal itself includes this bonus).
    /// </summary>
    public static Commission ForInvoice(Guid agentUserId, Guid invoiceId, decimal invoiceSubtotal, decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new DomainException("Percentage must be between 0 and 100.");
        var amount = Math.Round(invoiceSubtotal * (percentage / 100m), 2);
        return new Commission(
            agentUserId, CommissionSourceType.Invoice, invoiceId, null,
            CommissionBasis.PercentageOfInvoice, invoiceSubtotal, percentage, null, amount);
    }

    /// <summary>Farmer-agent-style commission: a flat birr amount per unit of quantity received.</summary>
    public static Commission ForStockReceipt(Guid agentUserId, Guid inventoryTransactionId, decimal quantity, decimal flatRateAmount)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (flatRateAmount < 0)
            throw new DomainException("Flat rate amount must be zero or greater.");
        var amount = Math.Round(quantity * flatRateAmount, 2);
        return new Commission(
            agentUserId, CommissionSourceType.StockReceipt, null, inventoryTransactionId,
            CommissionBasis.FlatRatePerQuantity, quantity, null, flatRateAmount, amount);
    }

    /// <summary>
    /// Hotel-agent-style commission when the HotelAgent rule is configured
    /// per-unit rather than per-invoice-percentage (see CommissionRule
    /// remarks: Basis is independent of AgentType, so either agent type can
    /// use either basis). An order can mix products across different units
    /// (kg, crate, litre...), each with its own rate, so the caller sums
    /// those per-line amounts itself (AccrueCommissionOnInvoiceIssued) and
    /// passes the total quantity and blended effective rate through here
    /// purely for display - commissionAmount is the number of record.
    /// </summary>
    public static Commission ForInvoiceFlatRate(Guid agentUserId, Guid invoiceId, decimal totalQuantity, decimal commissionAmount, decimal effectiveRate)
    {
        if (totalQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (commissionAmount < 0)
            throw new DomainException("Commission amount must be zero or greater.");
        return new Commission(
            agentUserId, CommissionSourceType.Invoice, invoiceId, null,
            CommissionBasis.FlatRatePerQuantity, totalQuantity, null, effectiveRate, commissionAmount);
    }

    public void Approve() => Status = CommissionStatus.Approved;
    public void MarkPaid() => Status = CommissionStatus.Paid;
    public void Cancel() => Status = CommissionStatus.Cancelled;
}
