using FluentValidation;

namespace FHSMS.Application.PlatformCommission.Commands.ConfigurePlatformCommission;

public class ConfigurePlatformCommissionCommandValidator : AbstractValidator<ConfigurePlatformCommissionCommand>
{
    public ConfigurePlatformCommissionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rate).InclusiveBetween(0, 100);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
