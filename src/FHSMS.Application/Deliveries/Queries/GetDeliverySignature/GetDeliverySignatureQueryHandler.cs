using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Deliveries.Queries.GetDeliverySignature;

public class GetDeliverySignatureQueryHandler : IRequestHandler<GetDeliverySignatureQuery, string?>
{
    private readonly IApplicationDbContext _context;
    public GetDeliverySignatureQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<string?> Handle(GetDeliverySignatureQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Delivery), request.DeliveryId);

        return delivery.SignatureImageBase64;
    }
}
