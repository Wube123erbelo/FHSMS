namespace FHSMS.Domain.Enums;

/// <summary>
/// A DriverPayment's lifecycle: generated automatically the moment a
/// delivery is marked Delivered (PendingApproval), then Approved when an
/// admin confirms it - the same two-step shape as FarmerInvoiceStatus,
/// applied to the driver side of the business instead of the farmer side.
/// </summary>
public enum DriverPaymentStatus
{
    PendingApproval = 1,
    Approved = 2
}
