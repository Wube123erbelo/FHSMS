import { useState } from "react";
import { Truck, RefreshCw, MapPin, Package as PackageIcon, Wallet } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { DriverDto, DriverEarningsDto, TripDto, TruckType } from "../../api/types";

const TRUCK_TYPES: TruckType[] = ["Pickup", "SmallTruck", "MediumTruck", "Isuzu", "HeavyTruck", "Trailer", "Other"];

type Tab = "available" | "mine";

export default function DriverPortalPage() {
  const { t } = useTranslation();
  const { fullName } = useAuth();
  const { data: profile, loading: profileLoading, reload: reloadProfile } = useFetch<DriverDto | null>("/drivers/profile/me");
  const [tab, setTab] = useState<Tab>("available");

  return (
    <div>
      <PageHeader
        title={`${t("driverPortal.welcome")}, ${profile?.fullName ?? fullName ?? ""}`}
        description={t("driverPortal.description")}
      />

      <div className="mb-6">
        {profileLoading ? (
          <LoadingState label={t("common.loading")} />
        ) : (
          <DriverProfileCard profile={profile} accountFullName={fullName ?? ""} onSaved={reloadProfile} />
        )}
      </div>

      {profile && (
        <>
          <div className="mb-6">
            <EarningsSummary />
          </div>

          <div className="mb-4 flex gap-2 border-b border-evergreen-100">
            <TabButton active={tab === "available"} onClick={() => setTab("available")} label={t("driverPortal.availableTrips")} />
            <TabButton active={tab === "mine"} onClick={() => setTab("mine")} label={t("driverPortal.myTrips")} />
          </div>

          {tab === "available" ? <AvailableTripsBoard /> : <MyTripsList />}
        </>
      )}
    </div>
  );
}

function EarningsSummary() {
  const { t } = useTranslation();
  const { data: earnings } = useFetch<DriverEarningsDto>("/drivers/earnings/me");
  if (!earnings) return null;

  return (
    <div className="grid grid-cols-1 gap-4 rounded-lg border border-evergreen-100 bg-evergreen-50/50 p-4 sm:grid-cols-4">
      <div>
        <p className="text-xs uppercase tracking-wide text-evergreen-700">{t("driverPortal.totalEarned")}</p>
        <p className="mt-1 text-2xl font-semibold text-evergreen-900">{earnings.totalEarned.toFixed(2)} {t("common.currency")}</p>
      </div>
      <div>
        <p className="text-xs uppercase tracking-wide text-evergreen-700">{t("driverPortal.thisMonth")}</p>
        <p className="mt-1 text-2xl font-semibold text-evergreen-900">{earnings.thisMonthEarned.toFixed(2)} {t("common.currency")}</p>
      </div>
      <div>
        <p className="text-xs uppercase tracking-wide text-evergreen-700">{t("driverPortal.pendingValue")}</p>
        <p className="mt-1 text-2xl font-semibold text-evergreen-900">{earnings.pendingTripValue.toFixed(2)} {t("common.currency")}</p>
        <p className="mt-0.5 text-xs text-ink-300">{t("driverPortal.pendingValueHint")}</p>
      </div>
      <div className="flex items-center gap-2 sm:justify-end">
        <Wallet className="h-8 w-8 text-evergreen-500" strokeWidth={1.5} />
        <span className="text-sm text-ink-600">
          {earnings.completedTripCount} {t("driverPortal.completedTrips")}
        </span>
      </div>
    </div>
  );
}

function TabButton({ active, onClick, label }: { active: boolean; onClick: () => void; label: string }) {
  return (
    <button
      onClick={onClick}
      className={`border-b-2 px-3 py-2 text-sm font-medium transition ${
        active ? "border-evergreen-600 text-evergreen-800" : "border-transparent text-ink-300 hover:text-evergreen-700"
      }`}
    >
      {label}
    </button>
  );
}

function DriverProfileCard({
  profile, accountFullName, onSaved
}: { profile: DriverDto | null; accountFullName: string; onSaved: () => void }) {
  const { t } = useTranslation();
  const [editing, setEditing] = useState(profile === null);
  const [fullName, setFullName] = useState(profile?.fullName ?? accountFullName);
  const [phone, setPhone] = useState(profile?.phone ?? "");
  const [plateNumber, setPlateNumber] = useState(profile?.plateNumber ?? "");
  const [truckType, setTruckType] = useState<TruckType>(profile?.truckType ?? "Pickup");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/drivers/profile", { fullName, phone: phone || null, plateNumber: plateNumber || null, truckType });
      setEditing(false);
      onSaved();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  if (!editing && profile) {
    return (
      <div className="card flex flex-wrap items-center justify-between gap-4 p-5">
        <div className="flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-full bg-evergreen-600 text-white">
            <Truck className="h-5 w-5" />
          </div>
          <div>
            <p className="font-semibold text-evergreen-900">{profile.fullName}</p>
            <p className="text-xs text-ink-300">
              {profile.plateNumber ?? "-"} &middot; {t(`driverPortal.truckTypes.${profile.truckType}`)} &middot; {profile.phone ?? "-"}
            </p>
          </div>
        </div>
        <button className="btn-secondary" onClick={() => setEditing(true)}>{t("common.edit")}</button>
      </div>
    );
  }

  return (
    <div className="card p-6">
      <h3 className="mb-1 text-base font-semibold text-evergreen-900">{t("driverPortal.registrationTitle")}</h3>
      <p className="mb-4 text-xs text-ink-300">{t("driverPortal.registrationHint")}</p>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("driverPortal.fullName")}</label>
          <input className="input" value={fullName} onChange={(e) => setFullName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("driverPortal.phone")}</label>
          <input className="input" value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="09XXXXXXXX" />
        </div>
        <div>
          <label className="label">{t("driverPortal.plateNumber")}</label>
          <input className="input" value={plateNumber} onChange={(e) => setPlateNumber(e.target.value)} placeholder="AA 12345" />
        </div>
        <div>
          <label className="label">{t("driverPortal.truckType")}</label>
          <select className="input" value={truckType} onChange={(e) => setTruckType(e.target.value as TruckType)}>
            {TRUCK_TYPES.map((tt) => <option key={tt} value={tt}>{t(`driverPortal.truckTypes.${tt}`)}</option>)}
          </select>
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <div className="mt-4 flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving || !fullName}>
          {saving ? t("common.saving") : t("driverPortal.register")}
        </button>
        {profile && <button className="btn-secondary" onClick={() => setEditing(false)}>{t("common.cancel")}</button>}
      </div>
    </div>
  );
}

