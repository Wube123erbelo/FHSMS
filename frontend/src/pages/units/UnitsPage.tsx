import { useState } from "react";
import { Plus } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { UnitDto } from "../../api/types";

export default function UnitsPage() {
  const { t } = useTranslation();
  const { data: units, loading, error, reload } = useFetch<UnitDto[]>("/units");
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<UnitDto | null>(null);
  const [showInactive, setShowInactive] = useState(false);

  const inactiveCount = (units ?? []).filter((u) => !u.isActive).length;
  const visibleUnits = (units ?? []).filter((u) => showInactive || u.isActive);

  // Same soft-delete pattern as CategoriesPage: DELETE only ever sets
  // IsActive false (units are referenced by products), so the button
  // toggles between deleting and restoring depending on current state,
  // and deleted units disappear from the list below by default.
  async function toggleActive(unit: UnitDto) {
    if (unit.isActive) {
      if (!confirm(`${t("common.delete")} "${unit.name}"?`)) return;
      try {
        await apiClient.delete(`/units/${unit.id}`);
        reload();
      } catch (err) {
        alert(extractErrorMessage(err));
      }
    } else {
      try {
        await apiClient.put(`/units/${unit.id}`, { name: unit.name, abbreviation: unit.abbreviation, isActive: true });
        reload();
      } catch (err) {
        alert(extractErrorMessage(err));
      }
    }
  }

  return (
    <div>
      <PageHeader
        title="Units"
        description="Units of measure (kg, crate, litre, ...) used by products."
        action={
          <button className="btn-primary" onClick={() => { setEditing(null); setShowForm((s) => !s); }}>
            <Plus className="h-4 w-4" /> {t("common.create")}
          </button>
        }
      />

      {(showForm || editing) && (
        <div className="card mb-6 max-w-md p-6">
          <UnitForm
            existing={editing}
            onDone={() => { setShowForm(false); setEditing(null); reload(); }}
          />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {units && units.length === 0 && <EmptyState title="Units" />}

      {units && units.length > 0 && (
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
                <th>Abbreviation</th>
                <th>{t("common.status")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {visibleUnits.map((u) => (
                <tr key={u.id}>
                  <td className="font-mono text-xs">{u.code}</td>
                  <td className="font-medium">{u.name}</td>
                  <td>{u.abbreviation}</td>
                  <td><StatusBadge status={u.isActive ? "Active" : "Inactive"} /></td>
                  <td>
                    <div className="flex gap-1">
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => { setShowForm(false); setEditing(u); }}>
                        {t("common.edit")}
                      </button>
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => toggleActive(u)}>
                        {u.isActive ? t("common.delete") : t("common.restore")}
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

function UnitForm({ existing, onDone }: { existing: UnitDto | null; onDone: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState(existing?.name ?? "");
  const [abbreviation, setAbbreviation] = useState(existing?.abbreviation ?? "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      if (existing) {
        await apiClient.put(`/units/${existing.id}`, { name, abbreviation, isActive: existing.isActive });
      } else {
        await apiClient.post("/units", { name, abbreviation });
      }
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div>
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">
        {existing ? t("common.edit") : t("common.create")}
      </h3>
      <div className="space-y-4">
        <div>
          <label className="label">{t("common.name")}</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} placeholder="Kilogram" />
        </div>
        <div>
          <label className="label">Abbreviation</label>
          <input className="input" value={abbreviation} onChange={(e) => setAbbreviation(e.target.value)} placeholder="kg" />
        </div>

        {error && <ErrorState message={error} />}

        <button className="btn-primary" onClick={submit} disabled={saving || !name || !abbreviation}>
          {saving ? t("common.saving") : t("common.save")}
        </button>
      </div>
    </div>
  );
}

