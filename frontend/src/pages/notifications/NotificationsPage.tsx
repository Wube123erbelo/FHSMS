import PageHeader from "../../components/PageHeader";
import BackLink from "../../components/BackLink";
import StatusBadge from "../../components/StatusBadge";
import { ErrorState, EmptyState, LoadingState } from "../../components/States";
import { apiClient } from "../../api/client";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import type { NotificationDto } from "../../api/types";

export default function NotificationsPage() {
  const { t } = useTranslation();
  const { data: notifications, loading, error, reload } = useFetch<NotificationDto[]>("/notifications/mine");

  async function markRead(id: string) {
    await apiClient.post(`/notifications/${id}/read`);
    reload();
  }

  return (
    <div>
      <BackLink to="/settings" />
      <PageHeader title={t("notifications.title")} description={t("notifications.description")} />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {notifications && notifications.length === 0 && <EmptyState title={t("notifications.emptyTitle")} />}

      {notifications && notifications.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th></th>
                <th>{t("notifications.channel")}</th>
                <th>{t("notifications.subject")}</th>
                <th>{t("common.status")}</th>
                <th>{t("notifications.sentAt")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {notifications.map((n) => (
                <tr key={n.id} className={!n.isRead ? "bg-evergreen-50/50 font-medium" : undefined}>
                  <td>
                    {!n.isRead && <span className="inline-block h-2 w-2 rounded-full bg-evergreen-500" title={t("notifications.unread")} />}
                  </td>
                  <td>{n.channel}</td>
                  <td>{n.subject}</td>
                  <td><StatusBadge status={n.status} /></td>
                  <td>{n.sentAt ? new Date(n.sentAt).toLocaleString() : "-"}</td>
                  <td>
                    {!n.isRead && (
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => markRead(n.id)}>
                        {t("notifications.markRead")}
                      </button>
                    )}
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
