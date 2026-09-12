using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class TaxConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public TaxType TaxType { get; set; }
    public bool IsEnabled { get; set; }
    public TaxCalculationMode CalculationMode { get; set; }
    public bool ExemptionAllowed { get; set; }
    public decimal? CurrentRate { get; set; }
    public DateTime? CurrentRateEffectiveFrom { get; set; }
    public decimal? PendingRate { get; set; }
    public DateTime? PendingRateEffectiveFrom { get; set; }
}
