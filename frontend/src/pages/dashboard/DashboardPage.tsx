import { Link, useNavigate } from "react-router-dom";
import {
  Percent, Package, ShoppingCart, Receipt, TrendingUp, AlertTriangle, Wallet, Coins, Truck
} from "lucide-react";
import {
  PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend
} from "recharts";
import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { BrandTooltip } from "../../components/charts/BrandTooltip";
import { LoadingState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type {
  ProductDto, TaxConfigurationDto, SalesSummaryDto, TopProductDto,
  RevenuePeriodDto, TopCustomerDto, CommissionDto, CommissionRuleDto, OrderDto
} from "../../api/types";

// Chart colors drawn from the FHSMS palette (tailwind.config) - evergreen +
// wheat + clay - rather than a default chart-library rainbow, so dashboard
// visuals stay on-brand instead of looking bolted on.
const CHART_COLORS = ["#264A3A", "#C9A227", "#A64B3B", "#3C6B54", "#9C7D1D", "#1C3B32"];

export default function DashboardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const isAgent = role === "HotelAgent" || role === "FarmerAgent";

  const { data: products } = useFetch<ProductDto[]>("/products");
  const { data: taxConfigs } = useFetch<TaxConfigurationDto[]>(isAdmin ? "/tax" : null);
  const { data: summary } = useFetch<SalesSummaryDto>(isAdmin ? "/reports/sales-summary" : null);
  const { data: topProducts } = useFetch<TopProductDto[]>(isAdmin ? "/reports/top-products?top=6" : null);
  const { data: revenueByPeriod } = useFetch<RevenuePeriodDto[]>(isAdmin ? "/reports/revenue-by-period?groupBy=Month" : null);
  const { data: topCustomers } = useFetch<TopCustomerDto[]>(isAdmin ? "/reports/top-customers?top=5" : null);
  // Every order, most recent first (see GetOrdersQueryHandler) - sliced down
  // to a handful below for an at-a-glance "where's everything right now"
  // widget, so the admin doesn't have to leave the dashboard to see it.
  const { data: allOrders } = useFetch<OrderDto[]>(isAdmin ? "/orders" : null);
  const recentOrders = allOrders?.slice(0, 6);

  const vat = taxConfigs?.find((tc) => tc.taxType === "Vat");
  const vatValue = vat
    ? vat.isEnabled ? `${t("dashboard.vatOn")} - ${vat.currentRate ?? "-"}%` : t("dashboard.vatOff")
    : t("dashboard.vatNotConfigured");

  return (
    <div>
      <PageHeader eyebrow={t("dashboard.eyebrow")} title={t("dashboard.title")} description={t("dashboard.description")} />

      {isAgent && <AgentQuickStats />}
      {role === "HotelAgent" && <MyOrdersTracking />}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <SummaryCard icon={Package} label={t("dashboard.activeProducts")} value={products?.length ?? "-"} to="/products" />
        <SummaryCard icon={ShoppingCart} label={t("dashboard.orders")} value={summary ? summary.orderCount : t("common.view")} to="/orders" />
        <SummaryCard icon={Receipt} label={t("dashboard.invoices")} value={summary ? summary.invoiceCount : t("common.view")} to="/invoices" />
        <SummaryCard icon={Percent} label={t("dashboard.vatStatus")} value={vatValue} to="/tax-settings" highlight={vat?.isEnabled} />
      </div>

      {isAdmin && summary && (
        <div className="mt-4 grid grid-cols-2 gap-4 sm:grid-cols-3">
          <SummaryCard icon={TrendingUp} label={t("reports.totalRevenue")} value={`${summary.totalRevenue.toFixed(2)} ${t("common.currency")}`} to="/reports" />
          <SummaryCard icon={TrendingUp} label={t("reports.totalTax")} value={`${summary.totalTaxCollected.toFixed(2)} ${t("common.currency")}`} to="/reports" />
          <SummaryCard
            icon={AlertTriangle}
            label={t("reports.outstanding")}
            value={`${summary.totalOutstanding.toFixed(2)} ${t("common.currency")}`}
            to="/reports"
            highlight={summary.totalOutstanding > 0}
          />
        </div>
      )}

      {/* Profit formula (see SalesSummaryDto on the backend for the full
          derivation): tax is pass-through money the company never keeps, so
          it plays no part here - ProductSalesRevenue and AmountPaidToFarmers
          come straight from Invoice.Subtotal and approved FarmerInvoice
          totals, TotalCommission is the exact amount frozen on every invoice
          at issue time, and HotelAgentBonus/FarmerAgentBonus are shown split
          per admin's request instead of only as one combined AgentBonus figure.
          DriverTripCost is what a driver is paid for a completed trip - a
          real, uncompensated company cost since no delivery-fee line item
          exists on any invoice - so it's netted out too, the same as the
          agent bonuses.
            GrossProfitOnGoods = ProductSalesRevenue - AmountPaidToFarmers
            NetProfit = GrossProfitOnGoods + TotalCommission - HotelAgentBonus - FarmerAgentBonus - DriverTripCost */}
      {isAdmin && summary && (
        <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <SummaryCard
            icon={TrendingUp}
            label={t("dashboard.productSalesRevenue")}
            value={`${summary.productSalesRevenue.toFixed(2)} ${t("common.currency")}`}
            to="/reports"
          />
          <SummaryCard
            icon={Wallet}
            label={t("dashboard.amountPaidToFarmers")}
            value={`${summary.amountPaidToFarmers.toFixed(2)} ${t("common.currency")}`}
            to="/farmer-invoices"
          />
          <SummaryCard
            icon={TrendingUp}
            label={t("dashboard.grossProfitOnGoods")}
            value={`${summary.grossProfitOnGoods.toFixed(2)} ${t("common.currency")}`}
            to="/reports"
          />
          <SummaryCard
            icon={Percent}
            label={
              summary.commissionRatePercent != null
                ? `${t("dashboard.totalCommission")} (${summary.commissionRatePercent}%)`
                : t("dashboard.totalCommission")
            }
            value={`${summary.totalCommission.toFixed(2)} ${t("common.currency")}`}
            to="/commission-settings"
          />
          <SummaryCard
            icon={Coins}
            label={t("dashboard.hotelAgentBonus")}
            value={`${summary.hotelAgentBonusTotal.toFixed(2)} ${t("common.currency")}`}
            to="/commissions"
          />
          <SummaryCard
            icon={Coins}
            label={t("dashboard.farmerAgentBonus")}
            value={`${summary.farmerAgentBonusTotal.toFixed(2)} ${t("common.currency")}`}
            to="/commissions"
          />
          <SummaryCard
            icon={Wallet}
            label={t("dashboard.netProfit")}
            value={`${summary.netProfit.toFixed(2)} ${t("common.currency")}`}
            to="/reports"
            highlight
          />
          <SummaryCard
            icon={Truck}
            label={t("dashboard.driverTripCost")}
            value={`${summary.driverTripCost.toFixed(2)} ${t("common.currency")}`}
            to="/deliveries"
          />
        </div>
      )}
      {isAdmin && summary && (
        <p className="mt-2 font-mono text-[11px] text-ink-300">{t("dashboard.netProfitFormula")}</p>
      )}

      {isAdmin && (
        <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-2">
          <div className="card p-5">
            <h3 className="mb-1 text-sm font-semibold text-evergreen-900">{t("dashboard.productDistribution")}</h3>
            <p className="mb-3 text-xs text-ink-300">{t("dashboard.productDistributionHint")}</p>
            {!topProducts ? (
              <LoadingState label={t("common.loading")} />
            ) : topProducts.length === 0 ? (
              <p className="py-8 text-center text-sm text-ink-300">{t("dashboard.noData")}</p>
            ) : (
              <ResponsiveContainer width="100%" height={280}>
                <PieChart>
                  <defs>
                    {topProducts.map((_, i) => (
                      <linearGradient key={i} id={`slice-${i}`} x1="0" y1="0" x2="0" y2="1">
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
                    innerRadius={58}
                    outerRadius={92}
                    paddingAngle={2}
                    cornerRadius={6}
                  >
                    {topProducts.map((_, i) => <Cell key={i} fill={`url(#slice-${i})`} stroke="white" strokeWidth={2} />)}
                  </Pie>
                  <Tooltip content={<BrandTooltip valueFormatter={(v) => `${v.toFixed(0)} kg`} />} />
                  <Legend
                    verticalAlign="bottom"
                    iconType="circle"
                    iconSize={8}
                    wrapperStyle={{ fontSize: 11, paddingTop: 12 }}
                  />
                </PieChart>
              </ResponsiveContainer>
            )}
          </div>

          <div className="card p-5">
            <h3 className="mb-1 text-sm font-semibold text-evergreen-900">{t("dashboard.revenueTrend")}</h3>
            <p className="mb-3 text-xs text-ink-300">{t("dashboard.revenueTrendHint")}</p>
            {!revenueByPeriod ? (
              <LoadingState label={t("common.loading")} />
            ) : revenueByPeriod.length === 0 ? (
              <p className="py-8 text-center text-sm text-ink-300">{t("dashboard.noData")}</p>
            ) : (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={revenueByPeriod}>
                  <defs>
                    <linearGradient id="revenueBarFill" x1="0" y1="0" x2="0" y2="1">
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
                  <Bar dataKey="revenue" fill="url(#revenueBarFill)" radius={[8, 8, 0, 0]} maxBarSize={56} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>
      )}

      {isAdmin && (
        <div className="mt-4 card p-5">
          <div className="mb-3 flex items-center justify-between">
            <div>
              <h3 className="text-sm font-semibold text-evergreen-900">{t("dashboard.orderTracking")}</h3>
              <p className="text-xs text-ink-300">{t("dashboard.orderTrackingHint")}</p>
            </div>
            <Link to="/orders" className="text-xs font-medium text-evergreen-700 underline underline-offset-2">
              {t("dashboard.viewAllOrders")}
            </Link>
          </div>
          {!recentOrders ? (
            <LoadingState label={t("common.loading")} />
          ) : recentOrders.length === 0 ? (
            <p className="py-8 text-center text-sm text-ink-300">{t("dashboard.noData")}</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="table-shell">
                <thead>
                  <tr>
                    <th>{t("orders.title")} #</th>
                    <th>{t("customers.title")}</th>
                    <th>{t("common.status")}</th>
                    <th>{t("common.date")}</th>
                    <th>{t("orders.subtotal")}</th>
                  </tr>
                </thead>
                <tbody>
                  {recentOrders.map((o) => (
                    <tr key={o.id} className="cursor-pointer hover:bg-evergreen-50/40" onClick={() => navigate(`/orders/${o.id}`)}>
                      <td className="font-medium text-evergreen-900">{o.orderNumber}</td>
                      <td>{o.customerName ?? "-"}</td>
                      <td><StatusBadge status={o.status} /></td>
                      <td className="text-ink-300">{new Date(o.orderDate).toLocaleDateString()}</td>
                      <td>{o.subtotal.toFixed(2)} {t("common.currency")}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {isAdmin && (
        <div className="mt-4 card p-5">
          <h3 className="mb-1 text-sm font-semibold text-evergreen-900">{t("dashboard.topHotels")}</h3>
          <p className="mb-3 text-xs text-ink-300">{t("dashboard.topHotelsHint")}</p>
          {!topCustomers ? (
            <LoadingState label={t("common.loading")} />
          ) : topCustomers.length === 0 ? (
            <p className="py-8 text-center text-sm text-ink-300">{t("dashboard.noData")}</p>
          ) : (
            <ol className="space-y-2">
              {topCustomers.map((c, i) => (
                <li key={c.customerId} className="flex items-center justify-between rounded-md border border-evergreen-100 px-3 py-2">
                  <span className="flex items-center gap-2">
                    <span className="flex h-6 w-6 items-center justify-center rounded-full bg-evergreen-600 text-xs font-semibold text-white">
                      {i + 1}
                    </span>
                    <span className="font-medium text-evergreen-900">{c.customerName}</span>
                  </span>
                  <span className="text-sm text-ink-600">
                    {c.quantityKg.toFixed(0)} kg &middot; {c.revenue.toFixed(2)} {t("common.currency")}
                  </span>
                </li>
              ))}
            </ol>
          )}
        </div>
      )}

      <div className="mt-8 card p-6">
        <h2 className="mb-2 text-base font-semibold text-evergreen-900">{t("dashboard.howTaxFlowsTitle")}</h2>
        <p className="max-w-3xl text-sm text-ink-600">
          {t("dashboard.howTaxFlowsBody")}{" "}
          <Link to="/tax-settings" className="text-evergreen-700 underline underline-offset-2">
            {t("dashboard.taxSettingsLink")}
          </Link>
        </p>
      </div>
    </div>
  );
}

/**
 * Today's earnings + incentive-rate card for a logged-in Hotel/Farmer agent -
 * the quick-stats block from the mobile mockup, ported to the responsive web
 * dashboard so both surfaces stay in sync off the same API.
 */
function AgentQuickStats() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const { data: commissions } = useFetch<CommissionDto[]>("/commissions/mine");
  const { data: rules } = useFetch<CommissionRuleDto[]>("/commissions/rules");

  if (!commissions) return null;

  const today = new Date().toDateString();
  const todaysCommissions = commissions.filter((c) => new Date(c.createdAt).toDateString() === today);
  const todaysTotal = todaysCommissions.reduce((sum, c) => sum + c.commissionAmount, 0);
  const monthTotal = commissions
    .filter((c) => {
      const d = new Date(c.createdAt);
      const now = new Date();
      return d.getMonth() === now.getMonth() && d.getFullYear() === now.getFullYear();
    })
    .reduce((sum, c) => sum + c.commissionAmount, 0);

  // The actual configured rate, not a hardcoded string - an admin can
  // change this at any time on the Commissions page, and this label must
  // always reflect whatever is live right now.
  const myRule = rules?.find((r) => r.agentType === role);
  let rateLabel = t("dashboard.rateNotConfigured");
  if (myRule) {
    rateLabel = myRule.basis === "PercentageOfInvoice"
      ? t("dashboard.rateIsPercentage").replace("{pct}", String(myRule.percentage ?? 0))
      : t("dashboard.rateIsFlat").replace("{amount}", (myRule.flatRateAmount ?? 0).toFixed(2));
  }

  return (
    <div className="mb-4 grid grid-cols-1 gap-4 rounded-lg border border-evergreen-100 bg-evergreen-50/50 p-4 sm:grid-cols-3">
      <div>
        <p className="text-xs uppercase tracking-wide text-evergreen-700">{t("dashboard.todaysEarnings")}</p>
        <p className="mt-1 text-2xl font-semibold text-evergreen-900">{todaysTotal.toFixed(2)} {t("common.currency")}</p>
        <p className="mt-0.5 text-xs text-ink-300">{rateLabel}</p>
      </div>
      <div>
        <p className="text-xs uppercase tracking-wide text-evergreen-700">{t("dashboard.thisMonth")}</p>
        <p className="mt-1 text-2xl font-semibold text-evergreen-900">{monthTotal.toFixed(2)} {t("common.currency")}</p>
        {commissions.length === 0 && (
          <p className="mt-0.5 text-xs text-ink-300">{t("dashboard.noCommissionYetHint")}</p>
        )}
      </div>
      <div className="flex items-center gap-2 sm:justify-end">
        <Wallet className="h-8 w-8 text-evergreen-500" strokeWidth={1.5} />
        <Link to="/commissions" className="text-sm font-medium text-evergreen-700 underline underline-offset-2">
          {t("dashboard.viewAllEarnings")}
        </Link>
      </div>
    </div>
  );
}

/**
 * A hotel agent's own orders with both their workflow status and their
 * delivery/truck status, right on login - the same "where's everything
 * right now" view the admin dashboard has, scoped automatically to just
 * this agent's own orders (GetOrdersQueryHandler filters by AgentUserId
 * for the HotelAgent role, so this is a plain call to /orders - no
 * separate "my orders" endpoint needed).
 */
function MyOrdersTracking() {
  const { t } = useTranslation();
  const { data: orders, loading } = useFetch<OrderDto[]>("/orders");
  const recent = orders?.slice(0, 6);

  return (
    <div className="mb-4 card p-5">
      <div className="mb-3 flex items-center justify-between">
        <div>
          <h3 className="text-sm font-semibold text-evergreen-900">{t("dashboard.myOrderTracking")}</h3>
          <p className="text-xs text-ink-300">{t("dashboard.myOrderTrackingHint")}</p>
        </div>
        <Link to="/orders" className="text-xs font-medium text-evergreen-700 underline underline-offset-2">
          {t("dashboard.viewAllOrders")}
        </Link>
      </div>
      {loading && <LoadingState label={t("common.loading")} />}
      {!loading && recent && recent.length === 0 && (
        <p className="py-6 text-center text-sm text-ink-300">{t("dashboard.noOrdersYetHint")}</p>
      )}
      {recent && recent.length > 0 && (
        <div className="space-y-2">
          {recent.map((o) => (
            <Link
              key={o.id}
              to={`/orders/${o.id}`}
              className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-evergreen-100 px-3 py-2 text-sm hover:bg-evergreen-50/40"
            >
              <div className="min-w-0">
                <p className="font-medium text-evergreen-900">{o.orderNumber}</p>
                <p className="text-xs text-ink-300">{o.customerName ?? "-"} &middot; {new Date(o.orderDate).toLocaleDateString()}</p>
              </div>
              <div className="flex items-center gap-2">
                <StatusBadge status={o.status} />
                {o.deliveryStatus ? (
                  <span className="flex items-center gap-1">
                    <Truck className="h-3 w-3 text-evergreen-500" />
                    <StatusBadge status={o.deliveryStatus} />
                  </span>
                ) : (
                  <span className="flex items-center gap-1 rounded-full bg-ink-900/5 px-2 py-0.5 text-xs text-ink-300">
                    <Truck className="h-3 w-3" />
                    {t("dashboard.notDispatchedYet")}
                  </span>
                )}
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

function SummaryCard({
  icon: Icon, label, value, to, highlight
}: { icon: React.ElementType; label: string; value: string | number; to: string; highlight?: boolean }) {
  return (
    <Link
      to={to}
      className={`card flex items-center gap-3 p-4 transition hover:shadow-md ${highlight ? "border-wheat-400" : ""}`}
    >
      <div className="flex h-10 w-10 items-center justify-center rounded-md bg-evergreen-50 text-evergreen-700">
        <Icon className="h-5 w-5" strokeWidth={1.75} />
      </div>
      <div className="min-w-0">
        <p className="truncate text-xs text-ink-300">{label}</p>
        <p className="truncate text-base font-semibold text-evergreen-900">{value}</p>
      </div>
    </Link>
  );
}
