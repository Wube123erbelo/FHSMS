using MediatR;

namespace FHSMS.Application.Deliveries.Commands.UpdateDeliveryStatus;

public record DispatchDeliveryCommand(Guid DeliveryId) : IRequest;

/// <summary>
/// Marking a delivery as Delivered captures proof of delivery at that moment -
/// who received it, an optional signature image (base64) or photo URL, and
/// GPS coordinates if the driver's device provided them. None of these are
/// editable after the fact; if any of it is wrong, that's a dispute/return,
/// not a correction to this record.
/// </summary>
public record MarkDeliveredCommand(
    Guid DeliveryId,
    string? Notes,
    string? ReceivedByName = null,
    string? SignatureImageBase64 = null,
    string? PhotoUrl = null,
    double? Latitude = null,
    double? Longitude = null) : IRequest;

public record MarkDeliveryFailedCommand(Guid DeliveryId, string? Notes) : IRequest;
