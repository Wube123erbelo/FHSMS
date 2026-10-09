using FluentValidation;

namespace FHSMS.Application.Invoices.Commands.GenerateInvoice;

public class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
    }
}
