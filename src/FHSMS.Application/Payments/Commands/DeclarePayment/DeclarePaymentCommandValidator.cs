using FluentValidation;

namespace FHSMS.Application.Payments.Commands.DeclarePayment;

public class DeclarePaymentCommandValidator : AbstractValidator<DeclarePaymentCommand>
{
    public DeclarePaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);

        // A reference is what an admin actually matches against the bank
        // statement, so unlike a staff-recorded payment it isn't optional here.
        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("Enter the transfer reference from your bank.")
            .MaximumLength(100);
    }
}
