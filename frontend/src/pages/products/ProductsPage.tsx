import { useMemo, useState } from "react";
import { Plus, History } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { CategoryDto, ProductDto, ProductPriceHistoryDto, TaxProfileType, UnitDto } from "../../api/types";

const TAX_PROFILE_LABEL: Record<TaxProfileType, string> = {
  StandardVat: "Standard VAT",
  ZeroRated: "Zero rated",
  TaxExempt: "Tax exempt",
  NoTax: "No tax"
};

/** Turns "Fresh Roma Tomato" + category code "CAT-0001" into a suggested SKU like "CAT1-FRT-001". Just a suggestion - the field stays free text. */
function suggestSku(name: string, categoryCode: string | undefined, sequence: number): string {
  const initials = name
    .trim()
    .split(/\s+/)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("")
    .slice(0, 4);
  const catPart = categoryCode ? categoryCode.replace(/\D/g, "").padStart(2, "0").slice(-2) : "00";
  return `C${catPart}-${initials || "SKU"}-${String(sequence).padStart(3, "0")}`;
}

export default function ProductsPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const { data: products, loading, error, reload } = useFetch<ProductDto[]>("/products?activeOnly=false");
  const { data: categories } = useFetch<CategoryDto[]>("/categories");
  const { data: units } = useFetch<UnitDto[]>("/units");
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<ProductDto | null>(null);
  const [historyFor, setHistoryFor] = useState<ProductDto | null>(null);
  const [priceFormFor, setPriceFormFor] = useState<ProductDto | null>(null);
  const [showInactive, setShowInactive] = useState(false);

  const inactiveCount = (products ?? []).filter((p) => !p.isActive).length;
  const visibleProducts = (products ?? []).filter((p) => showInactive || p.isActive);

  // Delete is a soft-delete (IsActive -> false) since products are
  // referenced by past orders/invoices/stock history. Restoring reuses the
  // same "update" endpoint the edit form uses, just flipping IsActive back
  // on - there was previously no way to undo a product deletion in this UI
  // at all.
  async function remove(product: ProductDto) {
    if (!confirm(`${t("common.delete")} "${product.name}"?`)) return;
    try {
      await apiClient.delete(`/products/${product.id}`);
      reload();
    } catch (err) {
      alert(extractErrorMessage(err));
    }
  }

  async function restore(product: ProductDto) {
    try {
      await apiClient.put(`/products/${product.id}`, {
        sku: product.sku, name: product.name, description: product.description,
        categoryId: product.categoryId, unitId: product.unitId, taxProfile: product.taxProfile,
        lowStockThreshold: product.lowStockThreshold, isActive: true
      });
      reload();
    } catch (err) {
      alert(extractErrorMessage(err));
    }
  }


  return (
    <div>
      <PageHeader
        title={t("products.title")}
        description={t("products.description")}
        action={
          <div className="flex gap-2">
            {isAdmin && <ExportButtons basePath="/products" filenameBase="products" />}
            {isAdmin && (
              <button className="btn-primary" onClick={() => setShowForm((s) => !s)}>
                <Plus className="h-4 w-4" /> {t("products.newProduct")}
              </button>
            )}
          </div>
        }
      />

      {showForm && (
        <div className="mb-6">
          <NewProductForm
            categories={categories ?? []}
            units={units ?? []}
            existingCount={products?.length ?? 0}
            onCreated={() => { setShowForm(false); reload(); }}
          />
        </div>
      )}

      {editing && (
        <div className="mb-6">
          <EditProductForm
            product={editing}
            categories={categories ?? []}
            units={units ?? []}
            onDone={() => { setEditing(null); reload(); }}
            onCancel={() => setEditing(null)}
          />
        </div>
      )}

      {priceFormFor && (
        <div className="mb-6">
          <SchedulePriceForm product={priceFormFor} onDone={() => { setPriceFormFor(null); reload(); }} onCancel={() => setPriceFormFor(null)} />
        </div>
      )}

      {historyFor && (
        <div className="mb-6">
          <PriceHistoryPanel product={historyFor} onClose={() => setHistoryFor(null)} />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {products && products.length === 0 && (
        <EmptyState title={t("products.emptyTitle")} description={t("products.emptyDescription")} />
      )}

      {products && products.length > 0 && (
        <>
        <label className="mb-3 flex items-center gap-2 text-xs text-ink-600">
          <input type="checkbox" checked={showInactive} onChange={(e) => setShowInactive(e.target.checked)} />
          {t("common.showInactive")}{inactiveCount > 0 ? ` (${inactiveCount})` : ""}
        </label>
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("products.sku")}</th>
                <th>{t("common.name")}</th>
                <th>{t("products.category")}</th>
                {role !== "HotelAgent" && role !== "HotelCustomer" && role !== "PublicPortalUser" && <th>{t("products.buyingPrice")}</th>}
                {role !== "FarmerAgent" && <th>{t("products.sellingPrice")}</th>}
                <th>{t("products.taxProfile")}</th>
                <th>{t("common.status")}</th>
                {isAdmin && <th></th>}
              </tr>
            </thead>
            <tbody>
              {visibleProducts.map((p) => (
                <tr key={p.id}>
                  <td className="font-mono text-xs">{p.sku}</td>
                  <td className="font-medium">{p.name}</td>
                  <td>{p.categoryCode ? `${p.categoryCode} - ${p.categoryName}` : p.categoryName ?? "-"}</td>
                  {role !== "HotelAgent" && role !== "HotelCustomer" && role !== "PublicPortalUser" && (
                    <td>
                      {p.currentBuyingPrice !== undefined
                        ? `${p.currentBuyingPrice.toFixed(2)} ${t("common.currency")}${p.unitAbbreviation ? ` / ${p.unitAbbreviation}` : ""}`
                        : t("products.noPriceYet")}
                    </td>
                  )}
                  {role !== "FarmerAgent" && (
                    <td>
                      {p.currentSellingPrice !== undefined
                        ? `${p.currentSellingPrice.toFixed(2)} ${t("common.currency")}${p.unitAbbreviation ? ` / ${p.unitAbbreviation}` : ""}`
                        : t("products.noPriceYet")}
                    </td>
                  )}
                  <td>{TAX_PROFILE_LABEL[p.taxProfile]}</td>
                  <td>{p.isActive ? t("common.active") : t("common.inactive")}</td>
                  {isAdmin && (
                    <td>
                      <div className="flex gap-1">
                        <button className="btn-secondary px-2 py-1 text-xs" onClick={() => setEditing(p)}>
                          {t("common.edit")}
                        </button>
                        <button className="btn-secondary px-2 py-1 text-xs" onClick={() => setPriceFormFor(p)}>
                          {t("products.schedulePrice")}
                        </button>
                        <button className="btn-secondary px-2 py-1 text-xs" onClick={() => setHistoryFor(p)}>
                          <History className="h-3.5 w-3.5" />
                        </button>
                        {p.isActive ? (
                          <button className="btn-secondary px-2 py-1 text-xs text-clay-600" onClick={() => remove(p)}>
                            {t("common.delete")}
                          </button>
                        ) : (
                          <button className="btn-secondary px-2 py-1 text-xs" onClick={() => restore(p)}>
                            {t("common.restore")}
                          </button>
                        )}
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        </>
      )}
    </div>
  );
}

function NewProductForm({
  categories, units, existingCount, onCreated
}: { categories: CategoryDto[]; units: UnitDto[]; existingCount: number; onCreated: () => void }) {
  const { t } = useTranslation();
  const [sku, setSku] = useState("");
  const [name, setName] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [unitId, setUnitId] = useState("");
  const [initialBuyingPrice, setInitialBuyingPrice] = useState(0);
  const [initialSellingPrice, setInitialSellingPrice] = useState(0);
  const [taxProfile, setTaxProfile] = useState<TaxProfileType>("StandardVat");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sellingBelowBuying = initialSellingPrice < initialBuyingPrice;

  const selectedCategory = categories.find((c) => c.id === categoryId);
  const selectedUnit = units.find((u) => u.id === unitId);
  const skuSuggestion = useMemo(
    () => (name ? suggestSku(name, selectedCategory?.code, existingCount + 1) : null),
    [name, selectedCategory, existingCount]
  );

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/products", { sku, name, categoryId, unitId, initialBuyingPrice, initialSellingPrice, taxProfile });
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("products.newProduct")}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("common.name")}</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} />
        </div>

        <div>
          <label className="label">{t("products.sku")}</label>
          <input className="input" value={sku} onChange={(e) => setSku(e.target.value)} />
          {skuSuggestion && (
            <p className="mt-1 text-[11px] text-ink-300">
              {t("products.skuHint")} <button type="button" className="underline" onClick={() => setSku(skuSuggestion)}>{skuSuggestion}</button>
            </p>
          )}
        </div>

        <div>
          <label className="label">{t("products.category")}</label>
          <select className="input" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
            <option value="">-</option>
            {categories.filter((c) => c.isActive).map((c) => (
              <option key={c.id} value={c.id}>{c.code} - {c.name}</option>
            ))}
          </select>
        </div>

        <div>
          <label className="label">Unit</label>
          <select className="input" value={unitId} onChange={(e) => setUnitId(e.target.value)}>
            <option value="">-</option>
            {units.filter((u) => u.isActive).map((u) => (
              <option key={u.id} value={u.id}>{u.code} - {u.name} ({u.abbreviation})</option>
            ))}
          </select>
        </div>

        <div>
          <label className="label">{t("products.initialBuyingPrice")} ({t("common.currency")})</label>
          <input type="number" className="input" value={initialBuyingPrice} onChange={(e) => setInitialBuyingPrice(Number(e.target.value))} />
          <p className="mt-1 text-[11px] text-evergreen-700">{t("products.buyingPriceHint")}</p>
        </div>

        <div>
          <label className="label">{t("products.initialSellingPrice")} ({t("common.currency")})</label>
          <input type="number" className="input" value={initialSellingPrice} onChange={(e) => setInitialSellingPrice(Number(e.target.value))} />
          <p className="mt-1 text-[11px] text-evergreen-700">{t("products.sellingPriceHint")}</p>
        </div>

        {selectedUnit && (
          <p className="sm:col-span-2 -mt-2 text-[11px] text-evergreen-700">
            {t("products.priceIsPerUnit")} <strong>{selectedUnit.abbreviation}</strong> ({selectedUnit.name})
          </p>
        )}

        <div>
          <label className="label">{t("products.taxProfile")}</label>
          <select className="input" value={taxProfile} onChange={(e) => setTaxProfile(e.target.value as TaxProfileType)}>
            {Object.entries(TAX_PROFILE_LABEL).map(([value, label]) => (
              <option key={value} value={value}>{label}</option>
            ))}
          </select>
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}
      {sellingBelowBuying && !error && (
        <p className="mt-4 text-xs font-medium text-clay-600">{t("products.sellingBelowBuyingWarning")}</p>
      )}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !name || !categoryId || !unitId || !sku || sellingBelowBuying}>
        {saving ? t("common.saving") : t("common.create")}
      </button>
    </div>
  );
}

