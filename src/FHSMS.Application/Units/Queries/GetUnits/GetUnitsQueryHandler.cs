using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Units.Queries.GetUnits;

public class GetUnitsQueryHandler : IRequestHandler<GetUnitsQuery, List<UnitDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUnitsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<UnitDto>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Units
            .OrderBy(u => u.Code)
            .Select(u => new UnitDto { Id = u.Id, Code = u.Code, Name = u.Name, Abbreviation = u.Abbreviation, IsActive = u.IsActive })
            .ToListAsync(cancellationToken);
    }
}
