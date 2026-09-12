using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Tax.Queries.GetActiveTaxConfiguration;

public class GetActiveTaxConfigurationsQueryHandler
    : IRequestHandler<GetActiveTaxConfigurationsQuery, List<TaxConfigurationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveTaxConfigurationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaxConfigurationDto>> Handle(
        GetActiveTaxConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var configurations = await _context.TaxConfigurations
            .Include(t => t.Rates)
            .Where(t => t.Status == TaxConfigurationStatus.Active)
            .ToListAsync(cancellationToken);

        return configurations.Select(c =>
        {
            var currentRate = c.GetRateAsOf();
            var pendingRate = c.GetPendingRate();
            return new TaxConfigurationDto
            {
                Id = c.Id,
                Name = c.Name,
                TaxType = c.TaxType,
                IsEnabled = c.IsEnabled,
                CalculationMode = c.CalculationMode,
                ExemptionAllowed = c.ExemptionAllowed,
                CurrentRate = currentRate?.Rate,
                CurrentRateEffectiveFrom = currentRate?.EffectiveFrom,
                PendingRate = pendingRate?.Rate,
                PendingRateEffectiveFrom = pendingRate?.EffectiveFrom
            };
        }).ToList();
    }
}
