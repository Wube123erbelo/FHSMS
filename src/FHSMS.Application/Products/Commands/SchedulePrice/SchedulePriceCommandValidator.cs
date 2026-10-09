using FluentValidation;

namespace FHSMS.Application.Products.Commands.SchedulePrice;

public class SchedulePriceCommandValidator : AbstractValidator<SchedulePriceCommand>
{
    public SchedulePriceCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
