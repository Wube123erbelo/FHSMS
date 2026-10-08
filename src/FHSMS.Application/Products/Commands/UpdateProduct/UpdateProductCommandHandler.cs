using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateProductCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.UnitId = request.UnitId;
        product.TaxProfile = request.TaxProfile;
        product.LowStockThreshold = request.LowStockThreshold;
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
