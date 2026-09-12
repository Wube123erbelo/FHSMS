import { useState } from "react";
import { Link } from "react-router-dom";
import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { InvoiceDto } from "../../api/types";

export default function InvoicesPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const { data: invoices, loading, error } = useFetch<InvoiceDto[]>("/invoices");

  return (
    <div>
      <PageHeader
        title={t("invoices.title")}
        description={t("invoices.description")}
        action={
          isAdmin ? <ExportButtons basePath="/invoices" filenameBase="invoices" /> : undefined
        }
      />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {invoices && invoices.length === 0 && <EmptyState title={t("invoices.title")} />}

      {invoices && invoices.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("invoices.invoiceId")} #</th>
                <th>{t("common.date")}</th>
                <th>{t("common.status")}</th>
                <th>{t("invoices.grandTotal")}</th>
                <th>{t("invoices.balanceDue")}</th>
              </tr>
            </thead>
            <tbody>
              {invoices.map((inv) => (
                <tr key={inv.id}>
                  <td className="font-mono text-xs">
                    <Link to={`/invoices/${inv.id}`} className="text-evergreen-700 underline underline-offset-2">{inv.invoiceNumber}</Link>
                  </td>
                  <td>{new Date(inv.invoiceDate).toLocaleDateString()}</td>
                  <td><StatusBadge status={inv.status} /></td>
                  <td>{inv.grandTotal.toFixed(2)} {t("common.currency")}</td>
                  <td>{inv.balanceDue.toFixed(2)} {t("common.currency")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
