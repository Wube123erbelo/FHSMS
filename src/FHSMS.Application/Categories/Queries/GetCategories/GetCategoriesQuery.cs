using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;
