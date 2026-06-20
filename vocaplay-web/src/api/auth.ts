import { api } from './axios';
import type { AuthResponse, TokenResponse } from '../types';

export function register(email: string, displayName: string, password: string) {
  return api.post<AuthResponse>('/auth/register', { email, displayName, password });
}

export function login(email: string, password: string) {
  return api.post<AuthResponse>('/auth/login', { email, password });
}

export function refresh(refreshToken: string) {
  return api.post<TokenResponse>('/auth/refresh', { refreshToken });
}

export function logout(refreshToken: string) {
  return api.post<void>('/auth/logout', { refreshToken });
}
