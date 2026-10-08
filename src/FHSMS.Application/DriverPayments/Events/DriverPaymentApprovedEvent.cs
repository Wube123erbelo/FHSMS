using MediatR;

namespace FHSMS.Application.DriverPayments.Events;

/// <summary>Published whenever a driver payment is approved (POST /driver-payments/{id}/approve).</summary>
public record DriverPaymentApprovedEvent(Guid DriverPaymentId, Guid? DriverId, decimal Amount, bool DriverWasPaid) : INotification;