function EditProductForm({
  product, categories, units, onDone, onCancel
}: { product: ProductDto; categories: CategoryDto[]; units: UnitDto[]; onDone: () => void; onCancel: () => void }) {
  const { t } = useTranslation();
  const [sku, setSku] = useState(product.sku);
  const [name, setName] = useState(product.name);
  const [description, setDescription] = useState(product.description ?? "");
  const [categoryId, setCategoryId] = useState(product.categoryId);
  const [unitId, setUnitId] = useState(product.unitId);
  const [taxProfile, setTaxProfile] = useState<TaxProfileType>(product.taxProfile);
  const [isActive, setIsActive] = useState(product.isActive);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.put(`/products/${product.id}`, {
        sku, name, description: description || null, categoryId, unitId, taxProfile,
        lowStockThreshold: product.lowStockThreshold ?? null, isActive
      });
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("common.edit")}: {product.name}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("common.name")}</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("products.sku")}</label>
          <input className="input" value={sku} onChange={(e) => setSku(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("products.category")}</label>
          <select className="input" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>{c.code} - {c.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label">Unit</label>
          <select className="input" value={unitId} onChange={(e) => setUnitId(e.target.value)}>
            {units.map((u) => (
              <option key={u.id} value={u.id}>{u.code} - {u.name} ({u.abbreviation})</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label">{t("products.taxProfile")}</label>
          <select className="input" value={taxProfile} onChange={(e) => setTaxProfile(e.target.value as TaxProfileType)}>
            {Object.entries(TAX_PROFILE_LABEL).map(([value, label]) => (
              <option key={value} value={value}>{label}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label">{t("common.status")}</label>
          <select className="input" value={isActive ? "1" : "0"} onChange={(e) => setIsActive(e.target.value === "1")}>
            <option value="1">{t("common.active")}</option>
            <option value="0">{t("common.inactive")}</option>
          </select>
        </div>
        <div className="sm:col-span-2">
          <label className="label">{t("common.notes")}</label>
          <input className="input" value={description} onChange={(e) => setDescription(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <div className="mt-4 flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving || !name || !sku}>
          {saving ? t("common.saving") : t("common.save")}
        </button>
        <button className="btn-secondary" onClick={onCancel}>{t("common.cancel")}</button>
      </div>
    </div>
  );
}

function SchedulePriceForm({ product, onDone, onCancel }: { product: ProductDto; onDone: () => void; onCancel: () => void }) {
  const { t } = useTranslation();
  const [kind, setKind] = useState<"Buying" | "Selling">("Buying");
  const [price, setPrice] = useState(product.currentBuyingPrice ?? 0);
  const [effectiveFrom, setEffectiveFrom] = useState(new Date().toISOString().slice(0, 10));
  const [reason, setReason] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function changeKind(next: "Buying" | "Selling") {
    setKind(next);
    setPrice((next === "Buying" ? product.currentBuyingPrice : product.currentSellingPrice) ?? 0);
  }

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post(`/products/${product.id}/price`, {
        kind, price, effectiveFrom: new Date(effectiveFrom).toISOString(), reason: reason || null
      });
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("products.schedulePrice")}: {product.name}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-4">
        <div>
          <label className="label">{t("products.priceKind")}</label>
          <select className="input" value={kind} onChange={(e) => changeKind(e.target.value as "Buying" | "Selling")}>
            <option value="Buying">{t("products.buyingPrice")}</option>
            <option value="Selling">{t("products.sellingPrice")}</option>
          </select>
        </div>
        <div>
          <label className="label">{t("products.newPrice")}</label>
          <input type="number" className="input" value={price} onChange={(e) => setPrice(Number(e.target.value))} />
          {product.unitAbbreviation && (
            <p className="mt-1 text-[11px] text-evergreen-700">{t("products.priceIsPerUnit")} <strong>{product.unitAbbreviation}</strong></p>
          )}
        </div>
        <div>
          <label className="label">{t("products.effectiveFrom")}</label>
          <input type="date" className="input" value={effectiveFrom} onChange={(e) => setEffectiveFrom(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("products.reason")}</label>
          <input className="input" value={reason} onChange={(e) => setReason(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <div className="mt-4 flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving}>
          {saving ? t("common.saving") : t("products.schedulePrice")}
        </button>
        <button className="btn-secondary" onClick={onCancel}>{t("common.cancel")}</button>
      </div>
    </div>
  );
}

function PriceHistoryPanel({ product, onClose }: { product: ProductDto; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, loading, error } = useFetch<ProductPriceHistoryDto[]>(`/products/${product.id}/price-history`);

  return (
    <div className="card p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-base font-semibold text-evergreen-900">{t("products.priceHistory")}: {product.name}</h3>
        <button className="btn-secondary px-2 py-1 text-xs" onClick={onClose}>{t("common.cancel")}</button>
      </div>
      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {data && (
        <div className="overflow-x-auto">
        <table className="table-shell">
          <thead>
            <tr>
              <th>{t("products.priceKind")}</th>
              <th>{t("products.newPrice")}</th>
              <th>{t("products.effectiveFrom")}</th>
              <th>{t("products.effectiveTo")}</th>
              <th>{t("products.reason")}</th>
            </tr>
          </thead>
          <tbody>
            {data.map((p) => (
              <tr key={p.id}>
                <td>{p.kind === "Buying" ? t("products.buyingPrice") : t("products.sellingPrice")}</td>
                <td>{p.price.toFixed(2)} {t("common.currency")}</td>
                <td>{new Date(p.effectiveFrom).toLocaleDateString()}</td>
                <td>{p.effectiveTo ? new Date(p.effectiveTo).toLocaleDateString() : t("products.current")}</td>
                <td>{p.reason ?? "-"}</td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      )}
    </div>
  );
}
