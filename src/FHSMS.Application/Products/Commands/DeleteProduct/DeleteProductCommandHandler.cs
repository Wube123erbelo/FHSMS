using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteProductCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        product.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
