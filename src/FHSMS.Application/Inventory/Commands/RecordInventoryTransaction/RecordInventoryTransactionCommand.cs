using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Inventory.Commands.RecordInventoryTransaction;

/// <summary>
/// Single entry point for every kind of stock movement (Receiving, Issuing,
/// Adjustment, Damage, Wastage) - all share validation and stock-level
/// recalculation, so there's one place stock integrity rules live. The same
/// command backs the web PWA, mobile, and the Telegram bot's /stockin flow.
/// </summary>
public record RecordInventoryTransactionCommand(
    Guid ProductId,
    InventoryTransactionType Type,
    decimal Quantity, // always a positive magnitude; sign is derived from Type (except Adjustment, which may be signed)
    Guid? OrderId,
    string? Reference,
    string? Notes,
    Guid? FarmerId = null,   // Receiving only: which farmer this stock came from
    Guid? AgentUserId = null // Receiving only: which farmer agent logged it - drives commission accrual
) : IRequest<Guid>;
