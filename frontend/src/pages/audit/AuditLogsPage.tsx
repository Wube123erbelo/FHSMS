import { useState } from "react";
import PageHeader from "../../components/PageHeader";
import BackLink from "../../components/BackLink";
import ExportButtons from "../../components/ExportButtons";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import type { AuditLogDto } from "../../api/types";

const ACTION_COLORS: Record<string, string> = {
  Created: "bg-evergreen-100 text-evergreen-700",
  Modified: "bg-wheat-100 text-wheat-600",
  Deleted: "bg-clay-500/10 text-clay-600"
};

export default function AuditLogsPage() {
  const { t } = useTranslation();
  const [entityName, setEntityName] = useState("");

  const url = entityName ? `/auditlogs?entityName=${encodeURIComponent(entityName)}` : "/auditlogs";
  const { data: logs, loading, error } = useFetch<AuditLogDto[]>(url, [entityName]);

  return (
    <div>
      <BackLink to="/settings" />
      <PageHeader
        title={t("audit.title")}
        description={t("audit.description")}
        action={<ExportButtons basePath="/auditlogs" filenameBase="audit-logs" params={{ entityName: entityName || undefined }} />}
      />

      <div className="card mb-6 max-w-lg p-6">
        <label className="label">{t("audit.filterEntity")}</label>
        <input
          className="input"
          placeholder="e.g. Invoice, TaxConfiguration, Product"
          value={entityName}
          onChange={(e) => setEntityName(e.target.value)}
        />
      </div>

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {logs && logs.length === 0 && <EmptyState title={t("audit.emptyTitle")} />}

      {logs && logs.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("audit.entity")}</th>
                <th>{t("audit.entityId")}</th>
                <th>{t("audit.action")}</th>
                <th>{t("audit.performedBy")}</th>
                <th>{t("audit.performedAt")}</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log) => (
                <tr key={log.id}>
                  <td className="font-medium">{log.entityName}</td>
                  <td className="font-mono text-xs">{log.entityId}</td>
                  <td><span className={`badge ${ACTION_COLORS[log.action] ?? "bg-ink-900/5 text-ink-600"}`}>{log.action}</span></td>
                  <td>{log.performedBy ?? "-"}</td>
                  <td>{new Date(log.performedAt).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
