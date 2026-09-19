using FluentValidation;

namespace FHSMS.Application.BankReconciliation.Commands.RecordBankTransaction;

public class RecordBankTransactionCommandValidator : AbstractValidator<RecordBankTransactionCommand>
{
    public RecordBankTransactionCommandValidator()
    {
        RuleFor(x => x.BankAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
