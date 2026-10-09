using FluentValidation;

namespace FHSMS.Application.Farmers.Commands.CreateFarmer;

public class CreateFarmerCommandValidator : AbstractValidator<CreateFarmerCommand>
{
    public CreateFarmerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
