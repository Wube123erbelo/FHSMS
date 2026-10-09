using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Inventory.Queries.GetStockHistory;

public record GetStockHistoryQuery(Guid ProductId) : IRequest<List<InventoryTransactionDto>>;
