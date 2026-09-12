import { useState } from "react";
import PageHeader from "../../components/PageHeader";
import BackLink from "../../components/BackLink";
import { LoadingState, ErrorState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { PlatformCommissionConfigurationDto, PlatformCommissionRateHistoryDto } from "../../api/types";

/**
 * Backs Settings -> Total Commission: the 2% the company charges hotels on
 * every sale, on top of the agent's own bonus. Same pattern as Tax & VAT -
 * an on/off switch, a configurable rate with an effective date, and a full
 * history of past rates. Every invoice generated after a change here picks
 * up the new rate automatically; invoices already issued keep whatever rate
 * applied when they were generated.
 */
export default function TotalCommissionSettingsPage() {
  const { t } = useTranslation();
  const { data: config, loading, error, reload } = useFetch<PlatformCommissionConfigurationDto | null>("/platformcommission");

  return (
    <div>
      <BackLink to="/settings" />
      <PageHeader
        eyebrow={t("commissionSettings.eyebrow")}
        title={t("commissionSettings.title")}
        description={t("commissionSettings.description")}
      />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}

      {config && <CommissionConfigCard config={config} onChanged={reload} />}
      {!loading && !error && !config && <NewCommissionConfigForm onCreated={reload} />}
    </div>
  );
}

function CommissionConfigCard({ config, onChanged }: { config: PlatformCommissionConfigurationDto; onChanged: () => void }) {
  const { t } = useTranslation();
  const [showSchedule, setShowSchedule] = useState(false);
  const [showHistory, setShowHistory] = useState(false);

  return (
    <div className="card p-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="font-mono text-xs uppercase tracking-widest text-wheat-600">{t("commissionSettings.platformCommission")}</p>
          <h3 className="text-lg font-semibold text-evergreen-900">{config.name}</h3>
          <p className="mt-1 text-sm text-ink-600">{t("commissionSettings.chargedOnEverySale")}</p>
        </div>
        <span
          className={`rounded-full px-3 py-1 text-xs font-semibold ${
            config.isEnabled ? "bg-evergreen-50 text-evergreen-700" : "bg-ink-900/5 text-ink-400"
          }`}
        >
          {config.isEnabled ? t("common.active") : t("common.inactive")}
        </span>
      </div>

      <div className="mt-5 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <Stat label={t("commissionSettings.currentRate")} value={config.currentRate !== undefined ? `${config.currentRate}%` : "-"} />
        <Stat
          label={t("tax.effectiveFrom")}
          value={config.currentRateEffectiveFrom ? new Date(config.currentRateEffectiveFrom).toLocaleDateString() : "-"}
        />
        <Stat label={t("tax.status")} value={config.isEnabled ? t("common.active") : t("common.inactive")} />
      </div>

      {config.pendingRate !== undefined && config.pendingRateEffectiveFrom && (
        <div className="mt-4 rounded-md border border-wheat-200 bg-wheat-50 px-4 py-3 text-sm text-wheat-800">
          A {config.pendingRate}% rate is already scheduled to take effect on{" "}
          {new Date(config.pendingRateEffectiveFrom).toLocaleDateString()}. Any new rate you schedule must take
          effect after that date.
        </div>
      )}

      <div className="mt-5 flex flex-wrap gap-2 border-t border-evergreen-100 pt-4">
        <button className="btn-secondary" onClick={() => setShowSchedule((s) => !s)}>
          {t("commissionSettings.scheduleNewRate")}
        </button>
        <button className="btn-secondary" onClick={() => setShowHistory((s) => !s)}>
          {showHistory ? t("tax.hideHistory") : t("tax.viewHistory")}
        </button>
      </div>

      {showSchedule && (
        <ScheduleRateForm
          config={config}
          onDone={() => {
            setShowSchedule(false);
            onChanged();
          }}
        />
      )}

      {showHistory && <RateHistory configurationId={config.id} />}
    </div>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-ink-300">{label}</p>
      <p className="mt-0.5 text-sm font-medium text-ink-900">{value}</p>
    </div>
  );
}

