using MediatR;

namespace FHSMS.Application.Reports.Queries.GetAgentsManagementSummary;

/// <summary>
/// The admin dashboard's "Agents Management" group in one call: the day's
/// order activity, the day's revenue/commission/bonus/profit figures, and
/// the full agent roster with each agent's kg moved that day.
///
/// Profit formula (see SalesSummaryDto for the full-period equivalent):
///   GrossProfitOnGoods = ProductSalesRevenue - AmountPaidToFarmers
///   NetProfit = GrossProfitOnGoods + TotalCommission - HotelAgentBonusTotal - FarmerAgentBonusTotal - DriverTripCost
/// Tax is excluded from profit entirely (pass-through to government).
/// DriverTripCost only counts APPROVED driver payments generated today - a
/// real company cost, since no delivery-fee line item exists on any invoice
/// to bill this through to the hotel.
/// GrossRevenue (Invoice.GrandTotal summed) is what hotels are billed in
/// total, shown as "total billed" context - it is NOT the same as profit.
/// </summary>
public record GetAgentsManagementSummaryQuery(DateTime Date) : IRequest<AgentsManagementSummaryDto>;

public class AgentsManagementSummaryDto
{
    public int DailyOrderCount { get; set; }
    /// <summary>Sum of today's Invoice.GrandTotal - what hotels were billed in total today. Informational "total billed" context, not company profit - see class remarks.</summary>
    public decimal GrossRevenue { get; set; }
    public decimal TaxCollected { get; set; }
    public decimal TotalCommission { get; set; }
    /// <summary>The platform commission rate currently configured (e.g. 2 for 2%) - for labeling only; TotalCommission itself is the exact sum frozen on today's invoices.</summary>
    public decimal? CommissionRatePercent { get; set; }
    public decimal AgentBonus { get; set; }
    /// <summary>Bonuses earned by hotel agents today (Commission records sourced from an Invoice).</summary>
    public decimal HotelAgentBonusTotal { get; set; }
    /// <summary>Bonuses earned by farmer agents today (Commission records sourced from a StockReceipt).</summary>
    public decimal FarmerAgentBonusTotal { get; set; }
    /// <summary>Sum of today's Invoice.Subtotal - selling price x quantity, the revenue from the goods themselves. This is what profit is actually computed against, not GrossRevenue.</summary>
    public decimal ProductSalesRevenue { get; set; }
    /// <summary>What the company owes/has paid farmers themselves for produce received today (sum of approved FarmerInvoice.TotalAmount) - the cost of goods sold.</summary>
    public decimal AmountPaidToFarmers { get; set; }
    /// <summary>ProductSalesRevenue - AmountPaidToFarmers.</summary>
    public decimal GrossProfitOnGoods { get; set; }
    /// <summary>GrossProfitOnGoods + TotalCommission - HotelAgentBonusTotal - FarmerAgentBonusTotal - DriverTripCost. The company's actual bottom line for today.</summary>
    public decimal NetProfit { get; set; }
    /// <summary>What's paid out to drivers for APPROVED trip payments generated today - a real company cost, subtracted in NetProfit above.</summary>
    public decimal DriverTripCost { get; set; }
    public List<RegisteredAgentDto> Agents { get; set; } = new();
}

public class RegisteredAgentDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string FullName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public bool IsOnline { get; set; }
    public decimal TodayOrderedKg { get; set; }
    /// <summary>Lifetime commission this agent has earned (excludes Cancelled), so admin can see and pay each agent's own value - not just the day's rolled-up total above.</summary>
    public decimal TotalCommissionEarned { get; set; }
    /// <summary>Portion of TotalCommissionEarned not yet marked Paid - what's actually owed to this agent right now.</summary>
    public decimal UnpaidCommission { get; set; }
}
