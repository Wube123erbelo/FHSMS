using FluentValidation;

namespace FHSMS.Application.Deliveries.Commands.CreateDelivery;

public class CreateDeliveryCommandValidator : AbstractValidator<CreateDeliveryCommand>
{
    public CreateDeliveryCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.DestinationAddress).NotEmpty().MaximumLength(300);
    }
}
