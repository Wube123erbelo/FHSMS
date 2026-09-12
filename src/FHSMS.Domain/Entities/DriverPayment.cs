using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// The driver-side counterpart to FarmerInvoice: what the company owes a
/// driver for one completed trip. Auto-generated the instant a delivery is
/// marked Delivered (see MarkDeliveredCommandHandler) - Amount is
/// Delivery.TripPrice frozen at that exact moment, so a later change to how
/// trips are priced never rewrites an already-generated payment record.
///
/// Exactly one DriverPayment exists per Delivery - enforced both by a unique
/// DB index (DriverPaymentConfiguration) and structurally, since both are
/// created together inside a single handler call.
///
/// Unlike FarmerInvoice (whose "was the farmer paid" flag lives on
/// InventoryTransaction, a separate entity), there is no separate
/// transaction-like record on the driver side to hold that fact - so
/// DriverWasPaid lives directly here, set at the same time an admin
/// approves the payment (see Approve).
/// </summary>
public class DriverPayment : AuditableEntity
{
    public Guid DeliveryId { get; private set; }
    public Guid? DriverId { get; private set; }
    public string? DriverName { get; private set; }
    /// <summary>Delivery.TripPrice as of the moment this trip was marked delivered - frozen, never recalculated even if trip pricing conventions change later.</summary>
    public decimal Amount { get; private set; }

    public DriverPaymentStatus Status { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    /// <summary>Whether the company has actually disbursed this amount to the driver - a separate fact from Status, recorded at the same time an admin approves (see Approve), same principle as InventoryTransaction.FarmerPaymentConfirmed.</summary>
    public bool DriverWasPaid { get; private set; }

    private DriverPayment() { } // EF Core

    public static DriverPayment Create(Guid deliveryId, Guid? driverId, string? driverName, decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Trip payment amount cannot be negative.");

        return new DriverPayment
        {
            DeliveryId = deliveryId,
            DriverId = driverId,
            DriverName = driverName,
            Amount = amount,
            Status = DriverPaymentStatus.PendingApproval
        };
    }

    /// <summary>Admin confirms this trip payment - records both that it's approved and, in the same action, whether the driver has actually been paid yet.</summary>
    public void Approve(Guid approvedByUserId, bool driverWasPaid)
    {
        if (Status == DriverPaymentStatus.Approved)
            throw new DomainException("This driver payment has already been approved.");

        Status = DriverPaymentStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
        DriverWasPaid = driverWasPaid;
    }
}
