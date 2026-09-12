using MediatR;

namespace FHSMS.Application.Deliveries.Commands.ConfirmDeliveryReceipt;

/// <summary>
/// The hotel side (whoever placed the order, or an admin) confirms goods
/// actually arrived - a check on the driver's own MarkDelivered claim, not
/// a rubber stamp of it. Required before Order.Complete() can run.
/// </summary>
public record ConfirmDeliveryReceiptCommand(Guid DeliveryId) : IRequest;
