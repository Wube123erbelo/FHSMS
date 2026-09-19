import { useState } from "react";
import { Plus, KeyRound } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import BackLink from "../../components/BackLink";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import type { UserDto, UserRole } from "../../api/types";

const ALL_ROLES: UserRole[] = ["SuperAdmin", "HotelAgent", "FarmerAgent", "HotelCustomer", "PublicPortalUser", "Driver"];
// Hotel customers and public portal users sign themselves up via public
// registration - the admin "New user" form only ever creates staff/agent
// accounts (mirrors the same restriction enforced server-side in
// CreateUserCommandHandler). Editing an existing account still allows every
// role, since a self-registered account may legitimately need managing.
const STAFF_ROLES: UserRole[] = ["SuperAdmin", "HotelAgent", "FarmerAgent", "Driver"];

export default function UsersPage() {
  const { t } = useTranslation();
  const { data: users, loading, error, reload } = useFetch<UserDto[]>("/users");
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<UserDto | null>(null);
  const [resettingFor, setResettingFor] = useState<UserDto | null>(null);

  async function toggleActive(user: UserDto) {
    await apiClient.post(`/users/${user.id}/${user.isActive ? "suspend" : "activate"}`);
    reload();
  }

  return (
    <div>
      <BackLink to="/settings" />
      <PageHeader
        title={t("users.title")}
        description={t("users.description")}
        action={
          <button className="btn-primary" onClick={() => { setEditing(null); setShowForm((s) => !s); }}>
            <Plus className="h-4 w-4" /> {t("users.newUser")}
          </button>
        }
      />

      {showForm && (
        <div className="mb-6">
          <NewUserForm onCreated={() => { setShowForm(false); reload(); }} />
        </div>
      )}

      {editing && (
        <div className="mb-6">
          <EditUserForm user={editing} onDone={() => { setEditing(null); reload(); }} onCancel={() => setEditing(null)} />
        </div>
      )}

      {resettingFor && (
        <div className="mb-6">
          <ResetPasswordForm user={resettingFor} onDone={() => setResettingFor(null)} onCancel={() => setResettingFor(null)} />
        </div>
      )}

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}

      {users && users.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("common.id")}</th>
                <th>{t("users.fullName")}</th>
                <th>{t("login.email")}</th>
                <th>{t("users.role")}</th>
                <th>{t("agentsManagement.presence")}</th>
                <th>{t("common.status")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id}>
                  <td className="font-mono text-xs">{u.code ?? "-"}</td>
                  <td className="font-medium">{u.fullName}</td>
                  <td>{u.email}</td>
                  <td>{u.role}</td>
                  <td>
                    <span className={`inline-flex items-center gap-1.5 text-xs ${u.isOnline ? "text-evergreen-700" : "text-ink-300"}`}>
                      <span className={`h-2 w-2 rounded-full ${u.isOnline ? "bg-evergreen-500" : "bg-ink-300"}`} />
                      {u.isOnline ? t("agentsManagement.online") : t("agentsManagement.offline")}
                    </span>
                  </td>
                  <td><StatusBadge status={u.isActive ? "Active" : "Inactive"} /></td>
                  <td>
                    <div className="flex gap-1">
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => { setShowForm(false); setEditing(u); }}>
                        {t("common.edit")}
                      </button>
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => setResettingFor(u)} title={t("users.resetPassword")}>
                        <KeyRound className="h-3.5 w-3.5" /> {t("users.resetPassword")}
                      </button>
                      <button className="btn-secondary px-2 py-1 text-xs" onClick={() => toggleActive(u)}>
                        {u.isActive ? t("users.suspend") : t("users.activate")}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function NewUserForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<UserRole>("HotelAgent");
  const [phone, setPhone] = useState("");
  const [location, setLocation] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/users", { fullName, email, password, role, phone: phone || null, location: location || null });
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("users.newUser")}</h3>
      <p className="mb-4 text-xs text-ink-300">{t("users.newUserHint")}</p>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("users.fullName")}</label>
          <input className="input" value={fullName} onChange={(e) => setFullName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("login.email")}</label>
          <input type="email" className="input" value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("login.password")}</label>
          <input type="password" className="input" value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("users.role")}</label>
          <select className="input" value={role} onChange={(e) => setRole(e.target.value as UserRole)}>
            {STAFF_ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
          </select>
        </div>
        <div>
          <label className="label">{t("customers.phone")}</label>
          <input className="input" value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="09XXXXXXXX" />
        </div>
        <div>
          <label className="label">{t("farmers.location")}</label>
          <input className="input" value={location} onChange={(e) => setLocation(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !fullName || !email || password.length < 8}>
        {saving ? t("common.saving") : t("common.create")}
      </button>
    </div>
  );
}

