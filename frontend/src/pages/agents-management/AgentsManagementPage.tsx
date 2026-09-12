import { useState } from "react";
import { Users, TrendingUp, Percent, Coins, Wallet, ShoppingCart, Truck } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import { LoadingState, ErrorState } from "../../components/States";
import { BrandTooltip } from "../../components/charts/BrandTooltip";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import type { AgentsManagementSummaryDto, TopCustomerDto, TopProductDto, RevenuePeriodDto } from "../../api/types";
import {
  PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend
} from "recharts";

const CHART_COLORS = ["#264A3A", "#C9A227", "#A64B3B", "#3C6B54", "#9C7D1D", "#1C3B32"];

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export default function AgentsManagementPage() {
  const { t } = useTranslation();
  const [date, setDate] = useState(todayIso());
  const { data: summary, loading, error } = useFetch<AgentsManagementSummaryDto>(`/reports/agents-management-summary?date=${date}`, [date]);
  const { data: topProducts } = useFetch<TopProductDto[]>("/reports/top-products?top=6");
  const { data: revenueByPeriod } = useFetch<RevenuePeriodDto[]>("/reports/revenue-by-period?groupBy=Month");
  const { data: topCustomers } = useFetch<TopCustomerDto[]>("/reports/top-customers?top=5");

  return (
    <div>
      <PageHeader
        eyebrow={t("agentsManagement.eyebrow")}
        title={t("agentsManagement.title")}
        description={t("agentsManagement.description")}
        action={
          <div>
            <label className="label text-xs">{t("agentsManagement.viewingDate")}</label>
            <input type="date" className="input" value={date} max={todayIso()} onChange={(e) => setDate(e.target.value)} />
          </div>
        }
      />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}

      {summary && (
        <>
          {/* Profit formula (see AgentsManagementSummaryDto on the backend):
                GrossProfitOnGoods = ProductSalesRevenue - AmountPaidToFarmers
                NetProfit = GrossProfitOnGoods + TotalCommission - HotelAgentBonus - FarmerAgentBonus - DriverTripCost
              Tax plays no part in profit. DriverTripCost only counts trips
              actually Delivered today - a real cost, since no delivery-fee
              line item exists on any invoice. GrossRevenue (today's
              Invoice.GrandTotal summed) is what hotels were billed today,
              shown as context only. */}
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-3 xl:grid-cols-6">
            <SummaryCard icon={ShoppingCart} label={t("agentsManagement.dailyOrders")} value={summary.dailyOrderCount} />
            <SummaryCard icon={TrendingUp} label={t("agentsManagement.grossRevenue")} value={`${summary.grossRevenue.toFixed(2)} ${t("common.currency")}`} />
            <SummaryCard icon={TrendingUp} label={t("dashboard.productSalesRevenue")} value={`${summary.productSalesRevenue.toFixed(2)} ${t("common.currency")}`} />
            <SummaryCard icon={Wallet} label={t("dashboard.amountPaidToFarmers")} value={`${summary.amountPaidToFarmers.toFixed(2)} ${t("common.currency")}`} />
            <SummaryCard icon={TrendingUp} label={t("dashboard.grossProfitOnGoods")} value={`${summary.grossProfitOnGoods.toFixed(2)} ${t("common.currency")}`} />
            <SummaryCard
              icon={Percent}
              label={summary.commissionRatePercent != null ? `${t("dashboard.totalCommission")} (${summary.commissionRatePercent}%)` : t("dashboard.totalCommission")}
              value={`${summary.totalCommission.toFixed(2)} ${t("common.currency")}`}
            />
            <SummaryCard icon={Coins} label={t("dashboard.hotelAgentBonus")} value={`${summary.hotelAgentBonusTotal.toFixed(2)} ${t("common.currency")}`} />
            <SummaryCard icon={Coins} label={t("dashboard.farmerAgentBonus")} value={`${summary.farmerAgentBonusTotal.toFixed(2)} ${t("common.currency")}`} />
            <SummaryCard icon={Wallet} label={t("dashboard.netProfit")} value={`${summary.netProfit.toFixed(2)} ${t("common.currency")}`} highlight />
            <SummaryCard icon={Truck} label={t("dashboard.driverTripCost")} value={`${summary.driverTripCost.toFixed(2)} ${t("common.currency")}`} />
          </div>
          <p className="mt-2 font-mono text-[11px] text-ink-300">{t("dashboard.netProfitFormula")}</p>

          {/* Registered agents roster */}
          <div className="mt-6 card overflow-x-auto">
            <div className="flex items-center gap-2 border-b border-evergreen-100 p-4">
              <Users className="h-4 w-4 text-evergreen-700" />
              <h3 className="text-sm font-semibold text-evergreen-900">{t("agentsManagement.registeredAgents")}</h3>
            </div>
            <table className="table-shell">
              <thead>
                <tr>
                  <th>{t("common.id")}</th>
                  <th>{t("common.name")}</th>
                  <th>{t("users.role")}</th>
                  <th>{t("customers.phone")}</th>
                  <th>{t("farmers.location")}</th>
                  <th>{t("common.status")}</th>
                  <th>{t("agentsManagement.todayOrdersKg")}</th>
                  <th>{t("agentsManagement.unpaidCommission")}</th>
                  <th>{t("agentsManagement.totalCommissionEarned")}</th>
                </tr>
              </thead>
              <tbody>
                {summary.agents.map((a) => (
                  <tr key={a.id}>
                    <td className="font-mono text-xs">{a.code ?? "-"}</td>
                    <td className="font-medium">{a.fullName}</td>
                    <td>{a.role}</td>
                    <td>{a.phone ?? "-"}</td>
                    <td>{a.location ?? "-"}</td>
                    <td>
                      <span className={`inline-flex items-center gap-1.5 ${a.isOnline ? "text-evergreen-700" : "text-ink-300"}`}>
                        <span className={`h-2 w-2 rounded-full ${a.isOnline ? "bg-evergreen-500" : "bg-ink-300"}`} />
                        {a.isOnline ? t("agentsManagement.online") : t("agentsManagement.offline")}
                        {!a.isActive && <span className="ml-1 text-clay-600">({t("common.inactive")})</span>}
                      </span>
                    </td>
                    <td>{a.todayOrderedKg.toFixed(1)} kg</td>
                    <td className="font-medium text-evergreen-900">{a.unpaidCommission.toFixed(2)} {t("common.currency")}</td>
                    <td className="text-ink-600">{a.totalCommissionEarned.toFixed(2)} {t("common.currency")}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Same charts as the dashboard, kept in sync visually */}
          <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-2">
            <div className="card p-5">
              <h3 className="mb-1 text-sm font-semibold text-evergreen-900">{t("dashboard.productDistribution")}</h3>
              <p className="mb-3 text-xs text-ink-300">{t("dashboard.productDistributionHint")}</p>
              {topProducts && topProducts.length > 0 && (
                <ResponsiveContainer width="100%" height={260}>
                  <PieChart>
                    <defs>
                      {topProducts.map((_, i) => (
                        <linearGradient key={i} id={`am-slice-${i}`} x1="0" y1="0" x2="0" y2="1">
                          <stop offset="0%" stopColor={CHART_COLORS[i % CHART_COLORS.length]} stopOpacity={1} />
                          <stop offset="100%" stopColor={CHART_COLORS[i % CHART_COLORS.length]} stopOpacity={0.75} />
                        </linearGradient>
                      ))}
                    </defs>
                    <Pie
                      data={topProducts}
                      dataKey="quantitySold"
                      nameKey="productName"
                      cx="50%"
                      cy="46%"
                      innerRadius={52}
                      outerRadius={84}
                      paddingAngle={2}
                      cornerRadius={6}
                    >
                      {topProducts.map((_, i) => <Cell key={i} fill={`url(#am-slice-${i})`} stroke="white" strokeWidth={2} />)}
                    </Pie>
                    <Tooltip content={<BrandTooltip valueFormatter={(v) => `${v.toFixed(0)} kg`} />} />
                    <Legend verticalAlign="bottom" iconType="circle" iconSize={8} wrapperStyle={{ fontSize: 11, paddingTop: 10 }} />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </div>
            <div className="card p-5">
              <h3 className="mb-1 text-sm font-semibold text-evergreen-900">{t("dashboard.revenueTrend")}</h3>
              <p className="mb-3 text-xs text-ink-300">{t("dashboard.revenueTrendHint")}</p>
              {revenueByPeriod && revenueByPeriod.length > 0 && (
                <ResponsiveContainer width="100%" height={260}>
                  <BarChart data={revenueByPeriod}>
                    <defs>
                      <linearGradient id="am-revenueBarFill" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stopColor="#3C6B54" stopOpacity={1} />
                        <stop offset="100%" stopColor="#264A3A" stopOpacity={0.85} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#E4EDE7" vertical={false} />
                    <XAxis dataKey="period" tick={{ fontSize: 11 }} axisLine={{ stroke: "#D6E3D9" }} tickLine={false} />
                    <YAxis tick={{ fontSize: 11 }} axisLine={false} tickLine={false} />
                    <Tooltip
                      cursor={{ fill: "#264A3A", fillOpacity: 0.06 }}
                      content={<BrandTooltip valueFormatter={(v) => `${v.toFixed(2)} ${t("common.currency")}`} />}
                    />
                    <Bar dataKey="revenue" fill="url(#am-revenueBarFill)" radius={[8, 8, 0, 0]} maxBarSize={56} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </div>
          </div>

          {topCustomers && topCustomers.length > 0 && (
            <div className="mt-4 card p-5">
              <h3 className="mb-3 text-sm font-semibold text-evergreen-900">{t("dashboard.topHotels")}</h3>
              <ol className="space-y-2">
                {topCustomers.map((c, i) => (
                  <li key={c.customerId} className="flex items-center justify-between rounded-md border border-evergreen-100 px-3 py-2">
                    <span className="flex items-center gap-2">
                      <span className="flex h-6 w-6 items-center justify-center rounded-full bg-evergreen-600 text-xs font-semibold text-white">{i + 1}</span>
                      <span className="font-medium text-evergreen-900">{c.customerName}</span>
                    </span>
                    <span className="text-sm text-ink-600">{c.quantityKg.toFixed(0)} kg &middot; {c.revenue.toFixed(2)} {t("common.currency")}</span>
                  </li>
                ))}
              </ol>
            </div>
          )}
        </>
      )}
    </div>
  );
}

function SummaryCard({
  icon: Icon, label, value, highlight
}: { icon: React.ElementType; label: string; value: string | number; highlight?: boolean }) {
  return (
    <div className={`card p-4 ${highlight ? "border-wheat-400" : ""}`}>
      <div className="flex items-center gap-2 text-evergreen-700">
        <Icon className="h-4 w-4" strokeWidth={1.75} />
        <p className="text-xs">{label}</p>
      </div>
      <p className="mt-2 text-lg font-semibold text-evergreen-900">{value}</p>
    </div>
  );
}
