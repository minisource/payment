import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { resolveLanguage, getLanguageHeader, getAcceptLanguageHeader } from '@/shared/i18n/language';
import { toast } from 'sonner';

/** Standard backend error shape (matches payment backend ErrorResponse) */
export interface ApiErrorResponse {
  error: {
    code: string;
    message: string;
    user_message?: string;
    category?: 'validation' | 'business' | 'permission' | 'not_found' | 'system';
    details?: Record<string, unknown>;
    field_errors?: Array<{ field: string; message: string }>;
    request_id?: string;
    correlation_id?: string;
  };
}

export class ApiError extends Error {
  constructor(
    public status: number,
    public code: string,
    message: string,
    public userMessage?: string,
    public category?: string,
    public fieldErrors?: Array<{ field: string; message: string }>,
    public requestId?: string,
    public correlationId?: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

function parseApiError(status: number, data: unknown): ApiError {
  const err = (data as ApiErrorResponse)?.error;
  if (err) {
    return new ApiError(status, err.code, err.message, err.user_message, err.category, err.field_errors, err.request_id, err.correlation_id);
  }
  return new ApiError(status, 'UNKNOWN', typeof data === 'string' ? data : 'Unknown error');
}

/** Detects if auth is required by checking localStorage state + config cache + dev mock */
function isAuthModeEnabled(): boolean {
  if (typeof window === 'undefined') return false;

  // Dev mock mode: no real auth needed
  if (process.env.NEXT_PUBLIC_DEV_MOCK_AUTH === 'true') return false;

  // If there's a stored token, auth is enabled
  if (localStorage.getItem('accessToken')) return true;

  // Check cached public config
  try {
    const cached = localStorage.getItem('payment-public-config-cache');
    if (cached) {
      const config = JSON.parse(cached);
      return config?.auth?.enabled === true;
    }
  } catch {
    // ignore parse errors
  }

  // Default: assume auth enabled if no config available
  return true;
}

/** Creates the base axios instance */
function createClient(): AxiosInstance {
  const baseURL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5075';

  const client = axios.create({
    baseURL,
    timeout: 30_000,
    headers: { 'Content-Type': 'application/json' },
    withCredentials: false,
  });

  // Request interceptor: attach auth token + tenant/application headers
  client.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    if (typeof window !== 'undefined') {
      const authEnabled = isAuthModeEnabled();

      // Only attach Authorization header if auth is enabled AND token exists
      if (authEnabled) {
        const token = localStorage.getItem('accessToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
      }
      // If auth is disabled, we intentionally omit the Authorization header

      // Tenant / Application headers from localStorage (set by providers)
      const tenantId = localStorage.getItem('X-Tenant-Id');
      if (tenantId) {
        config.headers['X-Tenant-Id'] = tenantId;
      }

      const appCode = localStorage.getItem('X-Application-Code') || 'payment-admin';
      config.headers['X-Application-Code'] = appCode;

      // Language headers for i18n
      const currentLang = resolveLanguage();
      config.headers['X-Language'] = getLanguageHeader(currentLang);
      config.headers['Accept-Language'] = getAcceptLanguageHeader(currentLang);
    }
    return config;
  });

  // Response interceptor: parse errors into ApiError; global toast for unhandled errors
  client.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
      if (error.response) {
        const status = error.response.status;

        // Only clear auth on 401 if auth is enabled
        if (status === 401 && typeof window !== 'undefined') {
          const authEnabled = isAuthModeEnabled();
          if (authEnabled) {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
          }
        }

        const apiError = parseApiError(status, error.response.data);

        // Global toast for errors NOT handled by individual pages/mutations
        // 400/422 = validation (handled per-form), 401/403 = auth (handled by guards), 404 = per-page
        if (status >= 500) {
          toast.error(apiError.userMessage || apiError.message || 'Server error', {
            description: `Status: ${status}`,
          });
        }

        return Promise.reject(apiError);
      }

      // Network error / connection refused / timeout
      toast.error('Connection failed', {
        description: error.message === 'Network Error'
          ? 'Cannot reach the server. Is the API running?'
          : error.message || 'Network request failed',
        duration: 5000,
      });

      return Promise.reject(new ApiError(0, 'NETWORK_ERROR', error.message || 'Network request failed'));
    },
  );

  return client;
}

export const apiClient = createClient();

/**
 * Extract `data` from axios response. Usage: `await get('/path')` returns `T`.
 */
export async function get<T>(url: string, params?: Record<string, unknown>): Promise<T> {
  const { data } = await apiClient.get<T>(url, { params });
  return data;
}

export async function post<T>(url: string, body?: unknown): Promise<T> {
  const { data } = await apiClient.post<T>(url, body);
  return data;
}

export async function patch<T>(url: string, body?: unknown): Promise<T> {
  const { data } = await apiClient.patch<T>(url, body);
  return data;
}

export async function del<T>(url: string): Promise<T> {
  const { data } = await apiClient.delete<T>(url);
  return data;
}