function AvailableTripsBoard() {
  const { t } = useTranslation();
  const { data: trips, loading, error, reload } = useFetch<TripDto[]>("/drivers/trips/available");
  const [acceptingId, setAcceptingId] = useState<string | null>(null);
  const [acceptError, setAcceptError] = useState<string | null>(null);

  async function accept(deliveryId: string) {
    setAcceptingId(deliveryId);
    setAcceptError(null);
    try {
      await apiClient.post(`/drivers/trips/${deliveryId}/accept`);
      reload();
    } catch (err) {
      setAcceptError(extractErrorMessage(err));
    } finally {
      setAcceptingId(null);
    }
  }

  return (
    <div>
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-evergreen-900">{t("driverPortal.availableTrips")}</h3>
        <button className="btn-secondary px-2 py-1 text-xs" onClick={reload}>
          <RefreshCw className="h-3.5 w-3.5" /> {t("common.refresh")}
        </button>
      </div>

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {acceptError && <div className="mb-3"><ErrorState message={acceptError} /></div>}
      {trips && trips.length === 0 && <EmptyState title={t("driverPortal.noTripsAvailable")} />}

      <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
        {trips?.map((trip) => {
          const needsConfirmation = trip.driverId != null && !trip.driverConfirmed;
          return (
            <div key={trip.deliveryId} className="card flex items-center justify-between gap-3 p-4">
              <div className="min-w-0 flex-1">
                {needsConfirmation && (
                  <p className="mb-1 inline-block rounded bg-wheat-100 px-1.5 py-0.5 text-[10px] font-medium uppercase tracking-wide text-wheat-600">
                    {t("driverPortal.assignedToYou")}
                  </p>
                )}
                <p className="flex items-center gap-1.5 text-sm font-medium text-evergreen-900">
                  <MapPin className="h-3.5 w-3.5 shrink-0 text-evergreen-600" />
                  <span className="truncate">{trip.originLocation ?? "-"}</span>
                  <span className="text-ink-300">&rarr;</span>
                  <span className="truncate">{trip.destinationName}</span>
                </p>
                <p className="mt-1 flex items-center gap-1.5 text-xs text-ink-300">
                  <PackageIcon className="h-3.5 w-3.5" /> {trip.productSummary}
                  <span>&middot; {new Date(trip.orderDate).toLocaleDateString()}</span>
                </p>
              </div>
              <div className="shrink-0 text-right">
                <p className="text-sm font-semibold text-evergreen-800">
                  {trip.tripPrice != null ? `${trip.tripPrice.toFixed(2)} ${t("common.currency")}` : t("driverPortal.priceTbd")}
                </p>
                <button
                  className="btn-primary mt-1 px-3 py-1.5 text-xs"
                  onClick={() => accept(trip.deliveryId)}
                  disabled={acceptingId === trip.deliveryId}
                >
                  {acceptingId === trip.deliveryId
                    ? t("common.saving")
                    : needsConfirmation ? t("driverPortal.confirmTrip") : t("driverPortal.acceptTrip")}
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function MyTripsList() {
  const { t } = useTranslation();
  const { data: trips, loading, error } = useFetch<TripDto[]>("/drivers/trips/mine");

  return (
    <div>
      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {trips && trips.length === 0 && <EmptyState title={t("driverPortal.noTripsYet")} />}

      {trips && trips.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("driverPortal.route")}</th>
                <th>{t("driverPortal.load")}</th>
                <th>{t("common.status")}</th>
                <th>{t("driverPortal.price")}</th>
              </tr>
            </thead>
            <tbody>
              {trips.map((trip) => (
                <tr key={trip.deliveryId}>
                  <td>{trip.originLocation ?? "-"} &rarr; {trip.destinationName}</td>
                  <td>{trip.productSummary}</td>
                  <td>
                    {trip.status}
                    {!trip.driverConfirmed && <span className="ml-1 text-xs text-wheat-600">({t("driverPortal.unconfirmed")})</span>}
                  </td>
                  <td>{trip.tripPrice != null ? `${trip.tripPrice.toFixed(2)} ${t("common.currency")}` : "-"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
