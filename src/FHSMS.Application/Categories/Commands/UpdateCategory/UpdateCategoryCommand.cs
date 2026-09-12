using MediatR;

namespace FHSMS.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid CategoryId, string Name, string? Description, bool IsActive) : IRequest;
