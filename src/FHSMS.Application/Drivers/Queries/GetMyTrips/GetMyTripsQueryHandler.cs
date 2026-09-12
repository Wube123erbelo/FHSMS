using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Drivers.Queries.GetAvailableTrips;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Drivers.Queries.GetMyTrips;

public class GetMyTripsQueryHandler : IRequestHandler<GetMyTripsQuery, List<TripDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyTripsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TripDto>> Handle(GetMyTripsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId) return new List<TripDto>();

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
        if (driver is null) return new List<TripDto>();

        var deliveries = await _context.Deliveries
            .Where(d => d.DriverId == driver.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return await GetAvailableTripsQueryHandler.MapToTripsAsync(deliveries, _context, cancellationToken);
    }
}
