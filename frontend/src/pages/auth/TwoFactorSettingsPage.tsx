import { useState } from "react";
import PageHeader from "../../components/PageHeader";
import BackLink from "../../components/BackLink";
import { ErrorState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { SetupTwoFactorResult } from "../../api/types";

/**
 * No QR image library needed - browsers can render a QR code by pointing an
 * <img> at a public QR-generation endpoint, but to avoid any external network
 * dependency at all we show the otpauth:// URI and secret as text/copy
 * instead. Any authenticator app supports manual entry via the secret.
 */
export default function TwoFactorSettingsPage() {
  const { t } = useTranslation();
  const [setup, setSetup] = useState<SetupTwoFactorResult | null>(null);
  const [code, setCode] = useState("");
  const [enabled, setEnabled] = useState(false);
  const [currentPassword, setCurrentPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function startSetup() {
    setBusy(true);
    setError(null);
    try {
      const { data } = await apiClient.post<SetupTwoFactorResult>("/auth/2fa/setup");
      setSetup(data);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  async function confirmSetup() {
    setBusy(true);
    setError(null);
    try {
      await apiClient.post("/auth/2fa/confirm", { code });
      setEnabled(true);
      setSetup(null);
      setCode("");
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  async function disable() {
    setBusy(true);
    setError(null);
    try {
      await apiClient.post("/auth/2fa/disable", { currentPassword });
      setEnabled(false);
      setCurrentPassword("");
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div>
      <BackLink to="/settings" />
      <PageHeader title={t("auth.twoFactorTitle")} description={t("auth.twoFactorDescription")} />

      {error && <div className="mb-4"><ErrorState message={error} /></div>}

      {enabled ? (
        <div className="card max-w-md p-6">
          <p className="mb-4 text-sm text-evergreen-700">{t("auth.twoFactorEnabled")}</p>
          <label className="label">{t("auth.currentPassword")}</label>
          <input type="password" className="input mb-3" value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} />
          <button className="btn-secondary text-clay-600" onClick={disable} disabled={busy || !currentPassword}>
            {t("auth.disableTwoFactor")}
          </button>
        </div>
      ) : setup ? (
        <div className="card max-w-md p-6">
          <p className="mb-3 text-sm text-ink-600">{t("auth.scanOrEnter")}</p>
          <div className="mb-3 rounded-md bg-ink-900/5 p-3 font-mono text-xs break-all">{setup.secret}</div>
          <p className="mb-4 break-all text-xs text-ink-300">{setup.otpAuthUri}</p>
          <label className="label">{t("auth.twoFactorCode")}</label>
          <input
            className="input mb-3 text-center font-mono text-lg tracking-widest"
            inputMode="numeric"
            maxLength={6}
            value={code}
            onChange={(e) => setCode(e.target.value)}
          />
          <button className="btn-primary" onClick={confirmSetup} disabled={busy || code.length !== 6}>
            {busy ? t("common.saving") : t("auth.verifyAndSignIn")}
          </button>
        </div>
      ) : (
        <div className="card max-w-md p-6">
          <p className="mb-4 text-sm text-ink-600">{t("auth.twoFactorDisabledHint")}</p>
          <button className="btn-primary" onClick={startSetup} disabled={busy}>
            {t("auth.enableTwoFactor")}
          </button>
        </div>
      )}
    </div>
  );
}
