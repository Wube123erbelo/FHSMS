using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Drivers.Queries.GetMyDriverEarnings;

public class GetMyDriverEarningsQueryHandler : IRequestHandler<GetMyDriverEarningsQuery, DriverEarningsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyDriverEarningsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DriverEarningsDto> Handle(GetMyDriverEarningsQuery request, CancellationToken cancellationToken)
    {
        var result = new DriverEarningsDto();
        if (_currentUser.UserId is not { } userId) return result;

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
        if (driver is null) return result;

        var trips = await _context.Deliveries
            .Where(d => d.DriverId == driver.Id)
            .ToListAsync(cancellationToken);

        var completed = trips.Where(d => d.Status == DeliveryStatus.Delivered).ToList();
        var pending = trips.Where(d => d.Status is DeliveryStatus.Pending or DeliveryStatus.InTransit).ToList();
        var now = DateTime.UtcNow;

        result.TotalEarned = completed.Sum(d => d.TripPrice ?? 0);
        result.ThisMonthEarned = completed
            .Where(d => d.DeliveredAt is { } deliveredAt && deliveredAt.Year == now.Year && deliveredAt.Month == now.Month)
            .Sum(d => d.TripPrice ?? 0);
        result.PendingTripValue = pending.Sum(d => d.TripPrice ?? 0);
        result.CompletedTripCount = completed.Count;
        result.PendingTripCount = pending.Count;

        return result;
    }
}
