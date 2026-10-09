using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A per-unit-of-measure override of a FlatRatePerQuantity CommissionRule's
/// default rate - e.g. the same rule can pay 0.50 birr/kg but 45 birr/quintal.
/// Admin-managed via CommissionRule.SetUnitRate/RemoveUnitRate, changeable at
/// any time.
/// </summary>
public class CommissionUnitRate : BaseEntity
{
    public Guid CommissionRuleId { get; private set; }
    public Guid UnitId { get; private set; }
    public Unit? Unit { get; private set; }
    public decimal RateAmount { get; private set; }

    private CommissionUnitRate() { } // EF Core

    public CommissionUnitRate(Guid commissionRuleId, Guid unitId, decimal rateAmount)
    {
        CommissionRuleId = commissionRuleId;
        UnitId = unitId;
        RateAmount = rateAmount;
    }

    public void UpdateRate(decimal rateAmount) => RateAmount = rateAmount;
}
