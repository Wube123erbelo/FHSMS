import { useState } from "react";
import { Link } from "react-router-dom";
import { Plus, Trash2 } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import StatusBadge from "../../components/StatusBadge";
import { ErrorState, LoadingState, EmptyState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import VisualOrderForm from "./VisualOrderForm";
import type { CustomerDto, OrderDto, OrderSourceType, ProductDto } from "../../api/types";

interface DraftItem {
  productId: string;
  quantity: number;
}

export default function OrdersPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const isFieldAgent = role === "HotelAgent" || role === "FarmerAgent";
  const [customerId, setCustomerId] = useState("");
  const [source, setSource] = useState<OrderSourceType>("HotelPortal");
  const [items, setItems] = useState<DraftItem[]>([{ productId: "", quantity: 1 }]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);

  const { data: orders, loading, error: listError, reload } = useFetch<OrderDto[]>("/orders");
  const { data: customers } = useFetch<CustomerDto[]>(showForm ? "/customers" : null, [showForm]);
  const { data: products } = useFetch<ProductDto[]>(showForm ? "/products" : null, [showForm]);

  function updateItem(index: number, patch: Partial<DraftItem>) {
    setItems((prev) => prev.map((it, i) => (i === index ? { ...it, ...patch } : it)));
  }

  function productFor(productId: string) {
    return products?.find((p) => p.id === productId);
  }

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/orders", {
        customerId,
        source,
        agentUserId: null,
        items: items.filter((i) => i.productId).map((i) => ({ productId: i.productId, quantity: i.quantity }))
      });
      setShowForm(false);
      setCustomerId("");
      setItems([{ productId: "", quantity: 1 }]);
      reload();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  async function cancelOrder(id: string) {
    await apiClient.post(`/orders/${id}/cancel`);
    reload();
  }

  return (
    <div>
      <PageHeader
        title={t("orders.title")}
        description={t("orders.description")}
        action={
          <div className="flex gap-2">
            {isAdmin && <ExportButtons basePath="/orders" filenameBase="orders" />}
            <button className="btn-primary" onClick={() => setShowForm((s) => !s)}>
              <Plus className="h-4 w-4" /> {t("orders.newOrder")}
            </button>
          </div>
        }
      />

      {showForm && isFieldAgent && (
        <div className="mb-6">
          <VisualOrderForm onCreated={() => { setShowForm(false); reload(); }} />
        </div>
      )}

      {showForm && !isFieldAgent && (
        <div className="card mb-6 max-w-2xl p-6">
          <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("orders.newOrder")}</h3>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="label">{t("customers.title")}</label>
              <select className="input" value={customerId} onChange={(e) => setCustomerId(e.target.value)}>
                <option value="">-</option>
                {(customers ?? []).filter((c) => c.isActive).map((c) => (
                  <option key={c.id} value={c.id}>{c.code} - {c.name}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="label">{t("orders.source")}</label>
              <select className="input" value={source} onChange={(e) => setSource(e.target.value as OrderSourceType)}>
                <option value="HotelPortal">{t("orders.sourceHotelPortal")}</option>
                <option value="HotelAgent">{t("orders.sourceHotelAgent")}</option>
                <option value="FarmerAgent">{t("orders.sourceFarmerAgent")}</option>
                <option value="Telegram">{t("orders.sourceTelegram")}</option>
                <option value="PublicPortal">{t("orders.sourcePublicPortal")}</option>
              </select>
            </div>
          </div>

          <div className="mt-5">
            <p className="label mb-2">{t("orders.items")}</p>
            <div className="space-y-3">
              {items.map((item, index) => {
                const product = productFor(item.productId);
                return (
                  <div key={index} className="rounded-md border border-ink-300/20 p-3">
                    <div className="flex gap-2">
                      <select
                        className="input"
                        value={item.productId}
                        onChange={(e) => updateItem(index, { productId: e.target.value })}
                      >
                        <option value="">{t("orders.chooseProduct")}</option>
                        {(products ?? []).filter((p) => p.isActive).map((p) => (
                          <option key={p.id} value={p.id}>
                            {p.sku} - {p.name} ({p.currentSellingPrice?.toFixed(2) ?? "-"} {t("common.currency")} / {p.unitAbbreviation ?? "-"})
                          </option>
                        ))}
                      </select>
                      <button className="btn-secondary px-2" onClick={() => setItems((prev) => prev.filter((_, i) => i !== index))} aria-label="Remove item">
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>

                    <div className="mt-2 flex items-center gap-2">
                      <label className="text-xs text-ink-600 whitespace-nowrap">
                        {t("common.quantity")}{product?.unitAbbreviation ? ` (${product.unitAbbreviation})` : ""}:
                      </label>
                      <input
                        type="number"
                        min={0.01}
                        step={0.01}
                        className="input w-32"
                        value={item.quantity}
                        onChange={(e) => updateItem(index, { quantity: Number(e.target.value) })}
                      />
                      {product && (
                        <span className="text-xs text-evergreen-700">
                          {t("orders.quantityHint")
                            .replace("{qty}", String(item.quantity))
                            .replace("{unit}", product.unitAbbreviation ?? "-")
                            .replace("{product}", product.name)}
                        </span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
            <button className="btn-secondary mt-2" onClick={() => setItems((prev) => [...prev, { productId: "", quantity: 1 }])}>
              <Plus className="h-4 w-4" /> {t("orders.addItem")}
            </button>
          </div>

          {error && <div className="mt-4"><ErrorState message={error} /></div>}

          <button className="btn-primary mt-5" onClick={submit} disabled={saving || !customerId}>
            {saving ? t("orders.creating") : t("orders.createAndConfirm")}
          </button>
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {listError && <ErrorState message={listError} />}
      {orders && orders.length === 0 && <EmptyState title={t("orders.title")} />}

      {orders && orders.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("orders.title")} #</th>
                <th>{t("customers.title")}</th>
                <th>{t("common.status")}</th>
                <th>{t("common.date")}</th>
                <th>{t("orders.subtotal")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o) => (
                <tr key={o.id}>
                  <td className="font-mono text-xs">
                    <Link to={`/orders/${o.id}`} className="text-evergreen-700 underline underline-offset-2">{o.orderNumber}</Link>
                  </td>
                  <td>{o.customerName ?? "-"}</td>
                  <td><StatusBadge status={o.status} /></td>
                  <td>{new Date(o.orderDate).toLocaleDateString()}</td>
                  <td>{o.subtotal.toFixed(2)} {t("common.currency")}</td>
                  <td>
                    {["Draft", "Pending", "Confirmed", "Preparing"].includes(o.status) && (
                      <button className="btn-secondary px-2 py-1 text-xs text-clay-600" onClick={() => cancelOrder(o.id)}>
                        {t("invoices.cancelOrder")}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
