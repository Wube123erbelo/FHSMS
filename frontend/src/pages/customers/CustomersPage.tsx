import { useState } from "react";
import { Plus } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { CustomerDto, CustomerCreditStatusDto } from "../../api/types";

export default function CustomersPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const canEdit = role === "SuperAdmin" || role === "HotelAgent";
  const { data: customers, loading, error, reload } = useFetch<CustomerDto[]>("/customers");
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<CustomerDto | null>(null);
  const [creditFor, setCreditFor] = useState<CustomerDto | null>(null);
  const [showInactive, setShowInactive] = useState(false);

  const inactiveCount = (customers ?? []).filter((c) => !c.isActive).length;
  const visibleCustomers = (customers ?? []).filter((c) => showInactive || c.isActive);

  // Suspend/Activate and Delete/Restore are the exact same underlying
  // action (Customer.IsActive true/false) under two different labels - this
  // one button now covers both, and deleted customers disappear from the
  // list below by default instead of just changing a status badge.
  async function toggleActive(customer: CustomerDto) {
    if (customer.isActive && !confirm(`${t("common.delete")} "${customer.name}"?`)) return;
    await apiClient.post(`/customers/${customer.id}/${customer.isActive ? "suspend" : "activate"}`);
    reload();
  }

  return (
    <div>
      <PageHeader
        title={t("customers.title")}
        description={t("customers.description")}
        action={
          <div className="flex gap-2">
            {isAdmin && <ExportButtons basePath="/customers" filenameBase="customers" />}
            <button className="btn-primary" onClick={() => { setEditing(null); setShowForm((s) => !s); }}>
              <Plus className="h-4 w-4" /> {t("customers.newCustomer")}
            </button>
          </div>
        }
      />

      {showForm && (
        <div className="mb-6">
          <NewCustomerForm onCreated={() => { setShowForm(false); reload(); }} />
        </div>
      )}

      {editing && (
        <div className="mb-6">
          <EditCustomerForm customer={editing} onDone={() => { setEditing(null); reload(); }} onCancel={() => setEditing(null)} />
        </div>
      )}

      {creditFor && (
        <div className="mb-6">
          <CreditStatusPanel customer={creditFor} onClose={() => setCreditFor(null)} />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {customers && customers.length === 0 && <EmptyState title={t("customers.title")} />}

      {customers && customers.length > 0 && (
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
                <th>{t("customers.contactPerson")}</th>
                <th>{t("customers.phone")}</th>
                <th>{t("customers.creditLimit")}</th>
                <th>{t("common.status")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {visibleCustomers.map((c) => (
                <tr key={c.id}>
                  <td className="font-mono text-xs">{c.code}</td>
                  <td className="font-medium">{c.name}</td>
                  <td>{c.contactPerson ?? "-"}</td>
                  <td>{c.phone ?? "-"}</td>
                  <td>{c.creditLimit !== undefined && c.creditLimit !== null ? `${c.creditLimit.toFixed(2)} ${t("common.currency")}` : "-"}</td>
                  <td><StatusBadge status={c.isActive ? "Active" : "Inactive"} /></td>
                  <td>
                    <div className="flex gap-1">
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => setCreditFor(c)}>
                        {t("customers.creditStatus")}
                      </button>
                      {canEdit && (
                        <button className="btn-secondary px-2 py-1 text-xs" onClick={() => { setShowForm(false); setEditing(c); }}>
                          {t("common.edit")}
                        </button>
                      )}
                      {isAdmin && (
                        <button className="btn-secondary px-2 py-1 text-xs" onClick={() => toggleActive(c)}>
                          {c.isActive ? t("common.delete") : t("common.restore")}
                        </button>
                      )}
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

function NewCustomerForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState("");
  const [contactPerson, setContactPerson] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [address, setAddress] = useState("");
  const [creditLimit, setCreditLimit] = useState<string>("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/customers", {
        name, contactPerson, phone, email, address,
        creditLimit: creditLimit ? Number(creditLimit) : null
      });
      setName(""); setContactPerson(""); setPhone(""); setEmail(""); setAddress(""); setCreditLimit("");
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("customers.newCustomer")}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("common.name")} value={name} onChange={setName} />
        <Field label={t("customers.contactPerson")} value={contactPerson} onChange={setContactPerson} />
        <Field label={t("customers.phone")} value={phone} onChange={setPhone} />
        <Field label={t("customers.email")} value={email} onChange={setEmail} />
        <div className="sm:col-span-2">
          <Field label={t("customers.address")} value={address} onChange={setAddress} />
        </div>
        <div>
          <label className="label">{t("customers.creditLimit")} ({t("common.currency")})</label>
          <input type="number" className="input" value={creditLimit} onChange={(e) => setCreditLimit(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !name}>
        {saving ? t("common.saving") : t("common.create")}
      </button>
    </div>
  );
}

function Field({ label, value, onChange }: { label: string; value: string; onChange: (v: string) => void }) {
  return (
    <div>
      <label className="label">{label}</label>
      <input className="input" value={value} onChange={(e) => onChange(e.target.value)} />
    </div>
  );
}

function EditCustomerForm({ customer, onDone, onCancel }: { customer: CustomerDto; onDone: () => void; onCancel: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState(customer.name);
  const [contactPerson, setContactPerson] = useState(customer.contactPerson ?? "");
  const [phone, setPhone] = useState(customer.phone ?? "");
  const [email, setEmail] = useState(customer.email ?? "");
  const [address, setAddress] = useState(customer.address ?? "");
  const [creditLimit, setCreditLimit] = useState<string>(customer.creditLimit != null ? String(customer.creditLimit) : "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.put(`/customers/${customer.id}`, {
        name, contactPerson: contactPerson || null, phone: phone || null, email: email || null,
        address: address || null, creditLimit: creditLimit ? Number(creditLimit) : null, isActive: customer.isActive
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
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("common.edit")}: {customer.name}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("common.name")} value={name} onChange={setName} />
        <Field label={t("customers.contactPerson")} value={contactPerson} onChange={setContactPerson} />
        <Field label={t("customers.phone")} value={phone} onChange={setPhone} />
        <Field label={t("customers.email")} value={email} onChange={setEmail} />
        <div className="sm:col-span-2">
          <Field label={t("customers.address")} value={address} onChange={setAddress} />
        </div>
        <div>
          <label className="label">{t("customers.creditLimit")} ({t("common.currency")})</label>
          <input type="number" className="input" value={creditLimit} onChange={(e) => setCreditLimit(e.target.value)} />
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

function CreditStatusPanel({ customer, onClose }: { customer: CustomerDto; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, loading, error } = useFetch<CustomerCreditStatusDto>(`/customers/${customer.id}/credit-status`);

  return (
    <div className="card p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-base font-semibold text-evergreen-900">{t("customers.creditStatus")}: {customer.name}</h3>
        <button className="btn-secondary px-2 py-1 text-xs" onClick={onClose}>{t("common.cancel")}</button>
      </div>
      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {data && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          <Stat label={t("customers.creditLimit")} value={data.creditLimit !== undefined && data.creditLimit !== null ? data.creditLimit.toFixed(2) : "-"} />
          <Stat label={t("customers.currentExposure")} value={data.currentExposure.toFixed(2)} />
          <Stat label={t("customers.availableCredit")} value={data.availableCredit !== undefined && data.availableCredit !== null ? data.availableCredit.toFixed(2) : "-"} highlight={data.overLimit} />
          <Stat label={t("invoices.balanceDue")} value={String(data.outstandingInvoiceCount)} />
        </div>
      )}
    </div>
  );
}

function Stat({ label, value, highlight }: { label: string; value: string; highlight?: boolean }) {
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-ink-300">{label}</p>
      <p className={`mt-0.5 text-sm font-medium ${highlight ? "text-clay-600" : "text-ink-900"}`}>{value}</p>
    </div>
  );
}
