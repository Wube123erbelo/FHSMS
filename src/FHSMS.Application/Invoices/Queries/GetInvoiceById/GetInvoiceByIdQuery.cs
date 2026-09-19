using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<InvoiceDto>;
