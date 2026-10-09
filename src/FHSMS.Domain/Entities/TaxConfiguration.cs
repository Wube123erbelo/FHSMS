using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A configurable tax definition managed entirely from Settings -> Tax & VAT.
/// The system never hard-codes a rate: TaxConfiguration only describes *how* a
/// tax behaves (enabled or not, inclusive or exclusive, which type). The actual
/// numeric rate - and its history over time - lives in TaxRate.
/// </summary>
public class TaxConfiguration : AuditableEntity
{
    public string Name { get; private set; } = default!;
    public TaxType TaxType { get; private set; }
    public bool IsEnabled { get; private set; }
    public TaxCalculationMode CalculationMode { get; private set; }
    public bool ExemptionAllowed { get; private set; }
    public TaxConfigurationStatus Status { get; private set; }

    private readonly List<TaxRate> _rates = new();
    public IReadOnlyCollection<TaxRate> Rates => _rates.AsReadOnly();

    private TaxConfiguration() { } // EF Core

    public TaxConfiguration(
        string name,
        TaxType taxType,
        bool isEnabled,
        TaxCalculationMode calculationMode,
        bool exemptionAllowed,
        decimal initialRate,
        DateTime effectiveFrom)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tax configuration name is required.");
        if (initialRate < 0 || initialRate > 100)
            throw new DomainException("Tax rate must be between 0 and 100.");

        Name = name;
        TaxType = taxType;
        IsEnabled = isEnabled;
        CalculationMode = calculationMode;
        ExemptionAllowed = exemptionAllowed;
        Status = TaxConfigurationStatus.Active;

        _rates.Add(new TaxRate(Id, initialRate, effectiveFrom));
    }

    /// <summary>
    /// Turns tax on/off from Settings. Existing invoices are never touched -
    /// they already carry their own snapshot of the rate that applied at the time.
    /// </summary>
    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;

    public void UpdateSettings(TaxCalculationMode calculationMode, bool exemptionAllowed)
    {
        CalculationMode = calculationMode;
        ExemptionAllowed = exemptionAllowed;
    }

    public void Deactivate() => Status = TaxConfigurationStatus.Inactive;

    /// <summary>
    /// Schedules a new rate effective from a future (or immediate) date, and closes
    /// off the previously-open-ended rate so the two never overlap. This is how
    /// "the VAT rate can change by effective date" is satisfied without ever
    /// mutating a rate that historical invoices may already reference.
    /// </summary>
    public TaxRate ScheduleRate(decimal newRate, DateTime effectiveFrom)
    {
        if (newRate < 0 || newRate > 100)
            throw new DomainException("Tax rate must be between 0 and 100.");

        var currentlyOpen = _rates
            .Where(r => r.EffectiveTo == null)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefault();

        if (currentlyOpen != null)
        {
            if (effectiveFrom <= currentlyOpen.EffectiveFrom)
            {
                // currentlyOpen is whichever rate was scheduled MOST RECENTLY,
                // which may itself be effective in the future (an admin can
                // schedule several changes ahead of time). Naming it explicitly,
                // and saying whether it's already active or still pending,
                // avoids the confusing "must take effect after the currently
                // active rate started" message when the real conflict is with
                // a not-yet-active scheduled change the admin can't otherwise see.
                var isPending = currentlyOpen.EffectiveFrom > DateTime.UtcNow;
                throw new DomainException(isPending
                    ? $"A {currentlyOpen.Rate}% rate is already scheduled to take effect on {currentlyOpen.EffectiveFrom:yyyy-MM-dd}. Choose a later date, or cancel that scheduled change first."
                    : $"New rate must take effect after the currently active rate started on {currentlyOpen.EffectiveFrom:yyyy-MM-dd}.");
            }
            currentlyOpen.Close(effectiveFrom);
        }

        var newRateRecord = new TaxRate(Id, newRate, effectiveFrom);
        _rates.Add(newRateRecord);
        return newRateRecord;
    }

    /// <summary>The rate already scheduled to take over in the future, if any (distinct from the one active right now).</summary>
    public TaxRate? GetPendingRate()
    {
        var date = DateTime.UtcNow;
        return _rates
            .Where(r => r.EffectiveTo == null && r.EffectiveFrom > date)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefault();
    }

    /// <summary>Returns the rate that was/ is in force on a given date (defaults to now).</summary>
    public TaxRate? GetRateAsOf(DateTime? asOf = null)
    {
        var date = asOf ?? DateTime.UtcNow;
        return _rates
            .Where(r => r.EffectiveFrom <= date && (r.EffectiveTo == null || r.EffectiveTo > date))
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefault();
    }
}
