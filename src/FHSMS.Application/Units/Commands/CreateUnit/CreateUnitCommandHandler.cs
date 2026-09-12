using FHSMS.Application.Common.Interfaces;
using MediatR;

namespace FHSMS.Application.Units.Commands.CreateUnit;

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public CreateUnitCommandHandler(IApplicationDbContext context, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        // Fully-qualified: IRequestHandler<T, Guid> pulls MediatR.Unit into
        // scope for its non-generic sibling handlers, and this file's own
        // "Unit" (the entity) would otherwise collide with it - see also
        // UpdateUnitCommandHandler/DeleteUnitCommandHandler, which hit the
        // same thing via nameof(Unit).
        var unit = new FHSMS.Domain.Entities.Unit
        {
            Code = await _numberGenerator.NextUnitCodeAsync(cancellationToken),
            Name = request.Name,
            Abbreviation = request.Abbreviation
        };
        _context.Units.Add(unit);
        await _context.SaveChangesAsync(cancellationToken);
        return unit.Id;
    }
}
