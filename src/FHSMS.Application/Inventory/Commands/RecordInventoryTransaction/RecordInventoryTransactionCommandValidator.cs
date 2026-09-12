using FluentValidation;

namespace FHSMS.Application.Inventory.Commands.RecordInventoryTransaction;

public class RecordInventoryTransactionCommandValidator : AbstractValidator<RecordInventoryTransactionCommand>
{
    public RecordInventoryTransactionCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).NotEqual(0);
    }
}