function ScheduleRateForm({ config, onDone }: { config: PlatformCommissionConfigurationDto; onDone: () => void }) {
  const { t } = useTranslation();
  const [rate, setRate] = useState(config.currentRate ?? 2);
  const [isEnabled, setIsEnabled] = useState(config.isEnabled);
  const [effectiveFrom, setEffectiveFrom] = useState(new Date().toISOString().slice(0, 10));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/platformcommission/configure", {
        existingConfigurationId: config.id,
        name: config.name,
        isEnabled,
        rate,
        effectiveFrom: new Date(effectiveFrom).toISOString()
      });
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="mt-4 rounded-md border border-evergreen-100 bg-evergreen-50/40 p-4">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label className="label">{t("commissionSettings.newRate")}</label>
          <input type="number" min={0} max={100} step={0.1} className="input" value={rate}
            onChange={(e) => setRate(Number(e.target.value))} />
        </div>
        <div>
          <label className="label">{t("tax.effectiveFrom")}</label>
          <input type="date" className="input" value={effectiveFrom} onChange={(e) => setEffectiveFrom(e.target.value)} />
        </div>
        <div className="flex items-end gap-2">
          <input id="commission-enabled" type="checkbox" checked={isEnabled} onChange={(e) => setIsEnabled(e.target.checked)} />
          <label htmlFor="commission-enabled" className="text-sm text-ink-600">{t("tax.enabled")}</label>
        </div>
      </div>

      {error && <div className="mt-3"><ErrorState message={error} /></div>}

      <div className="mt-4 flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving}>
          {saving ? t("common.saving") : t("commissionSettings.scheduleRate")}
        </button>
      </div>
    </div>
  );
}

function RateHistory({ configurationId }: { configurationId: string }) {
  const { t } = useTranslation();
  const { data, loading, error } = useFetch<PlatformCommissionRateHistoryDto[]>(`/platformcommission/${configurationId}/history`);

  return (
    <div className="mt-4">
      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {data && (
        <div className="overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("tax.rate")}</th>
                <th>{t("tax.effectiveFrom")}</th>
                <th>{t("tax.effectiveTo")}</th>
              </tr>
            </thead>
            <tbody>
              {data.map((r) => (
                <tr key={r.id}>
                  <td>{r.rate}%</td>
                  <td>{new Date(r.effectiveFrom).toLocaleDateString()}</td>
                  <td>{r.effectiveTo ? new Date(r.effectiveTo).toLocaleDateString() : t("tax.current")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function NewCommissionConfigForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState("Total Commission");
  const [isEnabled, setIsEnabled] = useState(true);
  const [rate, setRate] = useState(2);
  const [effectiveFrom, setEffectiveFrom] = useState(new Date().toISOString().slice(0, 10));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/platformcommission/configure", {
        name, isEnabled, rate, effectiveFrom: new Date(effectiveFrom).toISOString()
      });
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card max-w-xl p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("commissionSettings.noConfigTitle")}</h3>
      <div className="space-y-4">
        <div>
          <label className="label">{t("common.name")}</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="label">{t("commissionSettings.newRate")}</label>
            <input type="number" className="input" value={rate} onChange={(e) => setRate(Number(e.target.value))} />
          </div>
          <div>
            <label className="label">{t("tax.effectiveFrom")}</label>
            <input type="date" className="input" value={effectiveFrom} onChange={(e) => setEffectiveFrom(e.target.value)} />
          </div>
        </div>
        <label className="flex items-center gap-2 text-sm text-ink-600">
          <input type="checkbox" checked={isEnabled} onChange={(e) => setIsEnabled(e.target.checked)} /> {t("tax.enabled")}
        </label>

        {error && <ErrorState message={error} />}

        <button className="btn-primary" onClick={submit} disabled={saving}>
          {saving ? t("common.creating") : t("commissionSettings.createConfig")}
        </button>
      </div>
    </div>
  );
}
