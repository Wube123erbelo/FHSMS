import { useState } from "react";
import { Check } from "lucide-react";
import { ErrorState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import type { CustomerDto, OrderSourceType, ProductDto } from "../../api/types";

/**
 * Maps a product name to a food emoji for the icon-tile grid - the same
 * "visual order form with product icons" the mobile mockup shows. Products
 * don't carry an icon field in the data model, so this is a best-effort
 * keyword match with a produce-box fallback; swap in a real per-product
 * icon/image field later if the catalog grows large enough that guessing by
 * name stops being reliable.
 */
function iconFor(productName: string): string {
  const name = productName.toLowerCase();
  if (name.includes("teff") || name.includes("wheat") || name.includes("grain")) return "\u{1F33E}"; // 🌾
  if (name.includes("tomato")) return "\u{1F345}"; // 🍅
  if (name.includes("potato")) return "\u{1F954}"; // 🥔
  if (name.includes("onion")) return "\u{1F9C5}"; // 🧅
  if (name.includes("cabbage") || name.includes("lettuce")) return "\u{1F96C}"; // 🥬
  if (name.includes("carrot")) return "\u{1F955}"; // 🥕
  if (name.includes("pepper") || name.includes("chili")) return "\u{1F336}\uFE0F"; // 🌶️
  if (name.includes("maize") || name.includes("corn")) return "\u{1F33D}"; // 🌽
  if (name.includes("milk") || name.includes("dairy")) return "\u{1F95B}"; // 🥛
  if (name.includes("egg")) return "\u{1F95A}"; // 🥚
  if (name.includes("fruit") || name.includes("banana") || name.includes("mango")) return "\u{1F34C}"; // 🍌
  return "\u{1F4E6}"; // 📦
}

interface CartLine {
  productId: string;
  productName: string;
  icon: string;
  unitAbbreviation?: string;
  unitPrice: number;
  quantity: number;
}

export default function VisualOrderForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const { data: customers } = useFetch<CustomerDto[]>("/customers");
  const { data: products } = useFetch<ProductDto[]>("/products");

  const [customerId, setCustomerId] = useState("");
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [cart, setCart] = useState<CartLine[]>([]);
  const [note, setNote] = useState("");
  const [deliveryDate, setDeliveryDate] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const activeProducts = (products ?? []).filter((p) => p.isActive);
  const selectedProduct = activeProducts.find((p) => p.id === selectedProductId) ?? null;

  function addToOrder() {
    if (!selectedProduct || quantity <= 0) return;
    setCart((prev) => {
      const existing = prev.find((l) => l.productId === selectedProduct.id);
      if (existing) {
        return prev.map((l) => (l.productId === selectedProduct.id ? { ...l, quantity: l.quantity + quantity } : l));
      }
      return [
        ...prev,
        {
          productId: selectedProduct.id,
          productName: selectedProduct.name,
          icon: iconFor(selectedProduct.name),
          unitAbbreviation: selectedProduct.unitAbbreviation,
          // Orders are always priced at the selling price - what the hotel
          // is charged - never the buying price. For non-admin roles the API
          // has already stripped currentBuyingPrice out of the response
          // entirely, so this is also the only field that could be non-null.
          unitPrice: selectedProduct.currentSellingPrice ?? 0,
          quantity
        }
      ];
    });
    setSelectedProductId(null);
    setQuantity(1);
  }

  function removeLine(productId: string) {
    setCart((prev) => prev.filter((l) => l.productId !== productId));
  }

  async function submit() {
    if (!customerId || cart.length === 0 || !deliveryDate) return;
    setSaving(true);
    setError(null);
    try {
      const source: OrderSourceType = "HotelAgent";
      await apiClient.post("/orders", {
        customerId,
        source,
        agentUserId: null, // server derives this from the logged-in agent's own token
        notes: note || null,
        requestedDeliveryDate: deliveryDate,
        items: cart.map((l) => ({ productId: l.productId, quantity: l.quantity }))
      });
      setCustomerId("");
      setCart([]);
      setNote("");
      setDeliveryDate("");
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  const total = cart.reduce((sum, l) => sum + l.unitPrice * l.quantity, 0);

  return (
    <div className="card mx-auto max-w-md overflow-hidden p-0">
      <div className="bg-evergreen-700 px-5 py-4">
        <h3 className="text-base font-semibold text-white">{t("visualOrder.newOrderTitle")}</h3>
      </div>

      <div className="space-y-5 p-5">
        {/* Step 1: which hotel */}
        <div>
          <label className="label">{t("visualOrder.hotelLabel")}</label>
          <select className="input" value={customerId} onChange={(e) => setCustomerId(e.target.value)}>
            <option value="">{t("visualOrder.selectHotel")}</option>
            {(customers ?? []).filter((c) => c.isActive).map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>

        {/* Step 2: product icon grid */}
        <div>
          <label className="label">{t("visualOrder.productLabel")}</label>
          <div className="grid grid-cols-4 gap-2">
            {activeProducts.map((p) => {
              const active = p.id === selectedProductId;
              return (
                <button
                  key={p.id}
                  type="button"
                  onClick={() => setSelectedProductId(p.id)}
                  className={`flex flex-col items-center gap-1 rounded-lg border-2 p-2 text-center transition ${
                    active ? "border-evergreen-600 bg-evergreen-50" : "border-evergreen-100 hover:border-evergreen-300"
                  }`}
                >
                  <span className="text-2xl leading-none">{iconFor(p.name)}</span>
                  <span className="line-clamp-1 text-[11px] font-medium text-evergreen-900">{p.name}</span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Step 3: quantity + add */}
        {selectedProduct && (
          <div className="rounded-lg border border-evergreen-100 bg-evergreen-50/50 p-3">
            <label className="label">
              {t("visualOrder.quantityLabel")} {selectedProduct.unitAbbreviation ? `(${selectedProduct.unitAbbreviation})` : ""}
            </label>
            <div className="flex gap-2">
              <input
                type="number"
                min={0.01}
                step="0.01"
                className="input"
                value={quantity}
                onChange={(e) => setQuantity(Number(e.target.value))}
              />
              <button type="button" className="btn-primary px-4" onClick={addToOrder}>
                <Check className="h-4 w-4" /> {t("visualOrder.add")}
              </button>
            </div>
          </div>
        )}

        {/* Cart */}
        {cart.length > 0 && (
          <div>
            <label className="label">{t("visualOrder.orderSummary")}</label>
            <div className="space-y-1.5">
              {cart.map((l) => (
                <div key={l.productId} className="flex items-center justify-between rounded-md bg-canvas px-3 py-2 text-sm">
                  <span className="flex items-center gap-2">
                    <span className="text-lg">{l.icon}</span>
                    {l.productName} &times; {l.quantity} {l.unitAbbreviation ?? ""}
                  </span>
                  <span className="flex items-center gap-3">
                    <span className="text-ink-600">{(l.unitPrice * l.quantity).toFixed(2)} {t("common.currency")}</span>
                    <button onClick={() => removeLine(l.productId)} className="text-xs text-clay-600 underline">{t("common.delete")}</button>
                  </span>
                </div>
              ))}
            </div>
            <p className="mt-2 text-right text-sm font-semibold text-evergreen-900">
              {t("orders.subtotal")}: {total.toFixed(2)} {t("common.currency")}
            </p>
          </div>
        )}

        <div>
          <label className="label">{t("visualOrder.deliveryDateLabel")} <span className="text-clay-600">*</span></label>
          <input
            type="date"
            className="input"
            value={deliveryDate}
            min={new Date().toISOString().slice(0, 10)}
            onChange={(e) => setDeliveryDate(e.target.value)}
          />
          <p className="mt-1 text-xs text-ink-300">{t("visualOrder.deliveryDateHint")}</p>
        </div>

        <div>
          <label className="label">{t("visualOrder.noteLabel")}</label>
          <input className="input" value={note} onChange={(e) => setNote(e.target.value)} placeholder={t("visualOrder.notePlaceholder")} />
        </div>

        {error && <ErrorState message={error} />}

        <button
          className="btn-primary w-full justify-center py-3 text-base"
          onClick={submit}
          disabled={saving || !customerId || cart.length === 0 || !deliveryDate}
        >
          {saving ? t("orders.creating") : t("visualOrder.submitOrder")}
        </button>
      </div>
    </div>
  );
}
