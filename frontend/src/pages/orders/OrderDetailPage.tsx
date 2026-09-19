import { useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { Receipt, ArrowLeft, Repeat } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation, type TranslationKey } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { OrderDto, OrderStatus, DeliveryDto } from "../../api/types";

/**
 * Buttons offered for each status - mirrors Order's domain transition methods
 * exactly. All lifecycle actions except Cancel are admin-only server-side
 * (see OrdersController) - an agent places an order, but operations
 * confirms/prepares/ships/completes it, and billing generates the invoice.
 * That's a role-based/audit separation, not a UI restriction: this
 * `adminOnly` flag mirrors the actual server-side authorization so agents
 * don't see buttons that would just 403.
 */
const ACTIONS_BY_STATUS: Partial<Record<OrderStatus, { action: string; labelKey: TranslationKey; needsReason?: boolean; adminOnly?: boolean }[]>> = {
  Pending: [
    { action: "confirm", labelKey: "orderStatus.confirm", adminOnly: true },
    { action: "reject", labelKey: "orderStatus.reject", needsReason: true, adminOnly: true },
    { action: "cancel", labelKey: "invoices.cancelOrder" }
  ],
  Confirmed: [
    { action: "prepare", labelKey: "orderStatus.prepare", adminOnly: true },
    { action: "cancel", labelKey: "invoices.cancelOrder" }
  ],
  Preparing: [
    { action: "ship", labelKey: "orderStatus.ship", adminOnly: true },
    { action: "cancel", labelKey: "invoices.cancelOrder" }
  ],
  Shipped: [
    { action: "return", labelKey: "orderStatus.return", needsReason: true, adminOnly: true }
  ],
  Delivered: [
    { action: "complete", labelKey: "orderStatus.complete", adminOnly: true },
    { action: "return", labelKey: "orderStatus.return", needsReason: true, adminOnly: true }
  ]
};

