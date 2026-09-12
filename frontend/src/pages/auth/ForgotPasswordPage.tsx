import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { Sprout } from "lucide-react";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";

export default function ForgotPasswordPage() {
  const { t } = useTranslation();
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await apiClient.post("/auth/forgot-password", { email });
      setSent(true);
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
          <h1 className="font-display text-2xl text-white">{t("auth.forgotPassword")}</h1>
        </div>

        <form onSubmit={submit} className="card space-y-4 p-6">
          <div>
            <label className="label" htmlFor="email">{t("login.email")}</label>
            <input id="email" type="email" required className="input" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>

          {sent && <p className="text-sm text-evergreen-700">{t("auth.resetLinkSent")}</p>}
          {error && <p className="text-sm text-clay-600">{error}</p>}

          <button type="submit" disabled={loading} className="btn-primary w-full">
            {loading ? t("common.saving") : t("auth.sendResetLink")}
          </button>

          <div className="flex items-center justify-between text-xs">
            <Link to="/login" className="text-evergreen-700 underline underline-offset-2">{t("auth.backToLogin")}</Link>
            <Link to="/reset-password" className="text-evergreen-700 underline underline-offset-2">{t("auth.resetPassword")}</Link>
          </div>
        </form>
      </div>
    </div>
  );
}
