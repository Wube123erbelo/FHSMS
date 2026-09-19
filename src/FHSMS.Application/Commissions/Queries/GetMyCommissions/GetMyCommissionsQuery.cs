using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Commissions.Queries.GetMyCommissions;

/// <summary>
/// The calling agent's own commissions - no agentUserId parameter, because
/// the caller's identity comes from their own token (ICurrentUserService),
/// same principle as CreateOrderCommandHandler/RecordInventoryTransactionCommandHandler:
/// an agent never has to know or pass their own ID for the app to know who
/// they are. Backs the dashboard's "today's earnings" quick-stats card.
/// </summary>
public record GetMyCommissionsQuery : IRequest<List<CommissionDto>>;
