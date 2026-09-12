using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Farmers.Queries.GetFarmers;

public class GetFarmersQueryHandler : IRequestHandler<GetFarmersQuery, List<FarmerDto>>
{
    private readonly IApplicationDbContext _context;
    public GetFarmersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<FarmerDto>> Handle(GetFarmersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Farmers
            .OrderBy(f => f.Code)
            .Select(f => new FarmerDto
            {
                Id = f.Id,
                Code = f.Code,
                Name = f.Name,
                ContactPerson = f.ContactPerson,
                Phone = f.Phone,
                Location = f.Location,
                BankAccountNumber = f.BankAccountNumber,
                IsActive = f.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
