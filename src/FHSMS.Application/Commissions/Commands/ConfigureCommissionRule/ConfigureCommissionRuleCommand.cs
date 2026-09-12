using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Commissions.Commands.ConfigureCommissionRule;

/// <summary>
/// Sets (or updates) the commission rule for an agent type. Can be called at
/// any time by an admin - e.g. POST /api/commissions/rules - and takes effect
/// immediately for every commission accrued after the change; commissions
/// already accrued keep the rate that was in effect when they were created
/// (see Commission.ForInvoice / Commission.ForStockReceipt).
/// </summary>
public record ConfigureCommissionRuleCommand(
    AgentType AgentType,
    CommissionBasis Basis,
    decimal? Percentage,
    decimal? FlatRateAmount) : IRequest<Guid>;
