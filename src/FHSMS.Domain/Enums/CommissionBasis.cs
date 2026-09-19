namespace FHSMS.Domain.Enums;

/// <summary>
/// How a commission is calculated for a given agent type. Configured per
/// CommissionRule and adjustable by an admin at any time via
/// ConfigureCommissionRuleCommand - never hard-coded, same principle as tax.
/// </summary>
public enum CommissionBasis
{
    /// <summary>Percentage of the resulting invoice's grand total (e.g. hotel agents: 2% of the sale).</summary>
    PercentageOfInvoice = 1,

    /// <summary>Flat birr amount per unit of quantity handled (e.g. farmer agents: 0.50 birr per kg received).</summary>
    FlatRatePerQuantity = 2
}
