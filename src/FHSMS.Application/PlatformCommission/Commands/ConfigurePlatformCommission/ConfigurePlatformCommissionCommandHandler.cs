using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.PlatformCommission.Commands.ConfigurePlatformCommission;

public class ConfigurePlatformCommissionCommandHandler : IRequestHandler<ConfigurePlatformCommissionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ConfigurePlatformCommissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ConfigurePlatformCommissionCommand request, CancellationToken cancellationToken)
    {
        if (request.ExistingConfigurationId is { } id)
        {
            var existing = await _context.PlatformCommissionConfigurations
                .Include(c => c.Rates)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.PlatformCommissionConfiguration), id);

            existing.SetEnabled(request.IsEnabled);
            existing.ScheduleRate(request.Rate, request.EffectiveFrom);

            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var configuration = new Domain.Entities.PlatformCommissionConfiguration(
            request.Name, request.IsEnabled, request.Rate, request.EffectiveFrom);

        _context.PlatformCommissionConfigurations.Add(configuration);
        await _context.SaveChangesAsync(cancellationToken);
        return configuration.Id;
    }
}
