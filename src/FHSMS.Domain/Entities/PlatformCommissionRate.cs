using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

/// <summary>
/// A single dated rate belonging to a PlatformCommissionConfiguration.
/// Immutable once created (see PlatformCommissionConfiguration.ScheduleRate) -
/// exactly the pattern that lets an already-issued invoice keep the 2% (or
/// whatever it was) that applied when it was generated, even after the Admin
/// changes the current rate in Settings.
/// </summary>
public class PlatformCommissionRate : BaseEntity
{
    public Guid PlatformCommissionConfigurationId { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    private PlatformCommissionRate() { } // EF Core

    public PlatformCommissionRate(Guid platformCommissionConfigurationId, decimal rate, DateTime effectiveFrom)
    {
        PlatformCommissionConfigurationId = platformCommissionConfigurationId;
        Rate = rate;
        EffectiveFrom = effectiveFrom;
    }

    internal void Close(DateTime effectiveTo) => EffectiveTo = effectiveTo;
}
