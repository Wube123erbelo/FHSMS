import { useState } from "react";
import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { DriverPaymentDto } from "../../api/types";

/**
 * The driver-side counterpart to FarmerInvoicesPage. Every row here was
 * auto-generated the instant a delivery was marked Delivered
 * (Delivery.TripPrice frozen at that moment) - there is no manual entry.
 * A Driver sees only their own payment records; SuperAdmin sees all and can
 * approve each one (which also records whether the driver has actually been
 * paid yet) - enforced server-side in GetDriverPaymentsQueryHandler /
 * DriverPaymentsController.
 */
export default function DriverPaymentsPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const { data: payments, loading, error, reload } = useFetch<DriverPaymentDto[]>("/driverpayments");

  return (
    <div>
      <PageHeader
        title={t("driverPayments.title")}
        description={t("driverPayments.description")}
        action={isAdmin ? <ExportButtons basePath="/driverpayments" filenameBase="driver-payments" /> : undefined}
      />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {payments && payments.length === 0 && <EmptyState title={t("driverPayments.title")} />}

      {payments && payments.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("common.date")}</th>
                {isAdmin && <th>{t("driverPayments.driver")}</th>}
                <th>{t("driverPayments.destination")}</th>
                <th>{t("driverPayments.amount")}</th>
                <th>{t("common.status")}</th>
                <th>{t("driverPayments.wasDriverPaid")}</th>
                {isAdmin && <th></th>}
              </tr>
            </thead>
            <tbody>
              {payments.map((p) => (
                <PaymentRow key={p.id} payment={p} isAdmin={isAdmin} onChanged={reload} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function PaymentRow({ payment: p, isAdmin, onChanged }: { payment: DriverPaymentDto; isAdmin: boolean; onChanged: () => void }) {
  const { t } = useTranslation();
  const [approving, setApproving] = useState(false);
  const [driverWasPaid, setDriverWasPaid] = useState(false);
  const [saving, setSaving] = useState(false);

  async function approve() {
    setSaving(true);
    try {
      await apiClient.post(`/driverpayments/${p.id}/approve`, { driverWasPaid });
      onChanged();
    } catch (err) {
      alert(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <tr>
      <td>{new Date(p.createdAt).toLocaleDateString()}</td>
      {isAdmin && <td>{p.driverName ?? "-"}</td>}
      <td>{p.destinationAddress ?? "-"}</td>
      <td className="font-medium">{p.amount.toFixed(2)} {t("common.currency")}</td>
      <td>
        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${p.status === "Approved" ? "bg-evergreen-50 text-evergreen-700" : "bg-wheat-50 text-wheat-700"}`}>
          {p.status === "Approved" ? t("farmerInvoices.approved") : t("farmerInvoices.pendingApproval")}
        </span>
      </td>
      <td>{p.driverWasPaid ? t("common.yes") : t("common.no")}</td>
      {isAdmin && (
        <td>
          {p.status === "PendingApproval" && !approving && (
            <button className="btn-secondary px-2 py-1 text-xs" onClick={() => setApproving(true)}>
              {t("common.approve")}
            </button>
          )}
          {approving && (
            <div className="flex items-center gap-2">
              <label className="flex items-center gap-1 text-xs text-ink-600">
                <input type="checkbox" checked={driverWasPaid} onChange={(e) => setDriverWasPaid(e.target.checked)} />
                {t("driverPayments.wasDriverPaid")}
              </label>
              <button className="btn-primary px-2 py-1 text-xs" onClick={approve} disabled={saving}>
                {saving ? t("common.saving") : t("common.confirm")}
              </button>
            </div>
          )}
        </td>
      )}
    </tr>
  );
}
