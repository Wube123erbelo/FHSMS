import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Sprout } from "lucide-react";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";

export default function ResetPasswordPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [token, setToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await apiClient.post("/auth/reset-password", { email, token, newPassword });
      navigate("/login", { replace: true, state: { resetSuccess: true } });
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-evergreen-900 px-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center text-center">
          <Sprout className="mb-3 h-9 w-9 text-wheat-400" strokeWidth={1.5} />
          <h1 className="font-display text-2xl text-white">{t("auth.resetPassword")}</h1>
        </div>

        <form onSubmit={submit} className="card space-y-4 p-6">
          <div>
            <label className="label" htmlFor="email">{t("login.email")}</label>
            <input id="email" type="email" required className="input" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>
          <div>
            <label className="label" htmlFor="token">{t("auth.resetToken")}</label>
            <input id="token" required className="input" value={token} onChange={(e) => setToken(e.target.value)} />
          </div>
          <div>
            <label className="label" htmlFor="newPassword">{t("auth.newPassword")}</label>
            <input id="newPassword" type="password" required minLength={8} className="input" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
          </div>

          {error && <p className="text-sm text-clay-600">{error}</p>}

          <button type="submit" disabled={loading} className="btn-primary w-full">
            {loading ? t("common.saving") : t("auth.resetPassword")}
          </button>

          <Link to="/login" className="block text-center text-xs text-evergreen-700 underline underline-offset-2">
            {t("auth.backToLogin")}
          </Link>
        </form>
      </div>
    </div>
  );
}
