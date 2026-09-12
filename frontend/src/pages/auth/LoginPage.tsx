import { useState, type FormEvent } from "react";
import { Navigate, useNavigate, Link } from "react-router-dom";
import { Sprout, ShieldCheck, KeyRound } from "lucide-react";
import { useAuth } from "../../context/AuthContext";
import { extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import LanguageSwitcher from "../../components/LanguageSwitcher";

/**
 * Two logins, one screen: the "Staff & Customer" tab hits POST /auth/login
 * (any active role), the "Admin" tab hits POST /auth/admin-login, which the
 * backend rejects for anything but a SuperAdmin account - see
 * AdminLoginCommandHandler. If the account has 2FA enabled, both paths pause
 * with requiresTwoFactor and this page asks for a TOTP code before finishing.
 */
export default function LoginPage() {
  const { isAuthenticated, login, loginAsAdmin, completeTwoFactorLogin } = useAuth();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [mode, setMode] = useState<"staff" | "admin">("staff");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [pendingUserId, setPendingUserId] = useState<string | null>(null);
  const [twoFactorCode, setTwoFactorCode] = useState("");

  if (isAuthenticated) return <Navigate to="/" replace />;

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const result = mode === "admin" ? await loginAsAdmin(email, password) : await login(email, password);
      if (result.requiresTwoFactor && result.pendingUserId) {
        setPendingUserId(result.pendingUserId);
      } else {
        navigate("/", { replace: true });
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  async function handleTwoFactorSubmit(e: FormEvent) {
    e.preventDefault();
    if (!pendingUserId) return;
    setError(null);
    setLoading(true);
    try {
      await completeTwoFactorLogin(pendingUserId, twoFactorCode);
      navigate("/", { replace: true });
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-evergreen-900 px-4">
      <div className="mb-6">
        <LanguageSwitcher />
      </div>

      <div className="w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center text-center">
          {pendingUserId ? (
            <KeyRound className="mb-3 h-9 w-9 text-wheat-400" strokeWidth={1.5} />
          ) : mode === "admin" ? (
            <ShieldCheck className="mb-3 h-9 w-9 text-wheat-400" strokeWidth={1.5} />
          ) : (
            <Sprout className="mb-3 h-9 w-9 text-wheat-400" strokeWidth={1.5} />
          )}
          <h1 className="font-display text-2xl text-white">AgriLink <span className="font-normal text-evergreen-100/70">Ethiopia</span></h1>
          <p className="mt-1 text-sm text-evergreen-100/60">{t("login.tagline")}</p>
        </div>

        {pendingUserId ? (
          <form onSubmit={handleTwoFactorSubmit} className="card space-y-4 p-6">
            <p className="text-sm text-ink-600">{t("auth.twoFactorPrompt")}</p>
            <div>
              <label className="label">{t("auth.twoFactorCode")}</label>
              <input
                className="input text-center font-mono text-lg tracking-widest"
                inputMode="numeric"
                maxLength={6}
                required
                value={twoFactorCode}
                onChange={(e) => setTwoFactorCode(e.target.value)}
              />
            </div>
            {error && <p className="text-sm text-clay-600">{error}</p>}
            <button type="submit" disabled={loading} className="btn-primary w-full">
              {loading ? t("common.saving") : t("auth.verifyAndSignIn")}
            </button>
            <button
              type="button"
              className="block w-full text-center text-xs text-evergreen-700 underline underline-offset-2"
              onClick={() => { setPendingUserId(null); setTwoFactorCode(""); }}
            >
              {t("auth.backToLogin")}
            </button>
          </form>
        ) : (
          <>
            <div className="mb-4 flex overflow-hidden rounded-md border border-evergreen-700/60">
              <button
                className={`flex-1 py-2 text-sm transition ${mode === "staff" ? "bg-wheat-400 font-medium text-evergreen-900" : "bg-transparent text-evergreen-100/70 hover:text-white"}`}
                onClick={() => setMode("staff")}
              >
                {t("auth.staffLogin")}
              </button>
              <button
                className={`flex-1 py-2 text-sm transition ${mode === "admin" ? "bg-wheat-400 font-medium text-evergreen-900" : "bg-transparent text-evergreen-100/70 hover:text-white"}`}
                onClick={() => setMode("admin")}
              >
                {t("auth.adminLogin")}
              </button>
            </div>

            <form onSubmit={handleSubmit} className="card space-y-4 p-6">
              <div>
                <label className="label" htmlFor="email">{t("login.email")}</label>
                <input id="email" type="email" required className="input" value={email} onChange={(e) => setEmail(e.target.value)} />
              </div>
              <div>
                <label className="label" htmlFor="password">{t("login.password")}</label>
                <input id="password" type="password" required className="input" value={password} onChange={(e) => setPassword(e.target.value)} />
              </div>

              {error && <p className="text-sm text-clay-600">{error}</p>}

              <button type="submit" disabled={loading} className="btn-primary w-full">
                {loading ? t("login.signingIn") : t("login.signIn")}
              </button>

              <div className="flex items-center justify-end text-xs">
                <Link to="/forgot-password" className="text-evergreen-700 underline underline-offset-2">
                  {t("auth.forgotPassword")}
                </Link>
              </div>
            </form>
          </>
        )}
      </div>
    </div>
  );
}
