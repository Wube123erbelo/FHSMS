using FHSMS.Application.Common.Extensions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Reports.Queries.GetAgentsManagementSummary;

public class GetAgentsManagementSummaryQueryHandler
    : IRequestHandler<GetAgentsManagementSummaryQuery, AgentsManagementSummaryDto>
{
    private readonly IApplicationDbContext _context;
    public GetAgentsManagementSummaryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<AgentsManagementSummaryDto> Handle(GetAgentsManagementSummaryQuery request, CancellationToken cancellationToken)
    {
        var dayStart = request.Date.Date.AsUtc();
        var dayEnd = dayStart.AddDays(1);

        var ordersToday = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.OrderDate >= dayStart && o.OrderDate < dayEnd && o.Status != OrderStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var invoicesToday = await _context.Invoices
            .Where(i => i.InvoiceDate >= dayStart && i.InvoiceDate < dayEnd && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var commissionsToday = await _context.Commissions
            .Where(c => c.CreatedAt >= dayStart && c.CreatedAt < dayEnd && c.Status != CommissionStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var farmerInvoicesToday = await _context.FarmerInvoices
            .Where(f => f.CreatedAt >= dayStart && f.CreatedAt < dayEnd && f.Status == FarmerInvoiceStatus.Approved)
            .ToListAsync(cancellationToken);

        var platformCommissionConfig = await _context.PlatformCommissionConfigurations
            .Include(c => c.Rates)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Same formula as GetSalesSummaryQueryHandler (see SalesSummaryDto
        // remarks) - both figures now come straight from what was actually
        // frozen at issue time, not re-estimated from the currently
        // configured rate.
        var grossRevenue = invoicesToday.Sum(i => i.GrandTotal);
        var taxToday = invoicesToday.Sum(i => i.TaxAmount);
        var totalCommission = invoicesToday.Sum(i => i.PlatformCommissionAmount);
        var productSalesRevenue = invoicesToday.Sum(i => i.Subtotal);
        var hotelAgentBonusTotal = commissionsToday.Where(c => c.SourceType == CommissionSourceType.Invoice).Sum(c => c.CommissionAmount);
        var farmerAgentBonusTotal = commissionsToday.Where(c => c.SourceType == CommissionSourceType.StockReceipt).Sum(c => c.CommissionAmount);
        var agentBonus = hotelAgentBonusTotal + farmerAgentBonusTotal;
        var amountPaidToFarmers = farmerInvoicesToday.Sum(f => f.TotalAmount);

        // Only APPROVED driver payments count today - see
        // GetSalesSummaryQueryHandler's remarks for why this mirrors
        // AmountPaidToFarmers's Approved-only filter instead of summing raw
        // Delivery.TripPrice regardless of admin review.
        var driverTripCost = await _context.DriverPayments
            .Where(p => p.Status == Domain.Enums.DriverPaymentStatus.Approved && p.CreatedAt >= dayStart && p.CreatedAt < dayEnd)
            .SumAsync(p => p.Amount, cancellationToken);

        // See AgentsManagementSummaryDto/SalesSummaryDto remarks for why
        // this is the correct profit formula.
        var grossProfitOnGoods = productSalesRevenue - amountPaidToFarmers;
        var netProfit = grossProfitOnGoods + totalCommission - hotelAgentBonusTotal - farmerAgentBonusTotal - driverTripCost;

        var agents = await _context.Users
            .Where(u => u.Role == UserRole.HotelAgent || u.Role == UserRole.FarmerAgent)
            .ToListAsync(cancellationToken);

        var agentIds = agents.Select(a => a.Id).ToList();

        // Lifetime commission per agent (not just today's) - what "Registered
        // Agents" needs to show so an admin can see and pay out each agent's
        // own value, not just the day's rolled-up total above.
        var allCommissionsByAgent = await _context.Commissions
            .Where(c => agentIds.Contains(c.AgentUserId) && c.Status != CommissionStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var stockInToday = await _context.InventoryTransactions
            .Where(t => t.Type == InventoryTransactionType.Receiving && t.CreatedAt >= dayStart && t.CreatedAt < dayEnd && t.AgentUserId != null)
            .ToListAsync(cancellationToken);

        var onlineThreshold = DateTime.UtcNow.AddMinutes(-5); // matches the "online" definition used everywhere else in the app (see UserDto/presence display)

        var roster = agents.Select(agent =>
        {
            var kgToday = agent.Role == UserRole.HotelAgent
                ? ordersToday.Where(o => o.AgentUserId == agent.Id).SelectMany(o => o.Items).Sum(i => i.Quantity)
                : stockInToday.Where(t => t.AgentUserId == agent.Id).Sum(t => t.QuantityChange);

            var agentCommissions = allCommissionsByAgent.Where(c => c.AgentUserId == agent.Id).ToList();

            return new RegisteredAgentDto
            {
                Id = agent.Id,
                Code = agent.Code,
                FullName = agent.FullName,
                Role = agent.Role.ToString(),
                Phone = agent.Phone,
                Location = agent.Location,
                IsActive = agent.IsActive,
                IsOnline = agent.LastSeenAt is { } lastSeen && lastSeen >= onlineThreshold,
                TodayOrderedKg = kgToday,
                TotalCommissionEarned = agentCommissions.Sum(c => c.CommissionAmount),
                UnpaidCommission = agentCommissions.Where(c => c.Status != CommissionStatus.Paid).Sum(c => c.CommissionAmount)
            };
        }).OrderBy(a => a.FullName).ToList();

        return new AgentsManagementSummaryDto
        {
            DailyOrderCount = ordersToday.Count,
            GrossRevenue = grossRevenue,
            TaxCollected = taxToday,
            TotalCommission = totalCommission,
            CommissionRatePercent = platformCommissionConfig is { IsEnabled: true } ? platformCommissionConfig.GetRateAsOf()?.Rate : null,
            AgentBonus = agentBonus,
            HotelAgentBonusTotal = hotelAgentBonusTotal,
            FarmerAgentBonusTotal = farmerAgentBonusTotal,
            ProductSalesRevenue = productSalesRevenue,
            AmountPaidToFarmers = amountPaidToFarmers,
            GrossProfitOnGoods = grossProfitOnGoods,
            NetProfit = netProfit,
            DriverTripCost = driverTripCost,
            Agents = roster
        };
    }
}
