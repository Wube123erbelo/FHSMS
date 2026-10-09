using MediatR;

namespace FHSMS.Application.DriverPayments.Commands.ApproveDriverPayment;

/// <summary>
/// Admin confirms a driver's trip payment - the driver-side counterpart to
/// ConfirmStockReceiptCommand. DriverWasPaid records whether the company has
/// actually disbursed the amount yet, same principle as FarmerWasPaid there.
/// </summary>
public record ApproveDriverPaymentCommand(Guid DriverPaymentId, bool DriverWasPaid) : IRequest;
