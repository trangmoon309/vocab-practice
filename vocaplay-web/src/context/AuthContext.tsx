import { createContext, useCallback, useEffect, useState, type ReactNode } from 'react';
import * as authApi from '../api/auth';
import { getAccessToken, setAccessToken } from '../api/axios';
import type { User } from '../types';

interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string, rememberMe?: boolean) => Promise<void>;
  register: (email: string, displayName: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Prefer localStorage (remember me) then fall back to sessionStorage
    const storage = localStorage.getItem('refreshToken') ? localStorage : sessionStorage;
    const refreshToken = storage.getItem('refreshToken');
    if (!refreshToken) {
      setIsLoading(false);
      return;
    }

    authApi
      .refresh(refreshToken)
      .then((response) => {
        setAccessToken(response.data.accessToken);
        storage.setItem('refreshToken', response.data.refreshToken);
        const storedUser = storage.getItem('user');
        if (storedUser) {
          setUser(JSON.parse(storedUser) as User);
        }
      })
      .catch(() => {
        setAccessToken(null);
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        sessionStorage.removeItem('refreshToken');
        sessionStorage.removeItem('user');
      })
      .finally(() => setIsLoading(false));
  }, []);

  const login = useCallback(async (email: string, password: string, rememberMe = false) => {
    const response = await authApi.login(email, password);
    const storage = rememberMe ? localStorage : sessionStorage;
    // Clear the other storage to avoid stale tokens
    const other = rememberMe ? sessionStorage : localStorage;
    other.removeItem('refreshToken');
    other.removeItem('user');
    setAccessToken(response.data.accessToken);
    storage.setItem('refreshToken', response.data.refreshToken);
    storage.setItem('user', JSON.stringify(response.data.user));
    setUser(response.data.user);
  }, []);

  const register = useCallback(async (email: string, displayName: string, password: string) => {
    const response = await authApi.register(email, displayName, password);
    setAccessToken(response.data.accessToken);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    localStorage.setItem('user', JSON.stringify(response.data.user));
    setUser(response.data.user);
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    try {
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    } catch {
      // ignore — proceed with local cleanup regardless
    }
    setAccessToken(null);
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('rememberedEmail');
    sessionStorage.removeItem('refreshToken');
    sessionStorage.removeItem('user');
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: getAccessToken() !== null && user !== null,
        isLoading,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
