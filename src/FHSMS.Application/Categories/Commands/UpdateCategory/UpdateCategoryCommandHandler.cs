using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductCategory), request.CategoryId);

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
