using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single stock movement: receiving from a farmer, issuing against an order,
/// a manual adjustment, or recording damage/wastage. StockOnHand is always the
/// sum of signed quantities across a product's transactions - there is no
/// separately-maintained "current stock" counter to fall out of sync.
/// </summary>
public class InventoryTransaction : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public InventoryTransactionType Type { get; private set; }

    /// <summary>Positive for Receiving; negative for Issuing/Damage/Wastage; either sign for Adjustment.</summary>
    public decimal QuantityChange { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public Guid? OrderId { get; private set; }

    /// <summary>Which farmer this stock was received from. Only set for Receiving transactions.</summary>
    public Guid? FarmerId { get; private set; }

    /// <summary>Which farmer agent logged this receipt (web, mobile, or Telegram). Drives commission accrual.</summary>
    public Guid? AgentUserId { get; private set; }

    // --- Admin confirmation (Receiving only) ---
    /// <summary>
    /// A farmer agent logging stock received is a claim, not yet a fact on
    /// the shelf - IsConfirmed starts false for any Receiving transaction an
    /// agent logs, and this quantity is excluded from stock-on-hand until an
    /// admin confirms it actually arrived (see ConfirmReceipt). A Receiving
    /// transaction logged directly by an admin is auto-confirmed - there's no
    /// one else who needs to double check the admin's own count.
    /// </summary>
    public bool IsConfirmed { get; private set; } = true;
    public Guid? ConfirmedByUserId { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>Recorded at confirmation time: did the farmer actually get paid for this batch, and how much - the "check farmer got its money" obligation, captured in the same step as confirming the stock itself.</summary>
    public bool FarmerPaymentConfirmed { get; private set; }
    public decimal? AmountPaidToFarmer { get; private set; }

    private InventoryTransaction() { } // EF Core

    private InventoryTransaction(
        Guid productId, InventoryTransactionType type, decimal quantityChange,
        string? reference, string? notes, Guid? orderId, Guid? farmerId = null, Guid? agentUserId = null, bool isConfirmed = true)
    {
        ProductId = productId;
        Type = type;
        QuantityChange = quantityChange;
        Reference = reference;
        Notes = notes;
        OrderId = orderId;
        FarmerId = farmerId;
        AgentUserId = agentUserId;
        IsConfirmed = isConfirmed;
    }

    public static InventoryTransaction Receive(
        Guid productId, decimal quantity, string? reference, string? notes,
        Guid? farmerId = null, Guid? agentUserId = null, bool isConfirmed = true)
    {
        if (quantity <= 0) throw new DomainException("Received quantity must be greater than zero.");
        return new InventoryTransaction(
            productId, InventoryTransactionType.Receiving, quantity, reference, notes, null, farmerId, agentUserId, isConfirmed);
    }

    /// <summary>An admin verifies a farmer agent's stock-in actually arrived - only after this does the quantity count toward stock-on-hand. Also records whether/how much the farmer was paid.</summary>
    public void ConfirmReceipt(Guid confirmedByUserId, bool farmerWasPaid, decimal? amountPaidToFarmer)
    {
        if (Type != InventoryTransactionType.Receiving)
            throw new DomainException("Only a Receiving transaction can be confirmed.");
        if (IsConfirmed)
            throw new DomainException("This stock receipt has already been confirmed.");

        IsConfirmed = true;
        ConfirmedByUserId = confirmedByUserId;
        ConfirmedAt = DateTime.UtcNow;
        FarmerPaymentConfirmed = farmerWasPaid;
        AmountPaidToFarmer = amountPaidToFarmer;
    }

    public static InventoryTransaction Issue(Guid productId, decimal quantity, Guid? orderId, string? notes)
    {
        if (quantity <= 0) throw new DomainException("Issued quantity must be greater than zero.");
        return new InventoryTransaction(productId, InventoryTransactionType.Issuing, -quantity, null, notes, orderId);
    }

    public static InventoryTransaction Adjust(Guid productId, decimal signedQuantity, string? notes)
    {
        if (signedQuantity == 0) throw new DomainException("Adjustment quantity cannot be zero.");
        return new InventoryTransaction(productId, InventoryTransactionType.Adjustment, signedQuantity, null, notes, null);
    }

    public static InventoryTransaction Damage(Guid productId, decimal quantity, string? notes)
    {
        if (quantity <= 0) throw new DomainException("Damaged quantity must be greater than zero.");
        return new InventoryTransaction(productId, InventoryTransactionType.Damage, -quantity, null, notes, null);
    }

    public static InventoryTransaction Wastage(Guid productId, decimal quantity, string? notes)
    {
        if (quantity <= 0) throw new DomainException("Wastage quantity must be greater than zero.");
        return new InventoryTransaction(productId, InventoryTransactionType.Wastage, -quantity, null, notes, null);
    }
}
