import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  ConfirmPasswordResetRequest,
  ConfirmPasswordResetResponse,
  LoginRequest,
  LoginResponse,
  RequestPasswordResetRequest,
  RequestPasswordResetResponse
} from './auth.models';

const ACCESS_TOKEN_STORAGE_KEY = 'adminui.access_token';
const ROLE_CLAIM_KEYS = ['role', 'roles', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', request).pipe(
      tap((response) => {
        if (response?.accessToken) {
          localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, response.accessToken);
        }
      })
    );
  }

  requestPasswordReset(request: RequestPasswordResetRequest): Observable<RequestPasswordResetResponse> {
    return this.http.post<RequestPasswordResetResponse>('/api/auth/password-reset/request', request);
  }

  confirmPasswordReset(request: ConfirmPasswordResetRequest): Observable<ConfirmPasswordResetResponse> {
    return this.http.post<ConfirmPasswordResetResponse>('/api/auth/password-reset/confirm', request);
  }

  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  getRoles(): string[] {
    const token = this.getAccessToken();
    if (!token) {
      return [];
    }

    const payload = this.decodePayload(token);
    if (!payload) {
      return [];
    }

    for (const key of ROLE_CLAIM_KEYS) {
      const value = payload[key];
      if (typeof value === 'string' && value.trim()) {
        return [value];
      }
      if (Array.isArray(value)) {
        return value.filter((item): item is string => typeof item === 'string' && !!item.trim());
      }
    }

    return [];
  }

  hasRole(role: string): boolean {
    return this.getRoles().some((item) => item.toLowerCase() === role.toLowerCase());
  }

  isSystemAdministrator(): boolean {
    return this.hasRole('SystemAdministrator');
  }

  private decodePayload(token: string): Record<string, unknown> | null {
    const parts = token.split('.');
    if (parts.length < 2) {
      return null;
    }

    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
      const json = atob(padded);
      return JSON.parse(json) as Record<string, unknown>;
    } catch {
      return null;
    }
  }
}
