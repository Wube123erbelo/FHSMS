import { useState } from "react";
import {
  ResponsiveContainer, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,
  BarChart, Bar, PieChart, Pie, Cell, Legend
} from "recharts";
import { Download } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import { LoadingState, ErrorState } from "../../components/States";
import { BrandTooltip } from "../../components/charts/BrandTooltip";
import { useFetch } from "../../hooks/useFetch";
import { apiClient } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { SalesSummaryDto, RevenuePeriodDto, TopProductDto, CommissionSummaryDto } from "../../api/types";

const PIE_COLORS = ["#264A3A", "#C9A227", "#A64B3B", "#3C6B54", "#9C7D1D"];

function toDateInput(d: Date) {
  return d.toISOString().slice(0, 10);
}

export default function ReportsPage() {
  const { t } = useTranslation();
  const [from, setFrom] = useState(toDateInput(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)));
  const [to, setTo] = useState(toDateInput(new Date()));
  const [appliedFrom, setAppliedFrom] = useState(from);
  const [appliedTo, setAppliedTo] = useState(to);

  const qs = `from=${appliedFrom}&to=${appliedTo}`;
  const { data: summary, loading: loadingSummary, error: summaryError } =
    useFetch<SalesSummaryDto>(`/reports/sales-summary?${qs}`, [qs]);
  const { data: revenue, loading: loadingRevenue } =
    useFetch<RevenuePeriodDto[]>(`/reports/revenue-by-period?${qs}&groupBy=Day`, [qs]);
  const { data: topProducts, loading: loadingTop } =
    useFetch<TopProductDto[]>(`/reports/top-products?${qs}&top=8`, [qs]);
  const { data: commissionSummary, loading: loadingCommission } =
    useFetch<CommissionSummaryDto[]>(`/reports/commission-summary?${qs}`, [qs]);

  function apply() {
    setAppliedFrom(from);
    setAppliedTo(to);
  }

  async function downloadCsv(endpoint: string, filename: string) {
    const response = await apiClient.get(endpoint, { responseType: "blob" });
    const url = URL.createObjectURL(response.data as Blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
  }

  return (
    <div>
      <PageHeader title={t("reports.title")} description={t("reports.description")} />

      <div className="card mb-6 flex flex-wrap items-end gap-4 p-5">
        <div>
          <label className="label">{t("reports.from")}</label>
          <input type="date" className="input" value={from} onChange={(e) => setFrom(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("reports.to")}</label>
          <input type="date" className="input" value={to} onChange={(e) => setTo(e.target.value)} />
        </div>
        <button className="btn-primary" onClick={apply}>{t("reports.apply")}</button>
        <button className="btn-secondary ml-auto" onClick={() => downloadCsv("/orders/export/csv", "orders.csv")}>
          <Download className="h-4 w-4" /> {t("reports.exportCsv")}
        </button>
      </div>

      {summaryError && <ErrorState message={summaryError} />}
      {loadingSummary && <LoadingState label={t("common.loading")} />}

      {summary && (
        <div className="mb-6 grid grid-cols-2 gap-4 sm:grid-cols-4">
          <Stat label={t("reports.totalRevenue")} value={`${summary.totalRevenue.toFixed(2)} ${t("common.currency")}`} />
          <Stat label={t("reports.totalTax")} value={`${summary.totalTaxCollected.toFixed(2)} ${t("common.currency")}`} />
          <Stat label={t("reports.invoiceCount")} value={summary.invoiceCount.toString()} />
          <Stat label={t("reports.avgInvoice")} value={`${summary.averageInvoiceValue.toFixed(2)} ${t("common.currency")}`} />
          <Stat label={t("reports.outstanding")} value={`${summary.totalOutstanding.toFixed(2)} ${t("common.currency")}`} highlight />
          <Stat label={t("reports.orderCount")} value={summary.orderCount.toString()} />
          <Stat label={t("reports.cancelledOrders")} value={summary.cancelledOrderCount.toString()} highlight={summary.cancelledOrderCount > 0} />
        </div>
      )}

      {/* Profit formula (also returned verbatim as summary.netProfitFormula):
            GrossProfitOnGoods = ProductSalesRevenue - AmountPaidToFarmers
            NetProfit = GrossProfitOnGoods + TotalCommission - HotelAgentBonus - FarmerAgentBonus - DriverTripCost
          Tax plays no part in profit - it's collected on behalf of the
          government and passed through, never money the company keeps.
          DriverTripCost only counts completed (Delivered) trips - a real
          company cost, since no delivery-fee line item exists on any
          invoice to bill this through to the hotel.
          TotalRevenue above (Invoice.GrandTotal summed) is what hotels were
          billed in total, shown as context - it is NOT the same as profit. */}
      {summary && (
        <div className="mb-6 grid grid-cols-2 gap-4 sm:grid-cols-4">
          <Stat label={t("dashboard.productSalesRevenue")} value={`${summary.productSalesRevenue.toFixed(2)} ${t("common.currency")}`} />
          <Stat label={t("dashboard.amountPaidToFarmers")} value={`${summary.amountPaidToFarmers.toFixed(2)} ${t("common.currency")}`} />
          <Stat label={t("dashboard.grossProfitOnGoods")} value={`${summary.grossProfitOnGoods.toFixed(2)} ${t("common.currency")}`} />
          <Stat
            label={summary.commissionRatePercent != null ? `${t("dashboard.totalCommission")} (${summary.commissionRatePercent}%)` : t("dashboard.totalCommission")}
            value={`${summary.totalCommission.toFixed(2)} ${t("common.currency")}`}
          />
          <Stat label={t("dashboard.hotelAgentBonus")} value={`${summary.hotelAgentBonusTotal.toFixed(2)} ${t("common.currency")}`} />
          <Stat label={t("dashboard.farmerAgentBonus")} value={`${summary.farmerAgentBonusTotal.toFixed(2)} ${t("common.currency")}`} />
          <Stat label={t("dashboard.netProfit")} value={`${summary.netProfit.toFixed(2)} ${t("common.currency")}`} highlight />
          <Stat label={t("dashboard.driverTripCost")} value={`${summary.driverTripCost.toFixed(2)} ${t("common.currency")}`} />
        </div>
      )}
      {summary && <p className="mb-6 -mt-4 font-mono text-[11px] text-ink-300">{t("dashboard.netProfitFormula")}</p>}

      <div className="mb-6 card p-5">
        <h3 className="mb-4 text-sm font-semibold text-evergreen-900">{t("reports.revenueOverTime")}</h3>
        {loadingRevenue && <LoadingState label={t("common.loading")} />}
        {revenue && revenue.length > 0 && (
          <ResponsiveContainer width="100%" height={280}>
            <AreaChart data={revenue}>
              <defs>
                <linearGradient id="revenueArea" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#264A3A" stopOpacity={0.35} />
                  <stop offset="100%" stopColor="#264A3A" stopOpacity={0.02} />
                </linearGradient>
                <linearGradient id="taxArea" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#C9A227" stopOpacity={0.35} />
                  <stop offset="100%" stopColor="#C9A227" stopOpacity={0.02} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#E8DFC8" vertical={false} />
              <XAxis dataKey="period" tick={{ fontSize: 11 }} axisLine={{ stroke: "#E8DFC8" }} tickLine={false} />
              <YAxis tick={{ fontSize: 11 }} axisLine={false} tickLine={false} />
              <Tooltip content={<BrandTooltip valueFormatter={(v) => `${v.toFixed(2)} ${t("common.currency")}`} />} />
              <Area type="monotone" dataKey="revenue" stroke="#264A3A" strokeWidth={2.5} fill="url(#revenueArea)" name={t("reports.revenue")} activeDot={{ r: 5 }} />
              <Area type="monotone" dataKey="taxCollected" stroke="#C9A227" strokeWidth={2.5} fill="url(#taxArea)" name={t("reports.totalTax")} activeDot={{ r: 5 }} />
            </AreaChart>
          </ResponsiveContainer>
        )}
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-evergreen-900">{t("reports.topProducts")}</h3>
          {loadingTop && <LoadingState label={t("common.loading")} />}
          {topProducts && topProducts.length > 0 && (
            <ResponsiveContainer width="100%" height={280}>
              <BarChart data={topProducts} layout="vertical" margin={{ left: 24 }}>
                <defs>
                  <linearGradient id="topProductsBar" x1="0" y1="0" x2="1" y2="0">
                    <stop offset="0%" stopColor="#264A3A" stopOpacity={0.85} />
                    <stop offset="100%" stopColor="#3C6B54" stopOpacity={1} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#E8DFC8" horizontal={false} />
                <XAxis type="number" tick={{ fontSize: 11 }} axisLine={false} tickLine={false} />
                <YAxis type="category" dataKey="productName" tick={{ fontSize: 11 }} width={100} axisLine={false} tickLine={false} />
                <Tooltip cursor={{ fill: "#264A3A", fillOpacity: 0.06 }} content={<BrandTooltip valueFormatter={(v) => v.toFixed(2)} />} />
                <Bar dataKey="revenue" fill="url(#topProductsBar)" name={t("reports.revenue")} radius={[0, 8, 8, 0]} maxBarSize={28} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </div>

        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-evergreen-900">{t("reports.commissionByRole")}</h3>
          {loadingCommission && <LoadingState label={t("common.loading")} />}
          {commissionSummary && commissionSummary.length > 0 && (
            <ResponsiveContainer width="100%" height={280}>
              <PieChart>
                <defs>
                  {commissionSummary.map((_, i) => (
                    <linearGradient key={i} id={`role-slice-${i}`} x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor={PIE_COLORS[i % PIE_COLORS.length]} stopOpacity={1} />
                      <stop offset="100%" stopColor={PIE_COLORS[i % PIE_COLORS.length]} stopOpacity={0.75} />
                    </linearGradient>
                  ))}
                </defs>
                <Pie
                  data={commissionSummary}
                  dataKey="totalCommission"
                  nameKey="agentType"
                  cx="50%"
                  cy="46%"
                  innerRadius={58}
                  outerRadius={92}
                  paddingAngle={2}
                  cornerRadius={6}
                >
                  {commissionSummary.map((_, i) => (
                    <Cell key={i} fill={`url(#role-slice-${i})`} stroke="white" strokeWidth={2} />
                  ))}
                </Pie>
                <Tooltip content={<BrandTooltip valueFormatter={(v) => v.toFixed(2)} />} />
                <Legend verticalAlign="bottom" iconType="circle" iconSize={8} wrapperStyle={{ fontSize: 11, paddingTop: 12 }} />
              </PieChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>
    </div>
  );
}

function Stat({ label, value, highlight }: { label: string; value: string; highlight?: boolean }) {
  return (
    <div className="card p-4">
      <p className="text-[11px] uppercase tracking-wide text-ink-300">{label}</p>
      <p className={`mt-1 text-lg font-semibold ${highlight ? "text-clay-600" : "text-ink-900"}`}>{value}</p>
    </div>
  );
}
