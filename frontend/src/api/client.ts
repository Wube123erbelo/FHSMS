import axios from "axios";

/**
 * Single axios instance for the whole app. The JWT is attached automatically
 * by an interceptor (see AuthContext) so individual page components never
 * touch auth headers directly.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080/api"
});

export function setAuthToken(token: string | null) {
  if (token) {
    apiClient.defaults.headers.common.Authorization = `Bearer ${token}`;
  } else {
    delete apiClient.defaults.headers.common.Authorization;
  }
}

let currentRefreshToken: string | null = null;
let onTokensRefreshed: ((token: string, refreshToken: string) => void) | null = null;
let onRefreshFailed: (() => void) | null = null;

export function setRefreshToken(token: string | null) {
  currentRefreshToken = token;
}

/** AuthContext registers these once on mount so the interceptor below can update React state + storage after a silent refresh. */
export function registerTokenRefreshHandlers(
  onRefreshed: (token: string, refreshToken: string) => void,
  onFailed: () => void
) {
  onTokensRefreshed = onRefreshed;
  onRefreshFailed = onFailed;
}

let refreshInFlight: Promise<string | null> | null = null;

async function attemptRefresh(): Promise<string | null> {
  if (!currentRefreshToken) return null;

  if (!refreshInFlight) {
    refreshInFlight = apiClient
      .post<{ token: string; refreshToken: string }>("/auth/refresh", { token: currentRefreshToken })
      .then((res) => {
        setAuthToken(res.data.token);
        setRefreshToken(res.data.refreshToken);
        onTokensRefreshed?.(res.data.token, res.data.refreshToken);
        return res.data.token;
      })
      .catch(() => {
        onRefreshFailed?.();
        return null;
      })
      .finally(() => {
        refreshInFlight = null;
      });
  }

  return refreshInFlight;
}

// On a 401 (expired access token), silently exchange the refresh token for a
// new one and retry the original request exactly once - callers never see
// the expiry unless the refresh token itself is invalid/expired too.
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    if (error.response?.status === 401 && !original._retried && currentRefreshToken) {
      original._retried = true;
      const newToken = await attemptRefresh();
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(original);
      }
    }
    return Promise.reject(error);
  }
);

/** Normalizes the shape of API errors (validation dictionary, message, or unknown). */
export function extractErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (data && typeof data === "object" && "message" in data) {
      return String((data as { message: unknown }).message);
    }
    if (data && typeof data === "object") {
      const firstKey = Object.keys(data)[0];
      const value = (data as Record<string, unknown>)[firstKey];
      if (Array.isArray(value)) return `${firstKey}: ${value[0]}`;
    }
    return error.message;
  }
  return "Something went wrong. Please try again.";
}
