using MediatR;

namespace FHSMS.Application.Drivers.Queries.GetAllDrivers;

/// <summary>Admin's "Drivers" follow-up list - every registered driver with their trip stats. Admin-only; this is what replaced admin having access to the driver self-registration screen.</summary>
public record GetAllDriversQuery : IRequest<List<AdminDriverDto>>;

public class AdminDriverDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? PlateNumber { get; set; }
    public string TruckType { get; set; } = default!;
    public bool IsActive { get; set; }
    public int PendingTripCount { get; set; }
    public int CompletedTripCount { get; set; }
    public decimal TotalEarned { get; set; }
}
