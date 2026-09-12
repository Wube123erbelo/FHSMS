using MediatR;

namespace FHSMS.Application.Commissions.Commands.SetCommissionUnitRate;

/// <summary>
/// Sets (or replaces) the flat-rate-per-quantity override for one unit of
/// measure on a commission rule - e.g. "0.50 birr/kg" and "45 birr/quintal"
/// can both live on the same FarmerAgent rule. Callable by an admin at any
/// time; takes effect on the next stock receipt logged in that unit.
/// </summary>
public record SetCommissionUnitRateCommand(Guid CommissionRuleId, Guid UnitId, decimal RateAmount) : IRequest;
