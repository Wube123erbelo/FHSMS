using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(Guid? CustomerId = null, InvoiceStatus? Status = null) : IRequest<List<InvoiceDto>>;
