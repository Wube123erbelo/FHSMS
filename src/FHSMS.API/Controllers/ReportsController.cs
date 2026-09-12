using FHSMS.Application.Reports.Queries.GetAgentsManagementSummary;
using FHSMS.Application.Reports.Queries.GetCommissionSummary;
using FHSMS.Application.Reports.Queries.GetRevenueByPeriod;
using FHSMS.Application.Reports.Queries.GetSalesSummary;
using FHSMS.Application.Reports.Queries.GetTopCustomers;
using FHSMS.Application.Reports.Queries.GetTopProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class ReportsController : ApiControllerBase
{
    /// <summary>The admin dashboard's "Agents Management" group - daily orders, gross revenue/commission/bonus/net payout, and the full agent roster with today's activity and online status, all for one selected day.</summary>
    [HttpGet("agents-management-summary")]
    public async Task<ActionResult<AgentsManagementSummaryDto>> AgentsManagementSummary([FromQuery] DateTime date)
        => Ok(await Mediator.Send(new GetAgentsManagementSummaryQuery(date)));

    [HttpGet("sales-summary")]
    public async Task<ActionResult<SalesSummaryDto>> SalesSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await Mediator.Send(new GetSalesSummaryQuery(from, to)));

    [HttpGet("revenue-by-period")]
    public async Task<ActionResult<List<RevenuePeriodDto>>> RevenueByPeriod(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] ReportGrouping groupBy = ReportGrouping.Day)
        => Ok(await Mediator.Send(new GetRevenueByPeriodQuery(from, to, groupBy)));

    [HttpGet("top-products")]
    public async Task<ActionResult<List<TopProductDto>>> TopProducts(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 10)
        => Ok(await Mediator.Send(new GetTopProductsQuery(from, to, top)));

    /// <summary>The dashboard's "Top Hotels" leaderboard.</summary>
    [HttpGet("top-customers")]
    public async Task<ActionResult<List<TopCustomerDto>>> TopCustomers(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 5)
        => Ok(await Mediator.Send(new GetTopCustomersQuery(from, to, top)));

    /// <summary>Grouped by agent type - HotelAgent rows are the "Total Commission (2%)" figure, FarmerAgent rows are the "Agent Bonus (per kg)" figure, kept explicitly separate rather than summed together.</summary>
    [HttpGet("commission-summary")]
    public async Task<ActionResult<List<CommissionSummaryDto>>> CommissionSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await Mediator.Send(new GetCommissionSummaryQuery(from, to)));
}
