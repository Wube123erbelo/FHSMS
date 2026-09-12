import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { FarmerInvoiceDto } from "../../api/types";

/**
 * The buying-side counterpart to InvoicesPage. Every row here was
 * auto-generated the instant a stock-in was logged (Quantity x the buying
 * price at that moment) - there is no manual entry anywhere in this list.
 * FarmerAgent sees only invoices they logged; SuperAdmin/Driver see all
 * (enforced server-side in GetFarmerInvoicesQueryHandler).
 */
export default function FarmerInvoicesPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const { data: invoices, loading, error } = useFetch<FarmerInvoiceDto[]>("/farmerinvoices");

  return (
    <div>
      <PageHeader
        title={t("farmerInvoices.title")}
        description={t("farmerInvoices.description")}
        action={isAdmin ? <ExportButtons basePath="/farmerinvoices" filenameBase="farmer-invoices" /> : undefined}
      />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {invoices && invoices.length === 0 && <EmptyState title={t("farmerInvoices.title")} />}

      {invoices && invoices.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("invoices.invoiceId")} #</th>
                <th>{t("common.date")}</th>
                <th>{t("common.name")}</th>
                <th>{t("inventory.farmer")}</th>
                <th>{t("common.quantity")}</th>
                <th>{t("products.buyingPrice")}</th>
                <th>{t("orders.subtotal")}</th>
                <th>{t("common.status")}</th>
              </tr>
            </thead>
            <tbody>
              {invoices.map((inv) => (
                <tr key={inv.id}>
                  <td className="font-mono text-xs">{inv.invoiceNumber}</td>
                  <td>{new Date(inv.createdAt).toLocaleDateString()}</td>
                  <td>{inv.productName ?? "-"}</td>
                  <td>{inv.farmerName ?? "-"} {inv.agentName ? `(${inv.agentName})` : ""}</td>
                  <td>{inv.quantity} {inv.unitAbbreviation ?? ""}</td>
                  <td>{inv.buyingPriceApplied.toFixed(2)} {t("common.currency")}</td>
                  <td className="font-medium">{inv.totalAmount.toFixed(2)} {t("common.currency")}</td>
                  <td>
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                        inv.status === "Approved" ? "bg-evergreen-50 text-evergreen-700" : "bg-wheat-50 text-wheat-700"
                      }`}
                    >
                      {inv.status === "Approved" ? t("farmerInvoices.approved") : t("farmerInvoices.pendingApproval")}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
