using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Inventory.Queries.GetPendingStockConfirmations;

/// <summary>Admin's review queue - every farmer-agent-logged stock receipt not yet confirmed as actually arrived.</summary>
public record GetPendingStockConfirmationsQuery : IRequest<List<InventoryTransactionDto>>;
