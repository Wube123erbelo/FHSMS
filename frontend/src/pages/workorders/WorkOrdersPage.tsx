import { useState } from "react";
import { Plus } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { WorkOrderDto, WorkOrderPriority } from "../../api/types";

export default function WorkOrdersPage() {
  const { t } = useTranslation();
  const { data: workOrders, loading, error, reload } = useFetch<WorkOrderDto[]>("/workorders");
  const [showForm, setShowForm] = useState(false);

  async function transition(id: string, action: "start" | "complete" | "cancel") {
    await apiClient.post(`/workorders/${id}/${action}`);
    reload();
  }

  return (
    <div>
      <PageHeader
        title={t("workOrders.title")}
        description={t("workOrders.description")}
        action={
          <button className="btn-primary" onClick={() => setShowForm((s) => !s)}>
            <Plus className="h-4 w-4" /> {t("workOrders.newWorkOrder")}
          </button>
        }
      />

      {showForm && (
        <div className="mb-6">
          <NewWorkOrderForm onCreated={() => { setShowForm(false); reload(); }} />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {workOrders && workOrders.length === 0 && <EmptyState title={t("workOrders.emptyTitle")} />}

      {workOrders && workOrders.length > 0 && (
        <div className="space-y-3">
          {workOrders.map((w) => (
            <div key={w.id} className="card flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="font-medium text-ink-900">{w.title}</p>
                <p className="text-xs text-ink-300">
                  {w.priority} {w.dueDate ? `· ${t("workOrders.dueDate")}: ${new Date(w.dueDate).toLocaleDateString()}` : ""}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <StatusBadge status={w.status} />
                {w.status === "Pending" && (
                  <button className="btn-secondary px-2 py-1 text-xs" onClick={() => transition(w.id, "start")}>{t("workOrders.start")}</button>
                )}
                {(w.status === "Pending" || w.status === "InProgress") && (
                  <>
                    <button className="btn-secondary px-2 py-1 text-xs" onClick={() => transition(w.id, "complete")}>{t("workOrders.complete")}</button>
                    <button className="btn-secondary px-2 py-1 text-xs text-clay-600" onClick={() => transition(w.id, "cancel")}>{t("workOrders.cancel")}</button>
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function NewWorkOrderForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState<WorkOrderPriority>("Normal");
  const [dueDate, setDueDate] = useState("");
  const [assignedToUserId, setAssignedToUserId] = useState("");
  const [orderId, setOrderId] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/workorders", {
        title,
        description: description || null,
        orderId: orderId || null,
        assignedToUserId: assignedToUserId || null,
        priority,
        dueDate: dueDate ? new Date(dueDate).toISOString() : null
      });
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("workOrders.newWorkOrder")}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label className="label">{t("workOrders.orderTitle")}</label>
          <input className="input" value={title} onChange={(e) => setTitle(e.target.value)} />
        </div>
        <div className="sm:col-span-2">
          <label className="label">{t("common.notes")}</label>
          <input className="input" value={description} onChange={(e) => setDescription(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("workOrders.priority")}</label>
          <select className="input" value={priority} onChange={(e) => setPriority(e.target.value as WorkOrderPriority)}>
            <option value="Low">{t("workOrders.priorityLow")}</option>
            <option value="Normal">{t("workOrders.priorityNormal")}</option>
            <option value="High">{t("workOrders.priorityHigh")}</option>
            <option value="Urgent">{t("workOrders.priorityUrgent")}</option>
          </select>
        </div>
        <div>
          <label className="label">{t("workOrders.dueDate")}</label>
          <input type="date" className="input" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("workOrders.assignedTo")}</label>
          <input className="input" value={assignedToUserId} onChange={(e) => setAssignedToUserId(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("workOrders.linkedOrderId")}</label>
          <input className="input" value={orderId} onChange={(e) => setOrderId(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !title}>
        {saving ? t("common.saving") : t("common.create")}
      </button>
    </div>
  );
}
