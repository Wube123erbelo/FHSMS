using FHSMS.Application.Categories.Commands.CreateCategory;
using FHSMS.Application.Categories.Commands.DeleteCategory;
using FHSMS.Application.Categories.Commands.UpdateCategory;
using FHSMS.Application.Categories.Queries.GetCategories;
using FHSMS.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class CategoriesController : ApiControllerBase
{
    public record UpdateCategoryRequest(string Name, string? Description, bool IsActive);

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
        => Ok(await Mediator.Send(new GetCategoriesQuery()));

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> Create(CreateCategoryCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{categoryId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(Guid categoryId, UpdateCategoryRequest request)
    {
        await Mediator.Send(new UpdateCategoryCommand(categoryId, request.Name, request.Description, request.IsActive));
        return NoContent();
    }

    [HttpDelete("{categoryId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid categoryId)
    {
        await Mediator.Send(new DeleteCategoryCommand(categoryId));
        return NoContent();
    }
}
