using MediatR;

namespace FHSMS.Application.Commissions.Commands.RemoveCommissionUnitRate;

/// <summary>Removes a per-unit rate override, reverting that unit back to the rule's default FlatRateAmount.</summary>
public record RemoveCommissionUnitRateCommand(Guid CommissionRuleId, Guid UnitId) : IRequest;
