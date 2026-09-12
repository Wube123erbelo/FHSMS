using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// Configurable commission rule per agent type - same "never hard-code it"
/// principle as tax: rates live here, not scattered through calculation code,
/// and an admin can change them at any time via ConfigureCommissionRuleCommand
/// or SetUnitRate/RemoveUnitRate without a deployment.
///
/// Two calculation bases are supported, matching how FHSMS's two kinds of
/// agents actually get paid:
///   - PercentageOfInvoice: used for HotelAgent by default - a percentage of
///     the grand total of the invoice their order produced.
///   - FlatRatePerQuantity: used for FarmerAgent by default - a flat birr
///     amount per unit of stock they logged as received from a farmer.
///     Farmers supply produce in different units (kg, quintal, litre, crate,
///     ...), and a birr amount that's fair for a kg is not fair for a
///     quintal, so FlatRateAmount is only the *fallback* default. Per-Unit
///     overrides (UnitRates) let an admin set a distinct rate for each unit
///     of measure at any time - e.g. 0.50 birr/kg but 45 birr/quintal for the
///     same rule - and CommissionUnitRate is what AccrueCommissionOnStockReceived
///     actually looks up first, falling back to FlatRateAmount only when no
///     override exists for the product's unit.
///
/// Nothing here forces the PercentageOfInvoice/FlatRatePerQuantity pairing to
/// stay fixed to HotelAgent/FarmerAgent - the engine branches on whatever
/// Basis is configured, not on AgentType, so the business model can change
/// without a code change.
/// </summary>
public class CommissionRule : AuditableEntity
{
    public AgentType AgentType { get; private set; }
    public CommissionBasis Basis { get; private set; }

    /// <summary>Used when Basis == PercentageOfInvoice. 0-100.</summary>
    public decimal? Percentage { get; private set; }

    /// <summary>Used when Basis == FlatRatePerQuantity and no per-unit override applies. Birr per unit.</summary>
    public decimal? FlatRateAmount { get; private set; }

    public bool IsActive { get; private set; } = true;

    private readonly List<CommissionUnitRate> _unitRates = new();
    /// <summary>Per-unit-of-measure overrides of FlatRateAmount (e.g. a different birr amount per kg vs. per quintal).</summary>
    public IReadOnlyCollection<CommissionUnitRate> UnitRates => _unitRates.AsReadOnly();

    private CommissionRule() { } // EF Core

    public CommissionRule(AgentType agentType, CommissionBasis basis, decimal? percentage, decimal? flatRateAmount)
    {
        AgentType = agentType;
        Basis = basis;
        SetRateInternal(basis, percentage, flatRateAmount);
    }

    /// <summary>Convenience factory for a percentage-of-invoice rule.</summary>
    public static CommissionRule PercentageRule(AgentType agentType, decimal percentage)
        => new(agentType, CommissionBasis.PercentageOfInvoice, percentage, null);

    /// <summary>Convenience factory for a flat-rate-per-quantity rule (e.g. birr/kg default, with per-unit overrides addable via SetUnitRate).</summary>
    public static CommissionRule FlatRateRule(AgentType agentType, decimal flatRateAmount)
        => new(agentType, CommissionBasis.FlatRatePerQuantity, null, flatRateAmount);

    /// <summary>
    /// Changes the basis and/or default rate of an existing rule at any time.
    /// This is the single method admins use to adjust commissions - no
    /// redeploy, no code change, ever.
    /// </summary>
    public void Update(CommissionBasis basis, decimal? percentage, decimal? flatRateAmount)
    {
        Basis = basis;
        SetRateInternal(basis, percentage, flatRateAmount);
    }

    /// <summary>Sets (or replaces) the flat-rate override for a specific unit of measure - e.g. "45 birr per quintal". Adjustable at any time.</summary>
    public void SetUnitRate(Guid unitId, decimal rateAmount)
    {
        if (rateAmount < 0)
            throw new DomainException("Unit rate must be zero or greater.");

        var existing = _unitRates.FirstOrDefault(r => r.UnitId == unitId);
        if (existing != null)
            existing.UpdateRate(rateAmount);
        else
            _unitRates.Add(new CommissionUnitRate(Id, unitId, rateAmount));
    }

    /// <summary>Removes a per-unit override, reverting that unit to the rule's default FlatRateAmount.</summary>
    public void RemoveUnitRate(Guid unitId)
    {
        var existing = _unitRates.FirstOrDefault(r => r.UnitId == unitId);
        if (existing != null)
            _unitRates.Remove(existing);
    }

    /// <summary>The effective flat rate for a given unit: its override if one exists, otherwise the rule's default.</summary>
    public decimal? GetEffectiveFlatRate(Guid unitId)
        => _unitRates.FirstOrDefault(r => r.UnitId == unitId)?.RateAmount ?? FlatRateAmount;

    private void SetRateInternal(CommissionBasis basis, decimal? percentage, decimal? flatRateAmount)
    {
        if (basis == CommissionBasis.PercentageOfInvoice)
        {
            if (percentage is null || percentage < 0 || percentage > 100)
                throw new DomainException("Percentage must be between 0 and 100 for a percentage-of-invoice rule.");
            Percentage = percentage;
            FlatRateAmount = null;
        }
        else
        {
            if (flatRateAmount is null || flatRateAmount < 0)
                throw new DomainException("Flat rate amount must be zero or greater for a flat-rate-per-quantity rule.");
            FlatRateAmount = flatRateAmount;
            Percentage = null;
        }
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
