using FHSMS.Application.Auth.Commands.AdminLogin;
using FHSMS.Application.Auth.Commands.ChangePassword;
using FHSMS.Application.Auth.Commands.ForgotPassword;
using FHSMS.Application.Auth.Commands.Login;
using FHSMS.Application.Auth.Commands.Logout;
using FHSMS.Application.Auth.Commands.Register;
using FHSMS.Application.Auth.Commands.ResetPassword;
using FHSMS.Application.Auth.Commands.TwoFactor;
using FHSMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUser;

    public AuthController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    public record RefreshTokenRequest(string Token);
    public record TwoFactorCodeRequest(string Code);
    public record DisableTwoFactorRequest(string CurrentPassword);
    public record VerifyTwoFactorLoginRequest(Guid PendingUserId, string Code);

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register(RegisterCommand command)
        => Ok(await Mediator.Send(command));

    /// <summary>Staff and customer sign-in - any active role. Returns a JWT plus a rotating refresh token.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginCommand command)
        => Ok(await Mediator.Send(command with { IpAddress = ClientIp }));

    /// <summary>
    /// Admin portal sign-in. Same credential check as /login, but rejects any
    /// account that isn't SuperAdmin - see AdminLoginCommandHandler.
    /// </summary>
    [HttpPost("admin-login")]
    public async Task<ActionResult<LoginResult>> AdminLogin(AdminLoginCommand command)
        => Ok(await Mediator.Send(command with { IpAddress = ClientIp }));

    /// <summary>
    /// Exchanges a still-valid refresh token for a new JWT + new refresh token
    /// (rotation) - call this instead of forcing a full re-login when the
    /// access token expires. See RefreshTokenCommandHandler for the
    /// reuse-detection/revoke-all-sessions behaviour if a used token is replayed.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResult>> Refresh(RefreshTokenRequest request)
        => Ok(await Mediator.Send(new Application.Auth.Commands.RefreshAccessToken.RefreshTokenCommand(request.Token, ClientIp)));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        await Mediator.Send(new LogoutCommand(request.Token));
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        await Mediator.Send(command);
        // Always 204, regardless of whether the email matched a user - see ForgotPasswordCommandHandler.
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// The target user is always the caller (taken from the JWT), never a
    /// client-supplied ID - otherwise any authenticated user could change
    /// anyone else's password by guessing their GUID.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        if (_currentUser.UserId is not { } userId)
            return Unauthorized();

        await Mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword));
        return NoContent();
    }

    /// <summary>Step 1 of enabling 2FA - generates a secret and QR-code URI. Not active until /2fa/confirm succeeds.</summary>
    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<ActionResult<SetupTwoFactorResult>> SetupTwoFactor()
    {
        if (_currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await Mediator.Send(new SetupTwoFactorCommand(userId)));
    }

    /// <summary>Step 2 - proves the authenticator app was set up correctly before 2FA actually starts being enforced.</summary>
    [HttpPost("2fa/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmTwoFactor(TwoFactorCodeRequest request)
    {
        if (_currentUser.UserId is not { } userId) return Unauthorized();
        await Mediator.Send(new ConfirmTwoFactorCommand(userId, request.Code));
        return NoContent();
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor(DisableTwoFactorRequest request)
    {
        if (_currentUser.UserId is not { } userId) return Unauthorized();
        await Mediator.Send(new DisableTwoFactorCommand(userId, request.CurrentPassword));
        return NoContent();
    }

    /// <summary>Completes a login that /login or /admin-login paused with RequiresTwoFactor = true.</summary>
    [HttpPost("2fa/login-verify")]
    public async Task<ActionResult<LoginResult>> VerifyTwoFactorLogin(VerifyTwoFactorLoginRequest request)
        => Ok(await Mediator.Send(new VerifyTwoFactorLoginCommand(request.PendingUserId, request.Code, ClientIp)));
}
