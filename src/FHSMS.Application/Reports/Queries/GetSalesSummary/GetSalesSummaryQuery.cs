using MediatR;

namespace FHSMS.Application.Reports.Queries.GetSalesSummary;

public record GetSalesSummaryQuery(DateTime? From, DateTime? To) : IRequest<SalesSummaryDto>;

/// <summary>
/// Formula reference (also returned as FormulaLabel below so the UI never
/// has to hardcode it in more than one place):
///
///   ProductSalesRevenue      = sum of Invoice.Subtotal (selling price x qty, what hotels are charged for the goods themselves)
///   CostOfGoodsSold          = AmountPaidToFarmers (buying price x qty, what the company paid farmers for those same goods)
///   GrossProfitOnGoods       = ProductSalesRevenue - CostOfGoodsSold
///   NetProfit                = GrossProfitOnGoods + TotalCommission - HotelAgentBonusTotal - FarmerAgentBonusTotal - DriverTripCost
///
/// DriverTripCost is what a driver is paid for a completed trip - a real
/// company cost, since no delivery-fee line item exists on any invoice to
/// bill this through to the hotel. Like AmountPaidToFarmers, only counts
/// APPROVED DriverPayment records (see DriverPayment/DriverPaymentsController),
/// not every Delivery.TripPrice regardless of admin review. It is netted
/// against profit the same way agent bonuses are.
///
/// Tax is deliberately excluded from every line above - it is collected on
/// behalf of the government and passed through, never money the company
/// keeps, so it plays no part in profit (TotalTaxCollected is reported
/// separately, for compliance visibility only).
///
/// TotalRevenue (Invoice.GrandTotal summed = Subtotal + Tax + Commission +
/// HotelAgentBonus) is what hotels are billed in total, including the two
/// pass-through/cost components above - it is NOT the same thing as company
/// profit and is shown purely as "total billed" context.
/// </summary>
public class SalesSummaryDto
{
    public int InvoiceCount { get; set; }
    /// <summary>Sum of Invoice.GrandTotal - what hotels are billed in total. Informational "total billed" context, not company profit - see class remarks.</summary>
    public decimal TotalRevenue { get; set; }
    public decimal TotalTaxCollected { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal AverageInvoiceValue { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int OrderCount { get; set; }
    public int CancelledOrderCount { get; set; }
    /// <summary>The company's own commission, exactly as frozen on each invoice at issue time (Invoice.PlatformCommissionAmount) - never re-estimated. Reflects whatever rate was configured at the moment each invoice was generated, so it stays correct even across a mid-period rate change.</summary>
    public decimal TotalCommission { get; set; }
    /// <summary>The platform commission rate currently configured (e.g. 2 for 2%) - for labeling only. Individual invoices in the period may have been issued at a different rate if Admin changed it mid-period; TotalCommission above is always the exact sum regardless.</summary>
    public decimal? CommissionRatePercent { get; set; }
    /// <summary>HotelAgentBonusTotal + FarmerAgentBonusTotal - what's owed/paid out to field agents over the period, combined.</summary>
    public decimal AgentBonus { get; set; }
    /// <summary>Bonuses earned by hotel agents for placing orders (Commission records sourced from an Invoice), summed separately so Admin can see this figure at a glance without doing the split themselves.</summary>
    public decimal HotelAgentBonusTotal { get; set; }
    /// <summary>Bonuses earned by farmer agents for logging stock receipts (Commission records sourced from a StockReceipt), summed separately.</summary>
    public decimal FarmerAgentBonusTotal { get; set; }
    /// <summary>Sum of Invoice.Subtotal - selling price x quantity, the revenue from the goods themselves before tax/commission/bonus are layered on. This is what profit is actually computed against, not TotalRevenue.</summary>
    public decimal ProductSalesRevenue { get; set; }
    /// <summary>What the company owes/has paid to farmers for the goods sold in this same period (sum of approved FarmerInvoice.TotalAmount) - the cost of goods sold.</summary>
    public decimal AmountPaidToFarmers { get; set; }
    /// <summary>ProductSalesRevenue - AmountPaidToFarmers.</summary>
    public decimal GrossProfitOnGoods { get; set; }
    /// <summary>GrossProfitOnGoods + TotalCommission - HotelAgentBonusTotal - FarmerAgentBonusTotal - DriverTripCost. The company's actual bottom line for the period - see class remarks for the full formula.</summary>
    public decimal NetProfit { get; set; }
    /// <summary>Plain-English formula string for direct display, so the dashboard/reports UI never has to hardcode or re-derive it: "Net profit = (Product sales revenue - Paid to farmers) + Total commission - Hotel agent bonus - Farmer agent bonus - Driver trip cost".</summary>
    public string NetProfitFormula { get; set; } = "Net profit = (Product sales revenue - Paid to farmers) + Total commission - Hotel agent bonus - Farmer agent bonus - Driver trip cost";
    /// <summary>What's paid out to drivers for APPROVED trip payments (DriverPayment.Status == Approved) generated within the period - a real company cost, subtracted in NetProfit above.</summary>
    public decimal DriverTripCost { get; set; }
}
