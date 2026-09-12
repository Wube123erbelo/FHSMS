namespace FHSMS.Domain.Enums;

/// <summary>
/// A FarmerInvoice's lifecycle: generated automatically the moment stock is
/// logged (PendingApproval, unless the logger is a SuperAdmin - see
/// FarmerInvoice.Create), then Approved when an admin confirms the stock
/// receipt (ConfirmStockReceiptCommand) - at which point the amount is
/// treated as owed/paid to the farmer.
/// </summary>
public enum FarmerInvoiceStatus
{
    PendingApproval = 1,
    Approved = 2
}
