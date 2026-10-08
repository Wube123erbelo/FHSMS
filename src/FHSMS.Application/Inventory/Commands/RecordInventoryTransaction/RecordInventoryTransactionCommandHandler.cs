using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Inventory.Events;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Inventory.Commands.RecordInventoryTransaction;

public class RecordInventoryTransactionCommandHandler
    : IRequestHandler<RecordInventoryTransactionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;
    private readonly ICurrentUserService _currentUser;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public RecordInventoryTransactionCommandHandler(
        IApplicationDbContext context, IPublisher publisher, ICurrentUserService currentUser, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _publisher = publisher;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(RecordInventoryTransactionCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // Same anti-spoofing rule as CreateOrderCommandHandler: a farmer agent
        // (web, mobile, or Telegram - all through this one command) cannot log
        // stock "as" a different agent by editing a request body. Everyone else
        // (SuperAdmin correcting a record, an internal adjustment with no agent
        // involved) keeps whatever they explicitly passed.
        var agentUserId = _currentUser.Role == "FarmerAgent" ? _currentUser.UserId : request.AgentUserId;

        if (request.FarmerId is { } farmerId)
        {
            var farmerExists = await _context.Farmers.AnyAsync(f => f.Id == farmerId, cancellationToken);
            if (!farmerExists)
                throw new NotFoundException(nameof(Farmer), farmerId);
        }

        var isPreConfirmed = _currentUser.Role == "SuperAdmin";

        var transaction = request.Type switch
        {
            InventoryTransactionType.Receiving =>
                Domain.Entities.InventoryTransaction.Receive(
                    request.ProductId, request.Quantity, request.Reference, request.Notes,
                    request.FarmerId, agentUserId, isConfirmed: isPreConfirmed),
            InventoryTransactionType.Issuing =>
                Domain.Entities.InventoryTransaction.Issue(request.ProductId, request.Quantity, request.OrderId, request.Notes),
            InventoryTransactionType.Damage =>
                Domain.Entities.InventoryTransaction.Damage(request.ProductId, request.Quantity, request.Notes),
            InventoryTransactionType.Wastage =>
                Domain.Entities.InventoryTransaction.Wastage(request.ProductId, request.Quantity, request.Notes),
            InventoryTransactionType.Adjustment =>
                Domain.Entities.InventoryTransaction.Adjust(request.ProductId, request.Quantity, request.Notes),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Type))
        };

        _context.InventoryTransactions.Add(transaction);

        // Auto-generate the farmer-side invoice the instant stock is
        // received - Quantity x the product's CURRENT buying price, frozen
        // here, never a hand-typed cost. Exactly one is ever created per
        // transaction because this is the only place a Receiving transaction
        // is created, and the two are always saved together in this same
        // SaveChangesAsync call below.
        if (transaction.Type == InventoryTransactionType.Receiving)
        {
            var buyingPrice = product.GetBuyingPriceAsOf()?.Price
                ?? throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.ProductId), $"Product '{product.Name}' has no active buying price and cannot be received into stock.")
                });

            var invoiceNumber = await _numberGenerator.NextFarmerInvoiceNumberAsync(cancellationToken);
            var farmerInvoice = Domain.Entities.FarmerInvoice.Create(
                invoiceNumber, transaction.Id, request.ProductId, request.FarmerId, agentUserId,
                request.Quantity, buyingPrice, isPreApproved: isPreConfirmed, approvedByUserId: isPreConfirmed ? _currentUser.UserId : null);

            _context.FarmerInvoices.Add(farmerInvoice);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Commission accrual moved to ConfirmStockReceiptCommandHandler - an
        // agent shouldn't earn commission on stock that turns out to be
        // wrong or gets rejected at confirmation. An admin's own Receiving
        // entry is auto-confirmed above, so it fires immediately for them
        // via the same path (see IsConfirmed branch there).
        if (transaction.Type == InventoryTransactionType.Receiving && transaction.IsConfirmed && agentUserId is not null)
        {
            await _publisher.Publish(
                new StockReceivedEvent(transaction.Id, request.ProductId, request.Quantity, request.FarmerId, agentUserId),
                cancellationToken);
        }

        return transaction.Id;
    }
}
