import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { apiClient, setAuthToken, setRefreshToken, registerTokenRefreshHandlers } from "../api/client";
import type { LoginResult } from "../api/types";

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  fullName: string | null;
  role: string | null;
}

interface AuthContextValue extends AuthState {
  isAuthenticated: boolean;
  /** Returns the LoginResult so the caller can check requiresTwoFactor before treating this as a completed sign-in. */
  login: (email: string, password: string) => Promise<LoginResult>;
  loginAsAdmin: (email: string, password: string) => Promise<LoginResult>;
  completeTwoFactorLogin: (pendingUserId: string, code: string) => Promise<void>;
  logout: () => void;
}

const STORAGE_KEY = "fhsms.auth";

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function emptyState(): AuthState {
  return { token: null, refreshToken: null, fullName: null, role: null };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() => {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as AuthState) : emptyState();
  });

  useEffect(() => {
    setAuthToken(state.token);
    setRefreshToken(state.refreshToken);
  }, [state.token, state.refreshToken]);

  useEffect(() => {
    // Wire the axios interceptor's silent-refresh outcome back into React
    // state + storage, and force a clean logout if the refresh token itself
    // turns out to be invalid/expired/reused (see RefreshTokenCommandHandler).
    registerTokenRefreshHandlers(
      (token, refreshToken) => {
        setState((prev) => {
          const next = { ...prev, token, refreshToken };
          sessionStorage.setItem(STORAGE_KEY, JSON.stringify(next));
          return next;
        });
      },
      () => {
        setState(emptyState());
        sessionStorage.removeItem(STORAGE_KEY);
      }
    );
  }, []);

  function persist(next: AuthState) {
    setState(next);
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  }

  async function login(email: string, password: string): Promise<LoginResult> {
    const { data } = await apiClient.post<LoginResult>("/auth/login", { email, password });
    if (!data.requiresTwoFactor) {
      persist({ token: data.token, refreshToken: data.refreshToken, fullName: data.fullName, role: data.role });
    }
    return data;
  }

  async function loginAsAdmin(email: string, password: string): Promise<LoginResult> {
    const { data } = await apiClient.post<LoginResult>("/auth/admin-login", { email, password });
    if (!data.requiresTwoFactor) {
      persist({ token: data.token, refreshToken: data.refreshToken, fullName: data.fullName, role: data.role });
    }
    return data;
  }

  async function completeTwoFactorLogin(pendingUserId: string, code: string) {
    const { data } = await apiClient.post<LoginResult>("/auth/2fa/login-verify", { pendingUserId, code });
    persist({ token: data.token, refreshToken: data.refreshToken, fullName: data.fullName, role: data.role });
  }

  function logout() {
    if (state.refreshToken) {
      apiClient.post("/auth/logout", { token: state.refreshToken }).catch(() => {});
    }
    persist(emptyState());
  }

  const value = useMemo<AuthContextValue>(
    () => ({ ...state, isAuthenticated: !!state.token, login, loginAsAdmin, completeTwoFactorLogin, logout }),
    [state]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
