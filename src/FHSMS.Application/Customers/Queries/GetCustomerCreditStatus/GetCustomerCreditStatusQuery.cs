using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Customers.Queries.GetCustomerCreditStatus;

/// <summary>
/// Computes credit exposure on demand from live invoice balances rather than
/// a stored running total, so it can never drift out of sync - "current
/// exposure" always means "what the invoices say right now".
/// </summary>
public record GetCustomerCreditStatusQuery(Guid CustomerId) : IRequest<CustomerCreditStatusDto>;
