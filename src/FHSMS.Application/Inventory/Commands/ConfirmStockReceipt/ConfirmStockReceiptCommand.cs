using MediatR;

namespace FHSMS.Application.Inventory.Commands.ConfirmStockReceipt;

/// <summary>
/// Admin verifies a farmer agent's logged stock receipt actually arrived -
/// only after this does the quantity count toward stock-on-hand, and only
/// after this does the farmer agent's commission accrue. This is also where
/// the auto-generated FarmerInvoice for this receipt gets approved (see
/// FarmerInvoice.Approve) - the amount owed to the farmer is never typed by
/// hand, it comes from Quantity x the buying price frozen when the stock was
/// logged. FarmerWasPaid records the separate fact of whether the company has
/// actually disbursed that amount yet.
/// </summary>
public record ConfirmStockReceiptCommand(Guid InventoryTransactionId, bool FarmerWasPaid) : IRequest;
