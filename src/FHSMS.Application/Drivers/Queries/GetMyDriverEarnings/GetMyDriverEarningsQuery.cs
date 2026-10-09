using MediatR;

namespace FHSMS.Application.Drivers.Queries.GetMyDriverEarnings;

/// <summary>Sum of trip prices for the calling driver's completed trips - the "income for picking products" figure on their dashboard.</summary>
public record GetMyDriverEarningsQuery : IRequest<DriverEarningsDto>;

public class DriverEarningsDto
{
    public decimal TotalEarned { get; set; }
    public decimal ThisMonthEarned { get; set; }
    /// <summary>Trip prices for trips already accepted (and/or in transit) but not yet delivered - what the driver stands to earn once they complete them, not money already paid out.</summary>
    public decimal PendingTripValue { get; set; }
    public int CompletedTripCount { get; set; }
    public int PendingTripCount { get; set; }
}
