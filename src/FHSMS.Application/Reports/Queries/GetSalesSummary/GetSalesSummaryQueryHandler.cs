using FHSMS.Application.Common.Extensions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Reports.Queries.GetSalesSummary;

/// <summary>
/// Headline sales numbers for the admin dashboard/reports screen. Cancelled
/// invoices are excluded from revenue/tax figures but do still count in the
/// separate cancellation stat, so a spike in cancellations is visible without
/// distorting the revenue line.
/// </summary>
public class GetSalesSummaryQueryHandler : IRequestHandler<GetSalesSummaryQuery, SalesSummaryDto>
{
    private readonly IApplicationDbContext _context;
    public GetSalesSummaryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<SalesSummaryDto> Handle(GetSalesSummaryQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.AsUtc() ?? DateTime.UtcNow.AddMonths(-1);
        var to = request.To.AsUtc() ?? DateTime.UtcNow;

        var invoices = await _context.Invoices
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var orders = await _context.Orders
            .Where(o => o.OrderDate >= from && o.OrderDate <= to)
            .ToListAsync(cancellationToken);

        // Both commission figures below now come straight from what was
        // actually frozen onto each invoice/commission at the moment it was
        // issued (see Invoice.ApplyCompanyCharges and HotelAgentBonusCalculator)
        // - never re-estimated from the currently-configured rate, so a
        // mid-period rate change never distorts a past period's report.
        var commissions = await _context.Commissions
            .Where(c => c.CreatedAt >= from && c.CreatedAt <= to && c.Status != CommissionStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var farmerInvoices = await _context.FarmerInvoices
            .Where(f => f.CreatedAt >= from && f.CreatedAt <= to && f.Status == FarmerInvoiceStatus.Approved)
            .ToListAsync(cancellationToken);

        var platformCommissionConfig = await _context.PlatformCommissionConfigurations
            .Include(c => c.Rates)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var totalRevenue = invoices.Sum(i => i.GrandTotal);
        var totalTax = invoices.Sum(i => i.TaxAmount);
        var totalCommission = invoices.Sum(i => i.PlatformCommissionAmount);
        var productSalesRevenue = invoices.Sum(i => i.Subtotal);
        var hotelAgentBonusTotal = commissions.Where(c => c.SourceType == CommissionSourceType.Invoice).Sum(c => c.CommissionAmount);
        var farmerAgentBonusTotal = commissions.Where(c => c.SourceType == CommissionSourceType.StockReceipt).Sum(c => c.CommissionAmount);
        var agentBonus = hotelAgentBonusTotal + farmerAgentBonusTotal;
        var amountPaidToFarmers = farmerInvoices.Sum(f => f.TotalAmount);

        // Only APPROVED driver payments count as a real cost here - same
        // principle as AmountPaidToFarmers only counting Approved
        // FarmerInvoice rows. A payment auto-generated at MarkDelivered but
        // not yet reviewed by an admin isn't yet a confirmed company
        // obligation.
        var driverTripCost = await _context.DriverPayments
            .Where(p => p.Status == Domain.Enums.DriverPaymentStatus.Approved && p.CreatedAt >= from && p.CreatedAt <= to)
            .SumAsync(p => p.Amount, cancellationToken);

        // See SalesSummaryDto class remarks for why this is the correct
        // profit formula and TotalRevenue (GrandTotal) is not: tax is
        // pass-through money the company never keeps, and the platform
        // commission is genuine company revenue that must be ADDED, not
        // netted away against itself.
        var grossProfitOnGoods = productSalesRevenue - amountPaidToFarmers;
        var netProfit = grossProfitOnGoods + totalCommission - hotelAgentBonusTotal - farmerAgentBonusTotal - driverTripCost;

        return new SalesSummaryDto
        {
            InvoiceCount = invoices.Count,
            TotalRevenue = totalRevenue,
            TotalTaxCollected = totalTax,
            TotalDiscount = invoices.Sum(i => i.Discount),
            AverageInvoiceValue = invoices.Count > 0 ? Math.Round(totalRevenue / invoices.Count, 2) : 0,
            TotalOutstanding = invoices.Sum(i => i.BalanceDue),
            OrderCount = orders.Count,
            CancelledOrderCount = orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalCommission = totalCommission,
            CommissionRatePercent = platformCommissionConfig is { IsEnabled: true } ? platformCommissionConfig.GetRateAsOf()?.Rate : null,
            AgentBonus = agentBonus,
            HotelAgentBonusTotal = hotelAgentBonusTotal,
            FarmerAgentBonusTotal = farmerAgentBonusTotal,
            ProductSalesRevenue = productSalesRevenue,
            AmountPaidToFarmers = amountPaidToFarmers,
            GrossProfitOnGoods = grossProfitOnGoods,
            NetProfit = netProfit,
            DriverTripCost = driverTripCost
        };
    }
}
