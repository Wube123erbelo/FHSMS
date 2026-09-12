using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.PlatformCommission.Queries.GetPlatformCommissionConfiguration;

public class GetPlatformCommissionConfigurationQueryHandler
    : IRequestHandler<GetPlatformCommissionConfigurationQuery, PlatformCommissionConfigurationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPlatformCommissionConfigurationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformCommissionConfigurationDto?> Handle(
        GetPlatformCommissionConfigurationQuery request, CancellationToken cancellationToken)
    {
        var configuration = await _context.PlatformCommissionConfigurations
            .Include(c => c.Rates)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (configuration is null)
            return null;

        var currentRate = configuration.GetRateAsOf();
        var pendingRate = configuration.GetPendingRate();
        return new PlatformCommissionConfigurationDto
        {
            Id = configuration.Id,
            Name = configuration.Name,
            IsEnabled = configuration.IsEnabled,
            CurrentRate = currentRate?.Rate,
            CurrentRateEffectiveFrom = currentRate?.EffectiveFrom,
            PendingRate = pendingRate?.Rate,
            PendingRateEffectiveFrom = pendingRate?.EffectiveFrom
        };
    }
}
