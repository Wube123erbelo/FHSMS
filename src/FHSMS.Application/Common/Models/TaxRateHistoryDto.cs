namespace FHSMS.Application.Common.Models;

public class TaxRateHistoryDto
{
    public Guid Id { get; set; }
    public decimal Rate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
