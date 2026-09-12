import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import type { AdminDriverDto } from "../../api/types";

/**
 * Admin's read-only follow-up view of every registered driver. This is
 * deliberately NOT a registration form - an admin registering drivers here
 * would duplicate the "one login, one driver profile" rule (see
 * DriversController remarks). Admins create a driver's LOGIN via the Users
 * page (role = Driver); the driver then fills in their own profile after
 * signing in, and shows up here automatically once they do.
 */
export default function AdminDriversPage() {
  const { t } = useTranslation();
  const { data: drivers, loading, error } = useFetch<AdminDriverDto[]>("/drivers");

  return (
    <div>
      <PageHeader title={t("adminDrivers.title")} description={t("adminDrivers.description")} />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {drivers && drivers.length === 0 && <EmptyState title={t("adminDrivers.emptyTitle")} />}

      {drivers && drivers.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("common.id")}</th>
                <th>{t("driverPortal.fullName")}</th>
                <th>{t("driverPortal.phone")}</th>
                <th>{t("driverPortal.plateNumber")}</th>
                <th>{t("driverPortal.truckType")}</th>
                <th>{t("common.status")}</th>
                <th>{t("adminDrivers.pendingTrips")}</th>
                <th>{t("adminDrivers.completedTrips")}</th>
                <th>{t("adminDrivers.totalEarned")}</th>
              </tr>
            </thead>
            <tbody>
              {drivers.map((d) => (
                <tr key={d.id}>
                  <td className="font-mono text-xs">{d.code}</td>
                  <td className="font-medium">{d.fullName}</td>
                  <td>{d.phone ?? "-"}</td>
                  <td>{d.plateNumber ?? "-"}</td>
                  <td>{t(`driverPortal.truckTypes.${d.truckType}`)}</td>
                  <td><StatusBadge status={d.isActive ? "Active" : "Inactive"} /></td>
                  <td>{d.pendingTripCount}</td>
                  <td>{d.completedTripCount}</td>
                  <td>{d.totalEarned.toFixed(2)} {t("common.currency")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
