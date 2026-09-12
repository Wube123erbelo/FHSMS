import { useState } from "react";
import { Plus, X } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import BackLink from "../../components/BackLink";
import ExportButtons from "../../components/ExportButtons";
import StatusBadge from "../../components/StatusBadge";
import { ErrorState, EmptyState, LoadingState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { AgentType, CommissionBasis, CommissionDto, CommissionRuleDto, UnitDto, UserDto } from "../../api/types";

export default function CommissionsPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const isAdmin = role === "SuperAdmin";
  const { data: rules, loading: rulesLoading, error: rulesError, reload: reloadRules } = useFetch<CommissionRuleDto[]>("/commissions/rules");
  const { data: units } = useFetch<UnitDto[]>("/units");
  const { data: allUsers } = useFetch<UserDto[]>(isAdmin ? "/users" : null);
  const agentUsers = (allUsers ?? []).filter((u) => u.role === "HotelAgent" || u.role === "FarmerAgent");

  const [agentUserId, setAgentUserId] = useState("");
  const { data: lookedUpCommissions, error } = useFetch<CommissionDto[]>(
    agentUserId ? `/commissions/agents/${agentUserId}` : null, [agentUserId]
  );

  // Agents see their own commissions automatically - no ID to type in, no
  // way to accidentally (or otherwise) look up someone else's. Only admins
  // get the manual "look up any agent" tool below.
  const { data: myCommissions } = useFetch<CommissionDto[]>(!isAdmin ? "/commissions/mine" : null);
  const commissions = isAdmin ? lookedUpCommissions : myCommissions;

  return (
    <div>
      {isAdmin && <BackLink to="/settings" />}
      <PageHeader title={t("commissions.title")} description={t("commissions.description")} />

      {isAdmin && (
        <div className="mb-6 grid grid-cols-1 gap-4 lg:grid-cols-2">
          <RuleCard
            agentType="HotelAgent"
            rule={rules?.find((r) => r.agentType === "HotelAgent") ?? null}
            units={units ?? []}
            loading={rulesLoading}
            onChanged={reloadRules}
          />
          <RuleCard
            agentType="FarmerAgent"
            rule={rules?.find((r) => r.agentType === "FarmerAgent") ?? null}
            units={units ?? []}
            loading={rulesLoading}
            onChanged={reloadRules}
          />
        </div>
      )}
      {rulesError && <ErrorState message={rulesError} />}

      {isAdmin && (
        <div className="card my-6 max-w-lg p-6">
          <label className="label">{t("commissions.agentUserId")}</label>
          <div className="flex gap-2">
            <select className="input" value={agentUserId} onChange={(e) => setAgentUserId(e.target.value)}>
              <option value="">{t("commissions.selectAgent")}</option>
              {agentUsers.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.code ?? u.id} - {u.fullName} ({u.role})
                </option>
              ))}
            </select>
          </div>
          {error && <div className="mt-3"><ErrorState message={error} /></div>}
        </div>
      )}

      {isAdmin && agentUserId && lookedUpCommissions && lookedUpCommissions.length > 0 && (
        <div className="mb-4 flex flex-wrap items-center justify-between gap-3 rounded-md border border-evergreen-100 bg-evergreen-50/50 px-4 py-3 text-sm">
          <div>
            <span className="font-medium text-evergreen-900">{t("commissions.totalOwed")}: </span>
            <span className="font-semibold text-evergreen-900">
              {lookedUpCommissions.filter((c) => c.status !== "Cancelled").reduce((sum, c) => sum + c.commissionAmount, 0).toFixed(2)} {t("common.currency")}
            </span>
          </div>
          <ExportButtons basePath={`/commissions/agents/${agentUserId}`} filenameBase="commissions" />
        </div>
      )}

      {commissions && commissions.length === 0 && <EmptyState title={t("commissions.emptyTitle")} />}

      {commissions && commissions.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("commissions.source")}</th>
                <th>{t("commissions.baseAmount")}</th>
                <th>{t("commissions.commission")}</th>
                <th>{t("common.status")}</th>
              </tr>
            </thead>
            <tbody>
              {commissions.map((c) => (
                <tr key={c.id}>
                  <td className="font-mono text-xs">
                    {c.sourceType === "Invoice" ? t("commissions.invoice") : t("commissions.stockReceipt")}: {c.invoiceId ?? c.inventoryTransactionId}
                  </td>
                  <td>
                    {c.basis === "PercentageOfInvoice"
                      ? `${c.baseAmount.toFixed(2)} ${t("common.currency")} \u00d7 ${c.percentage}%`
                      : `${c.baseAmount} \u00d7 ${c.flatRateAmount?.toFixed(2)} ${t("common.currency")}`}
                  </td>
                  <td className="font-medium">{c.commissionAmount.toFixed(2)} {t("common.currency")}</td>
                  <td><StatusBadge status={c.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function RuleCard({
  agentType, rule, units, loading, onChanged
}: { agentType: AgentType; rule: CommissionRuleDto | null; units: UnitDto[]; loading: boolean; onChanged: () => void }) {
  const { t } = useTranslation();
  const [basis, setBasis] = useState<CommissionBasis>(rule?.basis ?? (agentType === "HotelAgent" ? "PercentageOfInvoice" : "FlatRatePerQuantity"));
  const [percentage, setPercentage] = useState(rule?.percentage ?? 2);
  const [flatRateAmount, setFlatRateAmount] = useState(rule?.flatRateAmount ?? 0.5);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  // Re-sync local edit state whenever the rule from the server changes underneath us.
  const [syncedRuleId, setSyncedRuleId] = useState(rule?.id);
  if (rule && rule.id !== syncedRuleId) {
    setSyncedRuleId(rule.id);
    setBasis(rule.basis);
    if (rule.percentage != null) setPercentage(rule.percentage);
    if (rule.flatRateAmount != null) setFlatRateAmount(rule.flatRateAmount);
  }

  async function submit() {
    setSaving(true);
    setError(null);
    setSaved(false);
    try {
      await apiClient.post("/commissions/rules", {
        agentType, basis,
        percentage: basis === "PercentageOfInvoice" ? percentage : null,
        flatRateAmount: basis === "FlatRatePerQuantity" ? flatRateAmount : null
      });
      setSaved(true);
      onChanged();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  async function addUnitRate(unitId: string, rateAmount: number) {
    if (!rule) return;
    await apiClient.put(`/commissions/rules/${rule.id}/unit-rates/${unitId}`, { rateAmount });
    onChanged();
  }

  async function removeUnitRate(unitId: string) {
    if (!rule) return;
    await apiClient.delete(`/commissions/rules/${rule.id}/unit-rates/${unitId}`);
    onChanged();
  }

  return (
    <div className="card p-6">
      <h3 className="mb-1 text-base font-semibold text-evergreen-900">
        {t("commissions.rulesTitle")}: {agentType === "HotelAgent" ? t("commissions.hotelAgent") : t("commissions.farmerAgent")}
      </h3>
      {loading ? (
        <LoadingState label={t("common.loading")} />
      ) : (
        <>
          <div className="mt-3">
            <label className="label">{t("commissions.basis")}</label>
            <select className="input" value={basis} onChange={(e) => setBasis(e.target.value as CommissionBasis)}>
              <option value="PercentageOfInvoice">{t("commissions.basisPercentage")}</option>
              <option value="FlatRatePerQuantity">{t("commissions.basisFlatRate")}</option>
            </select>
          </div>

          {basis === "PercentageOfInvoice" ? (
            <div className="mt-3">
              <label className="label">{t("commissions.percentage")}</label>
              <input type="number" step="0.01" className="input" value={percentage} onChange={(e) => setPercentage(Number(e.target.value))} />
              <p className="mt-1 text-[11px] text-ink-300">{t("commissions.percentageOfTotal")}</p>
            </div>
          ) : (
            <>
              <div className="mt-3">
                <label className="label">{t("commissions.flatRateAmount")}</label>
                <input type="number" step="0.01" className="input" value={flatRateAmount} onChange={(e) => setFlatRateAmount(Number(e.target.value))} />
                <p className="mt-1 text-[11px] text-ink-300">{t("commissions.flatRateHint")}</p>
              </div>

              {rule && (
                <div className="mt-4 rounded-md border border-evergreen-100 p-3">
                  <p className="text-xs font-semibold text-evergreen-900">{t("commissions.unitRatesTitle")}</p>
                  <p className="mt-0.5 text-[11px] text-ink-300">{t("commissions.unitRatesHint")}</p>
                  <div className="mt-2 space-y-1">
                    {rule.unitRates.map((ur) => (
                      <div key={ur.unitId} className="flex items-center justify-between text-sm">
                        <span>{ur.unitName} ({ur.unitAbbreviation})</span>
                        <span className="flex items-center gap-2">
                          {ur.rateAmount.toFixed(2)} {t("common.currency")}
                          <button onClick={() => removeUnitRate(ur.unitId)} className="text-clay-600"><X className="h-3.5 w-3.5" /></button>
                        </span>
                      </div>
                    ))}
                  </div>
                  <AddUnitRateRow
                    units={units.filter((u) => !rule.unitRates.some((ur) => ur.unitId === u.id))}
                    onAdd={addUnitRate}
                  />
                </div>
              )}
            </>
          )}

          {error && <div className="mt-4"><ErrorState message={error} /></div>}
          {saved && <p className="mt-3 text-sm text-evergreen-700">{t("commissions.ruleSaved")}</p>}
          <button className="btn-primary mt-4" onClick={submit} disabled={saving}>
            {saving ? t("common.saving") : t("commissions.saveRule")}
          </button>
        </>
      )}
    </div>
  );
}

function AddUnitRateRow({ units, onAdd }: { units: UnitDto[]; onAdd: (unitId: string, rate: number) => void }) {
  const { t } = useTranslation();
  const [unitId, setUnitId] = useState("");
  const [rate, setRate] = useState(0);

  return (
    <div className="mt-3 flex items-end gap-2">
      <div className="flex-1">
        <label className="label text-[11px]">{t("commissions.unit")}</label>
        <select className="input" value={unitId} onChange={(e) => setUnitId(e.target.value)}>
          <option value="">-</option>
          {units.map((u) => <option key={u.id} value={u.id}>{u.name} ({u.abbreviation})</option>)}
        </select>
      </div>
      <div className="w-28">
        <label className="label text-[11px]">{t("commissions.rate")}</label>
        <input type="number" step="0.01" className="input" value={rate} onChange={(e) => setRate(Number(e.target.value))} />
      </div>
      <button
        className="btn-secondary px-2 py-2"
        disabled={!unitId}
        onClick={() => { onAdd(unitId, rate); setUnitId(""); setRate(0); }}
      >
        <Plus className="h-4 w-4" />
      </button>
    </div>
  );
}
