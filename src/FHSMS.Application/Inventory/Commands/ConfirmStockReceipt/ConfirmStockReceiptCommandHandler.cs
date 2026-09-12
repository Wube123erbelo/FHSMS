using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Inventory.Events;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Inventory.Commands.ConfirmStockReceipt;

public class ConfirmStockReceiptCommandHandler : IRequestHandler<ConfirmStockReceiptCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublisher _publisher;

    public ConfirmStockReceiptCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPublisher publisher)
    {
        _context = context;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task Handle(ConfirmStockReceiptCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Must be logged in to confirm a stock receipt.");

        var transaction = await _context.InventoryTransactions
            .FirstOrDefaultAsync(t => t.Id == request.InventoryTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(InventoryTransaction), request.InventoryTransactionId);

        // The amount owed to the farmer comes from the invoice that was
        // auto-generated the instant this stock was logged (see
        // RecordInventoryTransactionCommandHandler) - never typed by hand
        // here. There is exactly one FarmerInvoice per transaction.
        var farmerInvoice = await _context.FarmerInvoices
            .FirstOrDefaultAsync(f => f.InventoryTransactionId == request.InventoryTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerInvoice), request.InventoryTransactionId);

        farmerInvoice.Approve(userId);
        transaction.ConfirmReceipt(userId, request.FarmerWasPaid, farmerInvoice.TotalAmount);
        await _context.SaveChangesAsync(cancellationToken);

        // Commission accrues now, not when the farmer agent originally logged
        // it - see RecordInventoryTransactionCommandHandler for why.
        if (transaction.AgentUserId is { } agentUserId)
        {
            await _publisher.Publish(
                new StockReceivedEvent(transaction.Id, transaction.ProductId, transaction.QuantityChange, transaction.FarmerId, agentUserId),
                cancellationToken);
        }
    }
}
