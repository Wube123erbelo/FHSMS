using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

/// <summary>The driver-side counterpart to FarmerInvoiceDto - what the company owes a driver for one completed trip.</summary>
public class DriverPaymentDto
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public Guid? OrderId { get; set; }
    public string? DestinationAddress { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public decimal Amount { get; set; }
    public DriverPaymentStatus Status { get; set; }
    public bool DriverWasPaid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
