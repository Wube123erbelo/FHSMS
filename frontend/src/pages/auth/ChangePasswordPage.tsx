import { useState, type FormEvent } from "react";
import PageHeader from "../../components/PageHeader";
import { ErrorState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";

export default function ChangePasswordPage() {
  const { t } = useTranslation();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [saving, setSaving] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSuccess(false);

    if (newPassword !== confirmPassword) {
      setError(t("auth.passwordMismatch"));
      return;
    }

    setSaving(true);
    try {
      await apiClient.post("/auth/change-password", { currentPassword, newPassword });
      setSuccess(true);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div>
      <PageHeader title={t("auth.changePassword")} />

      <form onSubmit={submit} className="card max-w-md space-y-4 p-6">
        <div>
          <label className="label">{t("auth.currentPassword")}</label>
          <input type="password" required className="input" value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("auth.newPassword")}</label>
          <input type="password" required minLength={8} className="input" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("auth.confirmNewPassword")}</label>
          <input type="password" required minLength={8} className="input" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} />
        </div>

        {error && <ErrorState message={error} />}
        {success && <p className="text-sm text-evergreen-700">{t("auth.passwordChanged")}</p>}

        <button type="submit" disabled={saving} className="btn-primary">
          {saving ? t("common.saving") : t("common.save")}
        </button>
      </form>
    </div>
  );
}
