using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Tax.Commands.UpdateVatToggle;

public class SetTaxEnabledCommandHandler : IRequestHandler<SetTaxEnabledCommand>
{
    private readonly IApplicationDbContext _context;

    public SetTaxEnabledCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SetTaxEnabledCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _context.TaxConfigurations
            .FirstOrDefaultAsync(t => t.Id == request.TaxConfigurationId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaxConfiguration), request.TaxConfigurationId);

        configuration.SetEnabled(request.IsEnabled);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
