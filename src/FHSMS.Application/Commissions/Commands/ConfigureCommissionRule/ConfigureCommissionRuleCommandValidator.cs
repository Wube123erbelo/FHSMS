using FHSMS.Domain.Enums;
using FluentValidation;

namespace FHSMS.Application.Commissions.Commands.ConfigureCommissionRule;

public class ConfigureCommissionRuleCommandValidator : AbstractValidator<ConfigureCommissionRuleCommand>
{
    public ConfigureCommissionRuleCommandValidator()
    {
        When(x => x.Basis == CommissionBasis.PercentageOfInvoice, () =>
        {
            RuleFor(x => x.Percentage)
                .NotNull().WithMessage("Percentage is required for a percentage-of-invoice rule.")
                .InclusiveBetween(0, 100);
        });

        When(x => x.Basis == CommissionBasis.FlatRatePerQuantity, () =>
        {
            RuleFor(x => x.FlatRateAmount)
                .NotNull().WithMessage("Flat rate amount is required for a flat-rate-per-quantity rule.")
                .GreaterThanOrEqualTo(0);
        });
    }
}
