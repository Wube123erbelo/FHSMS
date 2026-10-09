using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Services;
using FHSMS.Application.Invoices.Events;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Invoices.Commands.GenerateInvoice;

public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;
    private readonly ITaxEngine _taxEngine;
    private readonly IPublisher _publisher;

    public GenerateInvoiceCommandHandler(
        IApplicationDbContext context,
        IDocumentNumberGenerator numberGenerator,
        ITaxEngine taxEngine,
        IPublisher publisher)
    {
        _context = context;
        _numberGenerator = numberGenerator;
        _taxEngine = taxEngine;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // One invoice per order, hard rule - without this, clicking "generate
        // invoice" twice for the same order (a slow request retried, a double
        // click, a second admin on the same screen) would double the revenue,
        // tax, commission, and agent bonus. A genuine repeat purchase should
        // be a new Order (see DuplicateOrderCommand), which then gets its own
        // invoice here.
        var alreadyInvoiced = await _context.Invoices
            .AnyAsync(i => i.OrderId == order.Id && i.Status != InvoiceStatus.Cancelled, cancellationToken);
        if (alreadyInvoiced)
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.OrderId), "This order already has an invoice. To bill the same items again, place a new order instead.")
            });

        if (order.Status is not (OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.Shipped
            or OrderStatus.Delivered or OrderStatus.Completed))
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(order.Status), "Only a confirmed (or later-stage) order can be invoiced.")
            });

        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // Resolve the single active, enabled VAT-type configuration. In a system with
        // multiple simultaneous tax types this would loop per TaxType instead.
        var activeConfiguration = await _context.TaxConfigurations
            .Include(t => t.Rates)
            .Where(t => t.Status == TaxConfigurationStatus.Active && t.TaxType == TaxType.Vat)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var activeRate = activeConfiguration?.GetRateAsOf();

        var invoiceNumber = await _numberGenerator.NextInvoiceNumberAsync(cancellationToken);
        var invoice = new Invoice(
            invoiceNumber,
            order.Id,
            order.CustomerId,
            activeConfiguration?.Id,
            activeConfiguration?.IsEnabled ?? false,
            activeConfiguration?.CalculationMode);

        foreach (var orderItem in order.Items)
        {
            var taxProfile = products.TryGetValue(orderItem.ProductId, out var product)
                ? product.TaxProfile
                : TaxProfileType.StandardVat;

            var result = _taxEngine.Calculate(
                orderItem.Quantity,
                orderItem.UnitPrice,
                taxProfile,
                activeConfiguration,
                activeRate);

            invoice.AddItem(new InvoiceItem(
                invoice.Id,
                orderItem.ProductId,
                orderItem.ProductName,
                orderItem.Quantity,
                orderItem.UnitPrice,
                result.LineSubtotal,
                result.TaxProfileApplied,
                result.TaxRateApplied,
                result.TaxableAmount,
                result.TaxAmount,
                result.LineTotal));
        }

        invoice.Recalculate(request.Discount);

        // --- Company-side charges, layered on top of Subtotal, frozen at issue time ---
        // Both are computed against Subtotal (the selling-price total before
        // any of these charges), never against GrandTotal - GrandTotal is
        // itself Subtotal + these charges, so basing them on GrandTotal would
        // be circular (the charge would depend on a total that depends on
        // the charge).

        var platformCommissionConfig = await _context.PlatformCommissionConfigurations
            .Include(c => c.Rates)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var platformRate = platformCommissionConfig is { IsEnabled: true }
            ? platformCommissionConfig.GetRateAsOf()?.Rate ?? 0m
            : 0m;
        var platformCommissionAmount = Math.Round(invoice.Subtotal * platformRate / 100m, 2);

        var agentType = order.Source switch
        {
            OrderSourceType.HotelAgent => AgentType.HotelAgent,
            OrderSourceType.FarmerAgent => AgentType.FarmerAgent,
            _ => (AgentType?)null
        };

        var bonus = await HotelAgentBonusCalculator.ComputeAsync(_context, order, invoice.Subtotal, agentType, cancellationToken);

        invoice.ApplyCompanyCharges(platformCommissionConfig?.Id, platformRate, platformCommissionAmount, bonus.Amount);
        invoice.Issue();

        // Note: invoicing does not itself change the order's status - "has an
        // invoice" and "order lifecycle stage" are orthogonal per the state
        // machine (an order can be Confirmed, Preparing, Shipped, etc. and
        // already invoiced). Invoice.Status tracks the billing side separately.

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new InvoiceIssuedEvent(
                invoice.Id, order.Id, invoice.CustomerId, invoice.GrandTotal, invoice.Subtotal,
                order.AgentUserId, agentType, bonus),
            cancellationToken);

        return invoice.Id;
    }
}
