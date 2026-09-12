using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string DestinationAddress { get; set; } = default!;
    public string? OriginLocation { get; set; }
    public decimal? TripPrice { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? VehicleInfo { get; set; }
    public bool DriverConfirmed { get; set; }
    public bool RecipientConfirmed { get; set; }
    public DateTime? RecipientConfirmedAt { get; set; }
    public DeliveryStatus Status { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ReceivedByName { get; set; }
    public bool HasSignature { get; set; }
    public string? PhotoUrl { get; set; }
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
}
