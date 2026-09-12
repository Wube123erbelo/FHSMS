using FluentValidation;

namespace FHSMS.Application.Tax.Commands.ConfigureTax;

public class ConfigureTaxCommandValidator : AbstractValidator<ConfigureTaxCommand>
{
    public ConfigureTaxCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rate).InclusiveBetween(0, 100);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
