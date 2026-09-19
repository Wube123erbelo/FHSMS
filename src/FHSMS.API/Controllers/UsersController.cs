using FHSMS.Application.Users.Commands.AdminResetPassword;
using FHSMS.Application.Users.Commands.CreateUser;
using FHSMS.Application.Users.Commands.SetUserActive;
using FHSMS.Application.Users.Commands.UpdateUser;
using FHSMS.Application.Users.Queries.GetUsers;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

/// <summary>
/// Admin-only management of every user account - agents, hotel/customer
/// accounts, and other admins. This is the only place SuperAdmin,
/// HotelAgent, or FarmerAgent accounts can be created (see
/// RegisterCommandHandler for why the public sign-up endpoint refuses those
/// roles).
/// </summary>
[Authorize(Roles = "SuperAdmin")]
public class UsersController : ApiControllerBase
{
    public record UpdateUserRequest(string FullName, UserRole Role, bool IsActive, string? Phone = null, string? Location = null);
    public record ResetPasswordRequest(string NewPassword);

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
        => Ok(await Mediator.Send(new GetUsersQuery()));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateUserCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(Guid userId, UpdateUserRequest request)
    {
        await Mediator.Send(new UpdateUserCommand(userId, request.FullName, request.Role, request.IsActive, request.Phone, request.Location));
        return NoContent();
    }

    /// <summary>Issues a fresh password for a user directly - the credential an admin hands a field agent so they can log in.</summary>
    [HttpPost("{userId:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid userId, ResetPasswordRequest request)
    {
        await Mediator.Send(new AdminResetPasswordCommand(userId, request.NewPassword));
        return NoContent();
    }

    [HttpPost("{userId:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid userId)
    {
        await Mediator.Send(new SetUserActiveCommand(userId, false));
        return NoContent();
    }

    [HttpPost("{userId:guid}/activate")]
    public async Task<IActionResult> Activate(Guid userId)
    {
        await Mediator.Send(new SetUserActiveCommand(userId, true));
        return NoContent();
    }
}
