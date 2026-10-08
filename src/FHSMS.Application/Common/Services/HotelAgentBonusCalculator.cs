using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Common.Services;

/// <summary>Everything needed both to freeze the bonus onto the Invoice and to later create the matching Commission record - computed once, used twice.</summary>
public record HotelAgentBonusResult(
    decimal Amount,
    CommissionBasis? Basis,
    decimal? Percentage,
    decimal? FlatRateApplied,
    decimal? Quantity)
{
    public static readonly HotelAgentBonusResult None = new(0m, null, null, null, null);
}

/// <summary>
/// Computes the bonus owed to whichever agent placed an order, for the
/// invoice that order produced. Based on the invoice's Subtotal (the selling
/// price total, before tax/commission/bonus are layered on) - deliberately
/// NOT GrandTotal, because GrandTotal itself includes this bonus: basing the
/// bonus on GrandTotal would be circular.
///
/// Used from two places that must never disagree: GenerateInvoiceCommandHandler
/// (to freeze the amount onto the Invoice, satisfying "invoice must include
/// the hotel agent bonus") and AccrueCommissionOnInvoiceIssued (to create the
/// actual payable Commission record for the agent) - both call this, neither
/// duplicates the math.
/// </summary>
public static class HotelAgentBonusCalculator
{
    public static async Task<HotelAgentBonusResult> ComputeAsync(
        IApplicationDbContext context, Order order, decimal invoiceSubtotal, AgentType? agentType, CancellationToken cancellationToken)
    {
        if (order.AgentUserId is null || agentType is null)
            return HotelAgentBonusResult.None;

        var rule = await context.CommissionRules
            .FirstOrDefaultAsync(r => r.AgentType == agentType && r.IsActive, cancellationToken);

        // Commission is opt-in configuration, same as tax - silently skip if unset.
        if (rule is null)
            return HotelAgentBonusResult.None;

        if (rule.Basis == CommissionBasis.PercentageOfInvoice)
        {
            if (rule.Percentage is not { } percentage)
                return HotelAgentBonusResult.None;

            var amount = Math.Round(invoiceSubtotal * (percentage / 100m), 2);
            return new HotelAgentBonusResult(amount, CommissionBasis.PercentageOfInvoice, percentage, null, null);
        }

        // FlatRatePerQuantity - each order line is priced against its own
        // product's unit (with any per-unit override) and summed, since an
        // order can mix products across different units (kg, crate, litre...).
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var productUnits = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.UnitId, cancellationToken);

        decimal totalQuantity = 0m;
        decimal totalBonus = 0m;

        foreach (var item in order.Items)
        {
            if (!productUnits.TryGetValue(item.ProductId, out var unitId))
                continue; // product no longer exists - can't price this line, skip it rather than fail the whole invoice

            var effectiveRate = rule.GetEffectiveFlatRate(unitId);
            if (effectiveRate is not { } rate)
                continue; // no rate configured for this product's unit and no rule default

            totalQuantity += item.Quantity;
            totalBonus += Math.Round(item.Quantity * rate, 2);
        }

        if (totalQuantity <= 0 || totalBonus <= 0)
            return HotelAgentBonusResult.None;

        var blendedRate = Math.Round(totalBonus / totalQuantity, 4);
        return new HotelAgentBonusResult(totalBonus, CommissionBasis.FlatRatePerQuantity, null, blendedRate, totalQuantity);
    }
}
