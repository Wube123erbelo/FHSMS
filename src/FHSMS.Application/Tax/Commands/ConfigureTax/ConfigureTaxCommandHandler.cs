using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Tax.Commands.ConfigureTax;

public class ConfigureTaxCommandHandler : IRequestHandler<ConfigureTaxCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ConfigureTaxCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ConfigureTaxCommand request, CancellationToken cancellationToken)
    {
        if (request.ExistingTaxConfigurationId is { } id)
        {
            var existing = await _context.TaxConfigurations
                .Include(t => t.Rates)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
                ?? throw new NotFoundException(nameof(TaxConfiguration), id);

            existing.SetEnabled(request.IsEnabled);
            existing.UpdateSettings(request.CalculationMode, request.ExemptionAllowed);
            existing.ScheduleRate(request.Rate, request.EffectiveFrom);

            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var configuration = new TaxConfiguration(
            request.Name,
            request.TaxType,
            request.IsEnabled,
            request.CalculationMode,
            request.ExemptionAllowed,
            request.Rate,
            request.EffectiveFrom);

        _context.TaxConfigurations.Add(configuration);
        await _context.SaveChangesAsync(cancellationToken);
        return configuration.Id;
    }
}