export default function OrderDetailPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const { orderId } = useParams<{ orderId: string }>();
  const navigate = useNavigate();
  const { data: order, loading, error, reload } = useFetch<OrderDto>(orderId ? `/orders/${orderId}` : null, [orderId]);
  const { data: delivery, reload: reloadDelivery } = useFetch<DeliveryDto | null>(orderId ? `/deliveries/by-order/${orderId}` : null, [orderId]);
  const [generating, setGenerating] = useState(false);
  const [genError, setGenError] = useState<string | null>(null);
  const [busyAction, setBusyAction] = useState<string | null>(null);
  const [confirmingReceipt, setConfirmingReceipt] = useState(false);
  const [repeating, setRepeating] = useState(false);
  const [repeatDate, setRepeatDate] = useState("");
  const [repeatError, setRepeatError] = useState<string | null>(null);
  const [repeatSaving, setRepeatSaving] = useState(false);

  const isHotelAgentOwner = role === "HotelAgent"; // ownership itself is enforced server-side; this only decides whether to show the button
  const canConfirmReceipt = (isAdmin || isHotelAgentOwner) && delivery?.status === "Delivered" && !delivery?.recipientConfirmed;

  async function confirmReceipt() {
    if (!delivery) return;
    setConfirmingReceipt(true);
    setGenError(null);
    try {
      await apiClient.post(`/deliveries/${delivery.id}/confirm-receipt`);
      reloadDelivery();
      reload();
    } catch (err) {
      setGenError(extractErrorMessage(err));
    } finally {
      setConfirmingReceipt(false);
    }
  }

  async function generateInvoice() {
    if (!orderId) return;
    setGenerating(true);
    setGenError(null);
    try {
      const { data: invoiceId } = await apiClient.post<string>("/invoices/generate", { orderId, discount: 0 });
      navigate(`/invoices/${invoiceId}`);
    } catch (err) {
      setGenError(extractErrorMessage(err));
    } finally {
      setGenerating(false);
    }
  }

  async function runAction(action: string, needsReason?: boolean) {
    if (!orderId) return;
    let reason: string | null = null;
    if (needsReason) {
      reason = window.prompt(t("orderStatus.reasonPrompt")) ?? "";
    }
    setBusyAction(action);
    try {
      await apiClient.post(`/orders/${orderId}/${action}`, needsReason ? { reason } : undefined);
      reload();
    } catch (err) {
      setGenError(extractErrorMessage(err));
    } finally {
      setBusyAction(null);
    }
  }

  async function submitRepeat() {
    if (!order) return;
    setRepeatSaving(true);
    setRepeatError(null);
    try {
      // Same items and customer as this order, as a brand-new Order - the
      // clean way to place the "same thing again" without touching this
      // order or its invoice (GenerateInvoiceCommandHandler only allows one
      // invoice per order, on purpose - see its remarks).
      const { data: newOrderId } = await apiClient.post<string>("/orders", {
        customerId: order.customerId,
        source: order.source,
        agentUserId: order.agentUserId ?? null,
        notes: order.notes || null,
        requestedDeliveryDate: repeatDate || null,
        items: order.items.map((i) => ({ productId: i.productId, quantity: i.quantity }))
      });
      navigate(`/orders/${newOrderId}`);
    } catch (err) {
      setRepeatError(extractErrorMessage(err));
    } finally {
      setRepeatSaving(false);
    }
  }

  if (loading) return <LoadingState label={t("common.loading")} />;
  if (error) return <ErrorState message={error} />;
  if (!order) return null;

  // Invoicing is a billing/admin function too - agents can view but not generate.
  const canInvoice = isAdmin && ["Confirmed", "Preparing", "Shipped", "Delivered", "Completed"].includes(order.status);
  const allActions = ACTIONS_BY_STATUS[order.status] ?? [];
  const actions = allActions.filter((a) => isAdmin || !a.adminOnly);

  return (
    <div>
      <Link to="/orders" className="mb-3 inline-flex items-center gap-1 text-sm text-evergreen-700 hover:underline">
        <ArrowLeft className="h-4 w-4" /> {t("common.back")}
      </Link>

      <PageHeader
        eyebrow={order.orderNumber}
        title={order.customerName ?? t("orders.title")}
        description={new Date(order.orderDate).toLocaleString()}
        action={
          canInvoice ? (
            <button className="btn-primary" onClick={generateInvoice} disabled={generating}>
              <Receipt className="h-4 w-4" /> {generating ? t("orders.generating") : t("orders.generateInvoice")}
            </button>
          ) : undefined
        }
      />

      <div className="mb-4 flex flex-wrap items-center gap-2">
        <StatusBadge status={order.status} />
        {actions.map((a) => (
          <button
            key={a.action}
            className={`btn-secondary px-3 py-1 text-xs ${a.action === "cancel" || a.action === "reject" || a.action === "return" ? "text-clay-600" : ""}`}
            onClick={() => runAction(a.action, a.needsReason)}
            disabled={busyAction === a.action}
          >
            {t(a.labelKey)}
          </button>
        ))}
        {!isAdmin && allActions.some((a) => a.adminOnly) && (
          <span className="text-xs text-ink-300">{t("orderStatus.awaitingOps")}</span>
        )}
        {(isAdmin || isHotelAgentOwner) && (
          <button className="btn-secondary px-3 py-1 text-xs" onClick={() => setRepeating((r) => !r)}>
            <Repeat className="h-3.5 w-3.5" /> {t("orders.repeatOrder")}
          </button>
        )}
      </div>

      {repeating && (
        <div className="mb-4 rounded-md border border-evergreen-100 bg-evergreen-50/50 p-4">
          <p className="mb-2 text-sm text-ink-600">{t("orders.repeatOrderHint")}</p>
          <div className="flex flex-wrap items-end gap-2">
            <div>
              <label className="label text-xs">{t("visualOrder.deliveryDateLabel")}</label>
              <input type="date" className="input" value={repeatDate} onChange={(e) => setRepeatDate(e.target.value)} />
            </div>
            <button className="btn-primary px-3 py-1.5 text-sm" onClick={submitRepeat} disabled={repeatSaving}>
              {repeatSaving ? t("common.saving") : t("orders.confirmRepeat")}
            </button>
            <button className="btn-secondary px-3 py-1.5 text-sm" onClick={() => setRepeating(false)}>{t("common.cancel")}</button>
          </div>
          {repeatError && <div className="mt-2"><ErrorState message={repeatError} /></div>}
        </div>
      )}

      {isAdmin && ["Confirmed", "Preparing", "Shipped"].includes(order.status) && (
        <Link
          to="/deliveries"
          state={{ orderId: order.id }}
          className="mb-4 inline-block text-sm text-evergreen-700 underline underline-offset-2"
        >
          {t("orders.manageDeliveryLink")}
        </Link>
      )}

      {delivery && (
        <div className="mb-4 flex flex-wrap items-center gap-3 rounded-md border border-evergreen-100 bg-evergreen-50/50 px-3 py-2 text-sm">
          <StatusBadge status={delivery.status} />
          {delivery.driverName && <span className="text-ink-600">{t("deliveries.driverName")}: {delivery.driverName}</span>}
          {delivery.recipientConfirmed ? (
            <span className="text-evergreen-700">{t("orders.receiptConfirmed")}</span>
          ) : canConfirmReceipt ? (
            <button className="btn-primary px-3 py-1 text-xs" onClick={confirmReceipt} disabled={confirmingReceipt}>
              {confirmingReceipt ? t("common.saving") : t("orders.confirmReceipt")}
            </button>
          ) : delivery.status === "Delivered" ? (
            <span className="text-xs text-wheat-600">{t("orders.awaitingReceiptConfirmation")}</span>
          ) : null}
        </div>
      )}

      {order.requestedDeliveryDate && (
        <div className="mb-3 rounded-md border border-wheat-200 bg-wheat-50 px-3 py-2 text-sm text-ink-600">
          <span className="font-medium text-evergreen-900">{t("visualOrder.deliveryDateLabel")}:</span>{" "}
          {new Date(order.requestedDeliveryDate).toLocaleDateString()}
        </div>
      )}

      {order.notes && (
        <div className="mb-4 rounded-md border border-evergreen-100 bg-evergreen-50/50 px-3 py-2 text-sm text-ink-600">
          <span className="font-medium text-evergreen-900">{t("common.notes")}:</span> {order.notes}
        </div>
      )}

      {genError && <div className="mb-4"><ErrorState message={genError} /></div>}

      <div className="card overflow-x-auto">
        <table className="table-shell">
          <thead>
            <tr>
              <th>{t("orders.product")}</th>
              <th>{t("common.quantity")}</th>
              <th>{t("products.unitPrice")}</th>
              <th>{t("orders.lineTotal")}</th>
            </tr>
          </thead>
          <tbody>
            {order.items.map((item) => (
              <tr key={item.productId}>
                <td>{item.productName}</td>
                <td>{item.quantity}</td>
                <td>{item.unitPrice.toFixed(2)} {t("common.currency")}</td>
                <td className="font-medium">{item.lineTotal.toFixed(2)} {t("common.currency")}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="flex justify-end border-t border-evergreen-100 px-4 py-3 text-sm">
          <span className="font-medium text-ink-900">{t("orders.subtotal")}: {order.subtotal.toFixed(2)} {t("common.currency")}</span>
        </div>
      </div>
    </div>
  );
}
