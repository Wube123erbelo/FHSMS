import { useState } from "react";
import { Plus } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { FarmerDto } from "../../api/types";

export default function FarmersPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const canEdit = role === "SuperAdmin" || role === "FarmerAgent";
  const { data: farmers, loading, error, reload } = useFetch<FarmerDto[]>("/farmers");
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<FarmerDto | null>(null);

  async function toggleActive(farmer: FarmerDto) {
    await apiClient.post(`/farmers/${farmer.id}/${farmer.isActive ? "suspend" : "activate"}`);
    reload();
  }

  return (
    <div>
      <PageHeader
        title={t("farmers.title")}
        description={t("farmers.description")}
        action={
          <div className="flex gap-2">
            {isAdmin && <ExportButtons basePath="/farmers" filenameBase="farmers" />}
            <button className="btn-primary" onClick={() => { setEditing(null); setShowForm((s) => !s); }}>
              <Plus className="h-4 w-4" /> {t("farmers.newFarmer")}
            </button>
          </div>
        }
      />

      {showForm && (
        <div className="mb-6">
          <NewFarmerForm onCreated={() => { setShowForm(false); reload(); }} />
        </div>
      )}

      {editing && (
        <div className="mb-6">
          <EditFarmerForm farmer={editing} onDone={() => { setEditing(null); reload(); }} onCancel={() => setEditing(null)} />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {farmers && farmers.length === 0 && <EmptyState title={t("farmers.emptyTitle")} />}

      {farmers && farmers.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("common.id")}</th>
                <th>{t("common.name")}</th>
                <th>{t("customers.contactPerson")}</th>
                <th>{t("customers.phone")}</th>
                <th>{t("farmers.location")}</th>
                <th>{t("common.status")}</th>
                {(canEdit || isAdmin) && <th></th>}
              </tr>
            </thead>
            <tbody>
              {farmers.map((f) => (
                <tr key={f.id}>
                  <td className="font-mono text-xs">{f.code}</td>
                  <td className="font-medium">{f.name}</td>
                  <td>{f.contactPerson ?? "-"}</td>
                  <td>{f.phone ?? "-"}</td>
                  <td>{f.location ?? "-"}</td>
                  <td><StatusBadge status={f.isActive ? "Active" : "Inactive"} /></td>
                  {(canEdit || isAdmin) && (
                    <td>
                      <div className="flex gap-1">
                        {canEdit && (
                          <button className="btn-secondary px-2 py-1 text-xs" onClick={() => { setShowForm(false); setEditing(f); }}>
                            {t("common.edit")}
                          </button>
                        )}
                        {isAdmin && (
                          <button className="btn-secondary px-2 py-1 text-xs" onClick={() => toggleActive(f)}>
                            {f.isActive ? t("farmers.suspend") : t("farmers.activate")}
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
      )}
    </div>
  );
}

function NewFarmerForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState("");
  const [contactPerson, setContactPerson] = useState("");
  const [phone, setPhone] = useState("");
  const [location, setLocation] = useState("");
  const [bankAccountNumber, setBankAccountNumber] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/farmers", { name, contactPerson, phone, location, bankAccountNumber });
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("farmers.newFarmer")}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("common.name")}</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("customers.contactPerson")}</label>
          <input className="input" value={contactPerson} onChange={(e) => setContactPerson(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("customers.phone")}</label>
          <input className="input" value={phone} onChange={(e) => setPhone(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("farmers.location")}</label>
          <input className="input" value={location} onChange={(e) => setLocation(e.target.value)} />
        </div>
        <div className="sm:col-span-2">
          <label className="label">{t("farmers.bankAccountNumber")}</label>
          <input className="input" value={bankAccountNumber} onChange={(e) => setBankAccountNumber(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !name}>
        {saving ? t("common.saving") : t("common.create")}
      </button>
    </div>
  );
}

function EditFarmerForm({ farmer, onDone, onCancel }: { farmer: FarmerDto; onDone: () => void; onCancel: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState(farmer.name);
  const [contactPerson, setContactPerson] = useState(farmer.contactPerson ?? "");
  const [phone, setPhone] = useState(farmer.phone ?? "");
  const [location, setLocation] = useState(farmer.location ?? "");
  const [bankAccountNumber, setBankAccountNumber] = useState(farmer.bankAccountNumber ?? "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.put(`/farmers/${farmer.id}`, {
        name, contactPerson: contactPerson || null, phone: phone || null,
        location: location || null, bankAccountNumber: bankAccountNumber || null, isActive: farmer.isActive
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
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("common.edit")}: {farmer.name}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("common.name")}</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("customers.contactPerson")}</label>
          <input className="input" value={contactPerson} onChange={(e) => setContactPerson(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("customers.phone")}</label>
          <input className="input" value={phone} onChange={(e) => setPhone(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("farmers.location")}</label>
          <input className="input" value={location} onChange={(e) => setLocation(e.target.value)} />
        </div>
        <div className="sm:col-span-2">
          <label className="label">{t("farmers.bankAccountNumber")}</label>
          <input className="input" value={bankAccountNumber} onChange={(e) => setBankAccountNumber(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <div className="mt-4 flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving || !name}>
          {saving ? t("common.saving") : t("common.save")}
        </button>
        <button className="btn-secondary" onClick={onCancel}>{t("common.cancel")}</button>
      </div>
    </div>
  );
}
