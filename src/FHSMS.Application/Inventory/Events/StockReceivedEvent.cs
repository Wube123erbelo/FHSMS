using MediatR;

namespace FHSMS.Application.Inventory.Events;

/// <summary>
/// Published after a Receiving-type InventoryTransaction is saved with an
/// AgentUserId (i.e. a farmer agent logged stock coming in from a farmer -
/// via the web PWA, mobile, or the Telegram bot, all going through the same
/// RecordInventoryTransactionCommand). Anything that should happen "because
/// stock was received" subscribes here - currently just commission accrual,
/// mirroring InvoiceIssuedEvent on the sales side.
/// </summary>
public record StockReceivedEvent(
    Guid InventoryTransactionId,
    Guid ProductId,
    decimal Quantity,
    Guid? FarmerId,
    Guid? AgentUserId) : INotification;
