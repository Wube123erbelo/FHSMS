import { useState } from "react";
import { Plus, History, CheckCircle, Trash2 } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { InventoryTransactionType, StockLevelDto, ProductDto, FarmerDto, InventoryTransactionDto } from "../../api/types";

export default function InventoryPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const { data: levels, loading, error, reload } = useFetch<StockLevelDto[]>("/inventory/stock-levels");
  const [showForm, setShowForm] = useState(false);
  const [historyProductId, setHistoryProductId] = useState<string | null>(null);

  return (
    <div>
      <PageHeader
        title={t("inventory.title")}
        description={t("inventory.description")}
        action={
          <button className="btn-primary" onClick={() => setShowForm((s) => !s)}>
            <Plus className="h-4 w-4" /> {t("inventory.recordTransaction")}
          </button>
        }
      />

      {isAdmin && (
        <div className="mb-6">
          <PendingStockConfirmations onConfirmed={reload} />
        </div>
      )}

      {showForm && (
        <div className="mb-6">
          <NewTransactionForm onRecorded={() => { setShowForm(false); reload(); }} />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {levels && levels.length === 0 && <EmptyState title={t("inventory.emptyTitle")} />}

      {levels && levels.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("inventory.product")}</th>
                <th>{t("inventory.quantityOnHand")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {levels.map((l) => (
                <>
                  <tr key={l.productId}>
                    <td className="font-medium">{l.productName}</td>
                    <td className={l.quantityOnHand <= 0 ? "text-clay-600" : ""}>
                      {l.quantityOnHand} {l.unitAbbreviation}
                    </td>
                    <td>
                      <button
                        className="btn-secondary text-xs"
                        onClick={() => setHistoryProductId(historyProductId === l.productId ? null : l.productId)}
                      >
                        <History className="h-3.5 w-3.5" />
                        {historyProductId === l.productId ? t("inventory.hideHistory") : t("inventory.viewHistory")}
                      </button>
                    </td>
                  </tr>
                  {historyProductId === l.productId && (
                    <tr>
                      <td colSpan={3} className="bg-evergreen-50/40 p-0">
                        <ProductHistory productId={l.productId} />
                      </td>
                    </tr>
                  )}
                </>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function ProductHistory({ productId }: { productId: string }) {
  const { t } = useTranslation();
  const { data, loading, error } = useFetch<InventoryTransactionDto[]>(`/inventory/products/${productId}/history`);

  return (
    <div className="p-4">
      <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-evergreen-700">
        {t("inventory.historyTitle")}
      </h4>
      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {data && data.length === 0 && <p className="text-xs text-ink-300">{t("inventory.historyEmpty")}</p>}
      {data && data.length > 0 && (
        <ul className="space-y-1 text-sm">
          {data.map((tx) => (
            <li key={tx.id} className="flex items-center justify-between border-b border-evergreen-100 py-1">
              <span>{tx.type}</span>
              <span className={tx.quantityChange < 0 ? "text-clay-600" : "text-evergreen-700"}>
                {tx.quantityChange > 0 ? "+" : ""}{tx.quantityChange}
              </span>
              <span className="text-xs text-ink-300">{new Date(tx.createdAt).toLocaleDateString()}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

function NewTransactionForm({ onRecorded }: { onRecorded: () => void }) {
  const { t } = useTranslation();
  const { role } = useAuth();
  const { data: products } = useFetch<ProductDto[]>("/products");
  const { data: farmers } = useFetch<FarmerDto[]>("/farmers");

  const [type, setType] = useState<InventoryTransactionType>("Receiving");
  const [farmerId, setFarmerId] = useState("");
  const [reference, setReference] = useState("");
  const [notes, setNotes] = useState("");
  // Several products in one drop-off (a truck arriving with tomatoes, cabbage,
  // and mango together) works the same way the hotel order form builds up a
  // cart of items before submitting - one shared farmer/reference/notes, a
  // line per product, all recorded together.
  const [items, setItems] = useState<{ id: string; productId: string; quantity: number }[]>([
    { id: crypto.randomUUID(), productId: "", quantity: 1 }
  ]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Farmer agents record what they receive - hotel agents and admins only see
  // the field when it's actually relevant (a Receiving transaction).
  const showFarmerField = type === "Receiving";

  function addItem() {
    setItems((cur) => [...cur, { id: crypto.randomUUID(), productId: "", quantity: 1 }]);
  }

  function removeItem(id: string) {
    setItems((cur) => (cur.length > 1 ? cur.filter((i) => i.id !== id) : cur));
  }

  function updateItem(id: string, patch: Partial<{ productId: string; quantity: number }>) {
    setItems((cur) => cur.map((i) => (i.id === id ? { ...i, ...patch } : i)));
  }

  const validItems = items.filter((i) => i.productId && i.quantity > 0);
  const usedProductIds = new Set(items.map((i) => i.productId).filter(Boolean));
  const hasDuplicateProduct = usedProductIds.size !== items.filter((i) => i.productId).length;

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      // A shared reference ties every line of this drop-off together in the
      // history view, even though each product still gets its own ledger
      // entry underneath - generated only when the farmer agent didn't type
      // one themselves, so it never overwrites an intentional reference.
      const sharedReference = reference || (validItems.length > 1 ? `BATCH-${Date.now()}` : "");

      for (const item of validItems) {
        await apiClient.post("/inventory/transactions", {
          productId: item.productId,
          type,
          quantity: item.quantity,
          orderId: null,
          reference: sharedReference || null,
          notes: notes || null,
          farmerId: showFarmerField && farmerId ? farmerId : null
          // agentUserId is deliberately NOT sent - the server derives it from
          // your own login when you're a farmer agent, so you can't record
          // stock "as" someone else and no agentUserId field needs typing here.
        });
      }
      onRecorded();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("inventory.recordTransaction")}</h3>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("inventory.type")}</label>
          <select className="input" value={type} onChange={(e) => setType(e.target.value as InventoryTransactionType)}>
            <option value="Receiving">{t("inventory.typeReceiving")}</option>
            <option value="Issuing">{t("inventory.typeIssuing")}</option>
            <option value="Adjustment">{t("inventory.typeAdjustment")}</option>
            <option value="Damage">{t("inventory.typeDamage")}</option>
            <option value="Wastage">{t("inventory.typeWastage")}</option>
          </select>
        </div>
        {showFarmerField && (
          <div>
            <label className="label">{t("inventory.farmer")}</label>
            <select className="input" value={farmerId} onChange={(e) => setFarmerId(e.target.value)}>
              <option value="">{t("inventory.selectFarmer")}</option>
              {farmers?.map((f) => (
                <option key={f.id} value={f.id}>{f.name}</option>
              ))}
            </select>
            {role === "FarmerAgent" && <p className="mt-1 text-xs text-ink-300">{t("inventory.farmerHint")}</p>}
          </div>
        )}
        <div>
          <label className="label">{t("inventory.reference")}</label>
          <input className="input" value={reference} onChange={(e) => setReference(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("common.notes")}</label>
          <input className="input" value={notes} onChange={(e) => setNotes(e.target.value)} />
        </div>
      </div>

      <div className="mt-5">
        <div className="mb-2 flex items-center justify-between">
          <label className="label mb-0">{t("inventory.items")}</label>
          <button className="btn-secondary px-2.5 py-1 text-xs" onClick={addItem}>
            <Plus className="h-3.5 w-3.5" /> {t("inventory.addItem")}
          </button>
        </div>
        <div className="space-y-2">
          {items.map((item) => {
            const product = products?.find((p) => p.id === item.productId);
            // Buying price is auto-selected for reference here (this is the
            // "record transaction" role, never the selling price) - it's
            // what the company will owe the farmer for this line, at
            // Quantity x BuyingPrice, once the stock-in is confirmed.
            const buyingPrice = product?.currentBuyingPrice;
            const lineCost = buyingPrice !== undefined ? buyingPrice * (item.quantity || 0) : undefined;
            return (
              <div key={item.id} className="flex items-center gap-2">
                <select
                  className="input flex-1"
                  value={item.productId}
                  onChange={(e) => updateItem(item.id, { productId: e.target.value })}
                >
                  <option value="">{t("inventory.selectProduct")}</option>
                  {products?.map((p) => (
                    <option key={p.id} value={p.id}>{p.name}</option>
                  ))}
                </select>
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  className="input w-28"
                  placeholder={product?.unitAbbreviation ? `${t("common.quantity")} (${product.unitAbbreviation})` : t("common.quantity")}
                  value={item.quantity}
                  onChange={(e) => updateItem(item.id, { quantity: Number(e.target.value) })}
                />
                {type === "Receiving" && buyingPrice !== undefined && (
                  <span className="w-40 shrink-0 text-right text-xs text-evergreen-700">
                    {buyingPrice.toFixed(2)} {t("common.currency")}
                    {lineCost !== undefined && <> = <strong>{lineCost.toFixed(2)}</strong></>}
                  </span>
                )}
                <button
                  className="rounded-md p-2 text-ink-300 hover:bg-clay-50 hover:text-clay-600 disabled:opacity-30"
                  onClick={() => removeItem(item.id)}
                  disabled={items.length === 1}
                  aria-label={t("common.remove")}
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            );
          })}
        </div>
        {type === "Receiving" && (
          <p className="mt-2 text-[11px] text-evergreen-700">{t("inventory.buyingPriceHint")}</p>
        )}
        {hasDuplicateProduct && <p className="mt-2 text-xs text-clay-600">{t("inventory.duplicateProductHint")}</p>}
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button
        className="btn-primary mt-4"
        onClick={submit}
        disabled={saving || validItems.length === 0 || hasDuplicateProduct}
      >
        {saving ? t("common.saving") : t("inventory.recordTransaction")}
      </button>
    </div>
  );
}

function PendingStockConfirmations({ onConfirmed }: { onConfirmed: () => void }) {
  const { t } = useTranslation();
  const { data: pending, loading, error, reload } = useFetch<InventoryTransactionDto[]>("/inventory/pending-confirmations");
  const [confirmingId, setConfirmingId] = useState<string | null>(null);

  if (loading) return <LoadingState label={t("common.loading")} />;
  if (error) return <ErrorState message={error} />;
  if (!pending || pending.length === 0) return null;

  return (
    <div className="card border-wheat-400 p-5">
      <h3 className="mb-1 text-sm font-semibold text-evergreen-900">{t("inventory.pendingConfirmationsTitle")}</h3>
      <p className="mb-3 text-xs text-ink-300">{t("inventory.pendingConfirmationsHint")}</p>
      <div className="space-y-2">
        {pending.map((tx) => (
          <ConfirmationRow
            key={tx.id}
            tx={tx}
            confirming={confirmingId === tx.id}
            onStart={() => setConfirmingId(tx.id)}
            onCancel={() => setConfirmingId(null)}
            onDone={() => { setConfirmingId(null); reload(); onConfirmed(); }}
          />
        ))}
      </div>
    </div>
  );
}

function ConfirmationRow({
  tx, confirming, onStart, onCancel, onDone
}: { tx: InventoryTransactionDto; confirming: boolean; onStart: () => void; onCancel: () => void; onDone: () => void }) {
  const { t } = useTranslation();
  const [farmerWasPaid, setFarmerWasPaid] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function confirm() {
    setSaving(true);
    setError(null);
    try {
      // The amount owed to the farmer is never typed here - it comes from
      // the farmer invoice auto-generated when this stock was logged
      // (tx.farmerInvoiceTotalAmount = quantity x buying price at that
      // moment). Confirming just approves that invoice and records whether
      // the company has actually paid it out yet.
      await apiClient.post(`/inventory/transactions/${tx.id}/confirm`, { farmerWasPaid });
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="rounded-md border border-evergreen-100 p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="text-sm">
          <span className="font-medium text-evergreen-900">{tx.productName ?? tx.productId}</span>
          {" "}&middot; {tx.quantityChange} &middot;{" "}
          <span className="text-ink-300">
            {tx.farmerName ?? t("inventory.unknownFarmer")} ({tx.agentName ?? "-"})
          </span>
          {tx.farmerInvoiceTotalAmount !== undefined && (
            <>
              {" "}&middot;{" "}
              <span className="font-medium text-evergreen-900">
                {tx.farmerInvoiceNumber} - {tx.farmerInvoiceTotalAmount.toFixed(2)} {t("common.currency")}
              </span>
            </>
          )}
        </div>
        {!confirming && (
          <button className="btn-primary px-3 py-1 text-xs" onClick={onStart}>
            <CheckCircle className="h-3.5 w-3.5" /> {t("inventory.confirmReceipt")}
          </button>
        )}
      </div>

      {confirming && (
        <div className="mt-3 space-y-2 border-t border-evergreen-100 pt-3">
          {tx.farmerInvoiceTotalAmount !== undefined && (
            <p className="text-xs text-ink-600">
              {t("inventory.owedToFarmer")}: <strong>{tx.farmerInvoiceTotalAmount.toFixed(2)} {t("common.currency")}</strong>
              {" "}({tx.quantityChange} &times; {(tx.farmerInvoiceTotalAmount / tx.quantityChange).toFixed(2)} {t("common.currency")})
            </p>
          )}
          <label className="flex items-center gap-2 text-sm text-ink-600">
            <input type="checkbox" checked={farmerWasPaid} onChange={(e) => setFarmerWasPaid(e.target.checked)} />
            {t("inventory.farmerWasPaidLabel")}
          </label>
          {error && <ErrorState message={error} />}
          <div className="flex gap-2">
            <button className="btn-primary px-3 py-1 text-xs" onClick={confirm} disabled={saving}>
              {saving ? t("common.saving") : t("inventory.confirmAndSave")}
            </button>
            <button className="btn-secondary px-3 py-1 text-xs" onClick={onCancel}>{t("common.cancel")}</button>
          </div>
        </div>
      )}
    </div>
  );
}
