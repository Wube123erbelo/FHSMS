using FHSMS.Domain.Common;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// The company's own commission on hotel sales - what covers the company's
/// running costs and margin (separate from agent bonuses, which come out of
/// the same sale but are a different line). Configurable from Settings ->
/// Total Commission, exactly like TaxConfiguration: the rate is never
/// hard-coded, it is scheduled and effective-dated via PlatformCommissionRate,
/// and every invoice freezes the rate that applied when it was issued so a
/// later admin change never retroactively edits an already-issued invoice.
///
/// The system expects exactly one active row here (seeded at 2%) - Admin
/// changes the *rate over time* via ScheduleRate, and can flip IsEnabled off
/// only if the company genuinely wants to run promotionally commission-free
/// for a period, same opt-in/opt-out semantics as tax.
/// </summary>
public class PlatformCommissionConfiguration : AuditableEntity
{
    public string Name { get; private set; } = default!;
    public bool IsEnabled { get; private set; }

    private readonly List<PlatformCommissionRate> _rates = new();
    public IReadOnlyCollection<PlatformCommissionRate> Rates => _rates.AsReadOnly();

    private PlatformCommissionConfiguration() { } // EF Core

    public PlatformCommissionConfiguration(string name, bool isEnabled, decimal initialRate, DateTime effectiveFrom)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Commission configuration name is required.");
        if (initialRate < 0 || initialRate > 100)
            throw new DomainException("Commission rate must be between 0 and 100.");

        Name = name;
        IsEnabled = isEnabled;
        _rates.Add(new PlatformCommissionRate(Id, initialRate, effectiveFrom));
    }

    /// <summary>Turns the platform commission on/off from Settings. Already-issued invoices are never touched - they carry their own frozen snapshot.</summary>
    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;

    /// <summary>
    /// Schedules a new rate effective from a future (or immediate) date,
    /// closing off the previously open-ended rate so the two never overlap -
    /// identical pattern to TaxConfiguration.ScheduleRate. This is how "the
    /// 2% must be adjustable by admin at any time, and must be reflected on
    /// every new invoice and every report" is satisfied without ever
    /// mutating a rate an already-issued invoice references.
    /// </summary>
    public PlatformCommissionRate ScheduleRate(decimal newRate, DateTime effectiveFrom)
    {
        if (newRate < 0 || newRate > 100)
            throw new DomainException("Commission rate must be between 0 and 100.");

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

        var newRateRecord = new PlatformCommissionRate(Id, newRate, effectiveFrom);
        _rates.Add(newRateRecord);
        return newRateRecord;
    }

    /// <summary>The rate already scheduled to take over in the future, if any (distinct from the one active right now).</summary>
    public PlatformCommissionRate? GetPendingRate()
    {
        var date = DateTime.UtcNow;
        return _rates
            .Where(r => r.EffectiveTo == null && r.EffectiveFrom > date)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefault();
    }

    /// <summary>Returns the rate that was/is in force on a given date (defaults to now).</summary>
    public PlatformCommissionRate? GetRateAsOf(DateTime? asOf = null)
    {
        var date = asOf ?? DateTime.UtcNow;
        return _rates
            .Where(r => r.EffectiveFrom <= date && (r.EffectiveTo == null || r.EffectiveTo > date))
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefault();
    }
}
