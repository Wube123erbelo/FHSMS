namespace FHSMS.Application.Common.Models;

public class PlatformCommissionConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public decimal? CurrentRate { get; set; }
    public DateTime? CurrentRateEffectiveFrom { get; set; }
    public decimal? PendingRate { get; set; }
    public DateTime? PendingRateEffectiveFrom { get; set; }
}

public class PlatformCommissionRateHistoryDto
{
    public Guid Id { get; set; }
    public decimal Rate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
