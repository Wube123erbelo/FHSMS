using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Payments.Queries.GetPaymentClaims;

/// <summary>Defaults to the Pending ones - the admin's actual work queue.</summary>
public record GetPaymentClaimsQuery(PaymentStatus? Status = null) : IRequest<List<PaymentClaimDto>>;
