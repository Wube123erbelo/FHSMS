using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Receipts.Queries.GetReceiptById;

public record GetReceiptByIdQuery(Guid ReceiptId) : IRequest<ReceiptDto>;
