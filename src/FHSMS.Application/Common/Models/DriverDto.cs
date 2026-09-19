using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class DriverDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public Guid UserId { get; set; }
    public string FullName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? PlateNumber { get; set; }
    public TruckType TruckType { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>One row on the Driver Portal's "Available Trips" board.</summary>
public class TripDto
{
    public Guid DeliveryId { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string? OriginLocation { get; set; }
    public string DestinationAddress { get; set; } = default!;
    public string DestinationName { get; set; } = default!;
    /// <summary>Short product/quantity summary, e.g. "Tomato 1,250 kg" or "Tomato + 2 more".</summary>
    public string ProductSummary { get; set; } = default!;
    public decimal TotalQuantity { get; set; }
    public decimal? TripPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = default!;
    public Guid? DriverId { get; set; }
    /// <summary>False for a trip an admin booked directly that the driver hasn't confirmed yet - true for everything else (self-accepted trips are confirmed immediately).</summary>
    public bool DriverConfirmed { get; set; }
}
