using FluentValidation;

namespace FHSMS.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.InitialBuyingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialSellingPrice).GreaterThanOrEqualTo(0);
        // The company must not sell at a loss: selling price can never be
        // set below buying price. This is the same guardrail applied again
        // in SchedulePriceCommandValidator for every later price change.
        RuleFor(x => x.InitialSellingPrice)
            .GreaterThanOrEqualTo(x => x.InitialBuyingPrice)
            .WithMessage("Selling price must be greater than or equal to buying price so the company doesn't lose money.");
    }
}
