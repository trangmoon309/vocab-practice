import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type { TokenResponse } from '../types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080/api';

export const api = axios.create({
  baseURL: BASE_URL,
});

let accessToken: string | null = null;

export function setAccessToken(token: string | null): void {
  accessToken = token;
}

export function getAccessToken(): string | null {
  return accessToken;
}

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (accessToken) {
    config.headers.set('Authorization', `Bearer ${accessToken}`);
  }
  return config;
});

let refreshPromise: Promise<string | null> | null = null;

function getTokenStorage(): Storage | null {
  if (localStorage.getItem('refreshToken')) return localStorage;
  if (sessionStorage.getItem('refreshToken')) return sessionStorage;
  return null;
}

async function refreshAccessToken(): Promise<string | null> {
  const storage = getTokenStorage();
  const refreshToken = storage?.getItem('refreshToken') ?? null;
  if (!storage || !refreshToken) return null;

  try {
    const response = await axios.post<TokenResponse>(`${BASE_URL}/auth/refresh`, {
      refreshToken,
    });
    setAccessToken(response.data.accessToken);
    storage.setItem('refreshToken', response.data.refreshToken);
    return response.data.accessToken;
  } catch {
    setAccessToken(null);
    storage.removeItem('refreshToken');
    storage.removeItem('user');
    return null;
  }
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      if (!refreshPromise) {
        refreshPromise = refreshAccessToken().finally(() => {
          refreshPromise = null;
        });
      }

      const newToken = await refreshPromise;
      if (newToken) {
        originalRequest.headers.set('Authorization', `Bearer ${newToken}`);
        return api(originalRequest);
      }
    }

    return Promise.reject(error);
  }
);
