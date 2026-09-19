using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Drivers.Queries.GetMyDriverProfile;

public class GetMyDriverProfileQueryHandler : IRequestHandler<GetMyDriverProfileQuery, DriverDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyDriverProfileQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DriverDto?> Handle(GetMyDriverProfileQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId) return null;

        return await _context.Drivers
            .Where(d => d.UserId == userId)
            .Select(d => new DriverDto
            {
                Id = d.Id, Code = d.Code, UserId = d.UserId, FullName = d.FullName,
                Phone = d.Phone, PlateNumber = d.PlateNumber, TruckType = d.TruckType, IsActive = d.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
