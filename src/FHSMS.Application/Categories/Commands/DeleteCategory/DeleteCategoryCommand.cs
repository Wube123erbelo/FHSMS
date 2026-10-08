using MediatR;

namespace FHSMS.Application.Categories.Commands.DeleteCategory;

/// <summary>Soft-deletes (deactivates) a category - products referencing it are untouched.</summary>
public record DeleteCategoryCommand(Guid CategoryId) : IRequest;
