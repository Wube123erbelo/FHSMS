import { useState } from "react";
import { Plus } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { CategoryDto } from "../../api/types";

export default function CategoriesPage() {
  const { t } = useTranslation();
  const { data: categories, loading, error, reload } = useFetch<CategoryDto[]>("/categories");
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<CategoryDto | null>(null);
  const [showInactive, setShowInactive] = useState(false);

  const inactiveCount = (categories ?? []).filter((c) => !c.isActive).length;
  const visibleCategories = (categories ?? []).filter((c) => showInactive || c.isActive);

  // "Delete" is a soft-delete (IsActive -> false) - categories, units, and
  // customers are referenced by products/orders elsewhere, so a real
  // hard-delete could silently break historical records. The same button
  // also restores an item: once deactivated it disappears from the default
  // view below (matching what a "delete" button should feel like), and
  // reappears - with a "Restore" label instead - only when "Show inactive"
  // is checked.
  async function toggleActive(category: CategoryDto) {
    if (category.isActive) {
      if (!confirm(`${t("common.delete")} "${category.name}"?`)) return;
      try {
        await apiClient.delete(`/categories/${category.id}`);
        reload();
      } catch (err) {
        alert(extractErrorMessage(err));
      }
    } else {
      try {
        await apiClient.put(`/categories/${category.id}`, { name: category.name, description: category.description, isActive: true });
        reload();
      } catch (err) {
        alert(extractErrorMessage(err));
      }
    }
  }

  return (
    <div>
      <PageHeader
        title={t("products.category")}
        description="Product categories used across AgriLink."
        action={
          <button className="btn-primary" onClick={() => { setEditing(null); setShowForm((s) => !s); }}>
            <Plus className="h-4 w-4" /> {t("common.create")}
          </button>
        }
      />

      {(showForm || editing) && (
        <div className="card mb-6 max-w-md p-6">
          <CategoryForm existing={editing} onDone={() => { setShowForm(false); setEditing(null); reload(); }} />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {categories && categories.length === 0 && <EmptyState title={t("products.category")} />}

      {categories && categories.length > 0 && (
        <>
        <label className="mb-3 flex items-center gap-2 text-xs text-ink-600">
          <input type="checkbox" checked={showInactive} onChange={(e) => setShowInactive(e.target.checked)} />
          {t("common.showInactive")}{inactiveCount > 0 ? ` (${inactiveCount})` : ""}
        </label>
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("common.id")}</th>
                <th>{t("common.name")}</th>
                <th>{t("common.notes")}</th>
                <th>{t("common.status")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {visibleCategories.map((c) => (
                <tr key={c.id}>
                  <td className="font-mono text-xs">{c.code}</td>
                  <td className="font-medium">{c.name}</td>
                  <td>{c.description ?? "-"}</td>
                  <td><StatusBadge status={c.isActive ? "Active" : "Inactive"} /></td>
                  <td>
                    <div className="flex gap-1">
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => { setShowForm(false); setEditing(c); }}>
                        {t("common.edit")}
                      </button>
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => toggleActive(c)}>
                        {c.isActive ? t("common.delete") : t("common.restore")}
                      </button>
                    </div>
                  </td>
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

function CategoryForm({ existing, onDone }: { existing: CategoryDto | null; onDone: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState(existing?.name ?? "");
  const [description, setDescription] = useState(existing?.description ?? "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      if (existing) {
        await apiClient.put(`/categories/${existing.id}`, { name, description: description || null, isActive: existing.isActive });
      } else {
        await apiClient.post("/categories", { name, description: description || null });
      }
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <label className="label">{t("common.name")}</label>
        <input className="input" value={name} onChange={(e) => setName(e.target.value)} />
      </div>
      <div>
        <label className="label">{t("common.notes")}</label>
        <input className="input" value={description} onChange={(e) => setDescription(e.target.value)} />
      </div>
      {error && <ErrorState message={error} />}
      <div className="flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving || !name}>
          {saving ? t("common.saving") : existing ? t("common.save") : t("common.create")}
        </button>
        {existing && <button className="btn-secondary" onClick={onDone}>{t("common.cancel")}</button>}
      </div>
    </div>
  );
}
