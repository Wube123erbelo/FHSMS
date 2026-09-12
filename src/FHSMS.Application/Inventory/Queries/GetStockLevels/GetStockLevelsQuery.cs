using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Inventory.Queries.GetStockLevels;

/// <summary>Current stock on hand per product, derived by summing InventoryTransactions.</summary>
public record GetStockLevelsQuery : IRequest<List<StockLevelDto>>;
