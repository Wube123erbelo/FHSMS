import { useState } from "react";
import PageHeader from "../../components/PageHeader";
import BackLink from "../../components/BackLink";
import { LoadingState, ErrorState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { TaxCalculationMode, TaxConfigurationDto, TaxRateHistoryDto, TaxType } from "../../api/types";

/**
 * Backs Settings -> Tax & VAT exactly as specced: an on/off switch, a
 * configurable rate with an effective date, inclusive/exclusive mode, and a
 * full history of past rates. Nothing here is hard-coded - every value shown
 * comes straight from the API.
 */
export default function TaxSettingsPage() {
  const { t } = useTranslation();
  const { data: configs, loading, error, reload } = useFetch<TaxConfigurationDto[]>("/tax");

  return (
    <div>
      <BackLink to="/settings" />
      <PageHeader eyebrow={t("tax.eyebrow")} title={t("tax.title")} description={t("tax.description")} />

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}

      {configs && configs.length > 0 && (
        <div className="space-y-6">
          {configs.map((config) => (
            <TaxConfigCard key={config.id} config={config} onChanged={reload} />
          ))}
        </div>
      )}

      {configs && configs.length === 0 && <NewTaxConfigForm onCreated={reload} />}
    </div>
  );
}

function TaxConfigCard({ config, onChanged }: { config: TaxConfigurationDto; onChanged: () => void }) {
  const { t } = useTranslation();
  const [toggling, setToggling] = useState(false);
  const [showSchedule, setShowSchedule] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function toggle() {
    setToggling(true);
    setError(null);
    try {
      await apiClient.post(`/tax/${config.id}/toggle`, { isEnabled: !config.isEnabled });
      onChanged();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setToggling(false);
    }
  }

  return (
    <div className="card p-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="font-mono text-xs uppercase tracking-widest text-wheat-600">{config.taxType}</p>
          <h3 className="text-lg font-semibold text-evergreen-900">{config.name}</h3>
          <p className="mt-1 text-sm text-ink-600">
            {config.calculationMode === "Inclusive" ? t("tax.priceIncludesTax") : t("tax.taxAddedOnTop")} &middot;{" "}
            {config.exemptionAllowed ? t("tax.exemptionsAllowed") : t("tax.noExemptions")}
          </p>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-sm text-ink-600">{t("tax.vatEnabled")}</span>
          <button
            onClick={toggle}
            disabled={toggling}
            role="switch"
            aria-checked={config.isEnabled}
            className={`relative h-6 w-11 rounded-full transition ${config.isEnabled ? "bg-evergreen-600" : "bg-ink-900/15"}`}
          >
            <span
              className={`absolute top-0.5 h-5 w-5 rounded-full bg-white shadow transition ${
                config.isEnabled ? "left-5" : "left-0.5"
              }`}
            />
          </button>
        </div>
      </div>

      <div className="mt-5 grid grid-cols-2 gap-4 sm:grid-cols-4">
        <Stat label={t("tax.currentRate")} value={config.currentRate !== undefined ? `${config.currentRate}%` : "-"} />
        <Stat
          label={t("tax.effectiveFrom")}
          value={config.currentRateEffectiveFrom ? new Date(config.currentRateEffectiveFrom).toLocaleDateString() : "-"}
        />
        <Stat label={t("tax.mode")} value={config.calculationMode === "Inclusive" ? t("tax.inclusive") : t("tax.exclusive")} />
        <Stat label={t("tax.status")} value={config.isEnabled ? t("common.active") : t("common.inactive")} />
      </div>

      {config.pendingRate !== undefined && config.pendingRateEffectiveFrom && (
        <div className="mt-4 rounded-md border border-wheat-200 bg-wheat-50 px-4 py-3 text-sm text-wheat-800">
          A {config.pendingRate}% rate is already scheduled to take effect on{" "}
          {new Date(config.pendingRateEffectiveFrom).toLocaleDateString()}. Any new rate you schedule must take
          effect after that date.
        </div>
      )}

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <div className="mt-5 flex flex-wrap gap-2 border-t border-evergreen-100 pt-4">
        <button className="btn-secondary" onClick={() => setShowSchedule((s) => !s)}>
          {t("tax.scheduleNewRate")}
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

      {showHistory && <RateHistory taxConfigurationId={config.id} />}
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

function ScheduleRateForm({ config, onDone }: { config: TaxConfigurationDto; onDone: () => void }) {
  const { t } = useTranslation();
  const [rate, setRate] = useState(config.currentRate ?? 15);
  const [effectiveFrom, setEffectiveFrom] = useState(new Date().toISOString().slice(0, 10));
  const [mode, setMode] = useState<TaxCalculationMode>(config.calculationMode);
  const [exemptionAllowed, setExemptionAllowed] = useState(config.exemptionAllowed);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/tax/configure", {
        existingTaxConfigurationId: config.id,
        name: config.name,
        taxType: config.taxType,
        isEnabled: config.isEnabled,
        calculationMode: mode,
        exemptionAllowed,
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
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("tax.newRate")}</label>
          <input type="number" min={0} max={100} step={0.5} className="input" value={rate}
            onChange={(e) => setRate(Number(e.target.value))} />
        </div>
        <div>
          <label className="label">{t("tax.effectiveFrom")}</label>
          <input type="date" className="input" value={effectiveFrom} onChange={(e) => setEffectiveFrom(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("tax.calculationMode")}</label>
          <select className="input" value={mode} onChange={(e) => setMode(e.target.value as TaxCalculationMode)}>
            <option value="Exclusive">{t("tax.exclusive")}</option>
            <option value="Inclusive">{t("tax.inclusive")}</option>
          </select>
        </div>
        <div className="flex items-end gap-2">
          <input id="exempt" type="checkbox" checked={exemptionAllowed} onChange={(e) => setExemptionAllowed(e.target.checked)} />
          <label htmlFor="exempt" className="text-sm text-ink-600">{t("tax.allowExemptions")}</label>
        </div>
      </div>

      {error && <div className="mt-3"><ErrorState message={error} /></div>}

      <div className="mt-4 flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving}>
          {saving ? t("common.saving") : t("tax.scheduleRate")}
        </button>
      </div>
    </div>
  );
}

function RateHistory({ taxConfigurationId }: { taxConfigurationId: string }) {
  const { t } = useTranslation();
  const { data, loading, error } = useFetch<TaxRateHistoryDto[]>(`/tax/${taxConfigurationId}/history`);

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

function NewTaxConfigForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState("Value Added Tax");
  const [taxType, setTaxType] = useState<TaxType>("Vat");
  const [isEnabled, setIsEnabled] = useState(true);
  const [mode, setMode] = useState<TaxCalculationMode>("Exclusive");
  const [exemptionAllowed, setExemptionAllowed] = useState(true);
  const [rate, setRate] = useState(15);
  const [effectiveFrom, setEffectiveFrom] = useState(new Date().toISOString().slice(0, 10));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/tax/configure", {
        name, taxType, isEnabled, calculationMode: mode, exemptionAllowed, rate,
        effectiveFrom: new Date(effectiveFrom).toISOString()
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
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("tax.noConfigTitle")}</h3>
      <div className="space-y-4">
        <div>
          <label className="label">{t("common.name")}</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="label">{t("tax.taxType")}</label>
            <select className="input" value={taxType} onChange={(e) => setTaxType(e.target.value as TaxType)}>
              <option value="Vat">{t("tax.typeVat")}</option>
              <option value="SalesTax">{t("tax.typeSalesTax")}</option>
              <option value="WithholdingTax">{t("tax.typeWithholdingTax")}</option>
              <option value="Other">{t("tax.typeOther")}</option>
            </select>
          </div>
          <div>
            <label className="label">{t("tax.newRate")}</label>
            <input type="number" className="input" value={rate} onChange={(e) => setRate(Number(e.target.value))} />
          </div>
          <div>
            <label className="label">{t("tax.mode")}</label>
            <select className="input" value={mode} onChange={(e) => setMode(e.target.value as TaxCalculationMode)}>
              <option value="Exclusive">{t("tax.exclusive")}</option>
              <option value="Inclusive">{t("tax.inclusive")}</option>
            </select>
          </div>
          <div>
            <label className="label">{t("tax.effectiveFrom")}</label>
            <input type="date" className="input" value={effectiveFrom} onChange={(e) => setEffectiveFrom(e.target.value)} />
          </div>
        </div>
        <div className="flex items-center gap-4">
          <label className="flex items-center gap-2 text-sm text-ink-600">
            <input type="checkbox" checked={isEnabled} onChange={(e) => setIsEnabled(e.target.checked)} /> {t("tax.enabled")}
          </label>
          <label className="flex items-center gap-2 text-sm text-ink-600">
            <input type="checkbox" checked={exemptionAllowed} onChange={(e) => setExemptionAllowed(e.target.checked)} /> {t("tax.allowExemptions")}
          </label>
        </div>

        {error && <ErrorState message={error} />}

        <button className="btn-primary" onClick={submit} disabled={saving}>
          {saving ? t("common.creating") : t("tax.createConfig")}
        </button>
      </div>
    </div>
  );
}