function EditUserForm({ user, onDone, onCancel }: { user: UserDto; onDone: () => void; onCancel: () => void }) {
  const { t } = useTranslation();
  const [fullName, setFullName] = useState(user.fullName);
  const [role, setRole] = useState<UserRole>(user.role);
  const [isActive, setIsActive] = useState(user.isActive);
  const [phone, setPhone] = useState(user.phone ?? "");
  const [location, setLocation] = useState(user.location ?? "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.put(`/users/${user.id}`, { fullName, role, isActive, phone: phone || null, location: location || null });
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("common.edit")}: {user.fullName}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("users.fullName")}</label>
          <input className="input" value={fullName} onChange={(e) => setFullName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("login.email")}</label>
          <input className="input" value={user.email} disabled />
        </div>
        <div>
          <label className="label">{t("users.role")}</label>
          <select className="input" value={role} onChange={(e) => setRole(e.target.value as UserRole)}>
            {ALL_ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
          </select>
        </div>
        <div>
          <label className="label">{t("common.status")}</label>
          <select className="input" value={isActive ? "1" : "0"} onChange={(e) => setIsActive(e.target.value === "1")}>
            <option value="1">{t("common.active")}</option>
            <option value="0">{t("common.inactive")}</option>
          </select>
        </div>
        <div>
          <label className="label">{t("customers.phone")}</label>
          <input className="input" value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="09XXXXXXXX" />
        </div>
        <div>
          <label className="label">{t("farmers.location")}</label>
          <input className="input" value={location} onChange={(e) => setLocation(e.target.value)} />
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <div className="mt-4 flex gap-2">
        <button className="btn-primary" onClick={submit} disabled={saving || !fullName}>
          {saving ? t("common.saving") : t("common.save")}
        </button>
        <button className="btn-secondary" onClick={onCancel}>{t("common.cancel")}</button>
      </div>
    </div>
  );
}

function ResetPasswordForm({ user, onDone, onCancel }: { user: UserDto; onDone: () => void; onCancel: () => void }) {
  const { t } = useTranslation();
  const [newPassword, setNewPassword] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState(false);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post(`/users/${user.id}/reset-password`, { newPassword });
      setDone(true);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card max-w-md p-6">
      <h3 className="mb-2 text-base font-semibold text-evergreen-900">{t("users.resetPassword")}: {user.fullName}</h3>
      <p className="mb-4 text-xs text-ink-300">{t("users.resetPasswordHint")}</p>

      {done ? (
        <div>
          <p className="mb-4 rounded-md border border-evergreen-500/30 bg-evergreen-50 px-3 py-2 text-sm text-evergreen-900">
            {t("users.resetPasswordDone")} <strong className="font-mono">{newPassword}</strong>
          </p>
          <button className="btn-primary" onClick={onDone}>{t("common.save")}</button>
        </div>
      ) : (
        <>
          <label className="label">{t("users.newPassword")}</label>
          <input type="text" className="input" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
          {error && <div className="mt-4"><ErrorState message={error} /></div>}
          <div className="mt-4 flex gap-2">
            <button className="btn-primary" onClick={submit} disabled={saving || newPassword.length < 8}>
              {saving ? t("common.saving") : t("users.resetPassword")}
            </button>
            <button className="btn-secondary" onClick={onCancel}>{t("common.cancel")}</button>
          </div>
        </>
      )}
    </div>
  );
}
