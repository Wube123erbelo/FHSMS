using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.DriverPayments.Queries.GetDriverPayments;

/// <summary>Role-scoped in the handler: a Driver only ever gets back their own payment records; SuperAdmin sees everything.</summary>
public record GetDriverPaymentsQuery(DriverPaymentStatus? Status = null) : IRequest<List<DriverPaymentDto>>;
