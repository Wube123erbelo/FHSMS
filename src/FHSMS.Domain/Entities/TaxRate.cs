using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single dated rate belonging to a TaxConfiguration. Rates are immutable once
/// created (see TaxConfiguration.ScheduleRate) - the *value* of an old TaxRate row
/// never changes, which is exactly what lets historical invoices stay correct
/// even after the Admin updates the current rate.
/// </summary>
public class TaxRate : BaseEntity
{
    public Guid TaxConfigurationId { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    private TaxRate() { } // EF Core

    public TaxRate(Guid taxConfigurationId, decimal rate, DateTime effectiveFrom)
    {
        TaxConfigurationId = taxConfigurationId;
        Rate = rate;
        EffectiveFrom = effectiveFrom;
    }

    internal void Close(DateTime effectiveTo) => EffectiveTo = effectiveTo;
}
