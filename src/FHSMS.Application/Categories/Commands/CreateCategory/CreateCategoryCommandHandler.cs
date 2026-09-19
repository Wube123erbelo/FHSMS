using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;

namespace FHSMS.Application.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public CreateCategoryCommandHandler(IApplicationDbContext context, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ProductCategory
        {
            Code = await _numberGenerator.NextCategoryCodeAsync(cancellationToken),
            Name = request.Name,
            Description = request.Description
        };
        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
