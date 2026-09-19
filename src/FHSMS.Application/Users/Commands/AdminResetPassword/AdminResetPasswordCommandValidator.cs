using FluentValidation;

namespace FHSMS.Application.Users.Commands.AdminResetPassword;

public class AdminResetPasswordCommandValidator : AbstractValidator<AdminResetPasswordCommand>
{
    public AdminResetPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword).MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
