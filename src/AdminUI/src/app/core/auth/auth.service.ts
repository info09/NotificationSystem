import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, from, switchMap, tap, throwError } from 'rxjs';
import { LoginRequest, LoginResponse } from './auth.models';

const ACCESS_TOKEN_STORAGE_KEY = 'adminui.access_token';

// Dev fallback (backend login chưa implement - ST-P13-04). Chỉ dùng khi API /api/auth/login trả 404.
const DEV_JWT_ISSUER = 'notification-system-dev';
const DEV_JWT_AUDIENCE = 'notification-system-dev';
const DEV_JWT_SIGNING_KEY = 'dev-only-change-this-signing-key-min-32-chars-000000';
const DEV_JWT_ROLE = 'SystemAdministrator';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', request).pipe(
      tap((response) => {
        if (response?.accessToken) {
          localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, response.accessToken);
        }
      }),
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          return from(this.createDevLoginResponse(request.userName)).pipe(
            tap((response) => {
              localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, response.accessToken);
            })
          );
        }
        return throwError(() => error);
      })
    );
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

  private async createDevLoginResponse(userName: string): Promise<LoginResponse> {
    const subject = (userName || 'dev-user').trim() || 'dev-user';
    const accessToken = await this.createDevJwt(subject);
    return { accessToken, tokenType: 'Bearer' };
  }

  private async createDevJwt(subject: string): Promise<string> {
    const now = Math.floor(Date.now() / 1000);
    const header = { alg: 'HS256', typ: 'JWT' };
    const payload = {
      iss: DEV_JWT_ISSUER,
      aud: DEV_JWT_AUDIENCE,
      sub: subject,
      nameid: subject,
      unique_name: subject,
      role: DEV_JWT_ROLE,
      iat: now,
      nbf: now,
      exp: now + 60 * 60 * 12
    };

    const encodedHeader = this.base64UrlEncode(JSON.stringify(header));
    const encodedPayload = this.base64UrlEncode(JSON.stringify(payload));
    const unsignedToken = `${encodedHeader}.${encodedPayload}`;
    const signature = await this.signHs256(unsignedToken, DEV_JWT_SIGNING_KEY);
    return `${unsignedToken}.${signature}`;
  }

  private async signHs256(data: string, secret: string): Promise<string> {
    const encoder = new TextEncoder();
    const key = await crypto.subtle.importKey(
      'raw',
      encoder.encode(secret),
      { name: 'HMAC', hash: 'SHA-256' },
      false,
      ['sign']
    );
    const signatureBuffer = await crypto.subtle.sign('HMAC', key, encoder.encode(data));
    return this.base64UrlEncodeBytes(new Uint8Array(signatureBuffer));
  }

  private base64UrlEncode(value: string): string {
    return this.base64UrlEncodeBytes(new TextEncoder().encode(value));
  }

  private base64UrlEncodeBytes(bytes: Uint8Array): string {
    let binary = '';
    for (const b of bytes) {
      binary += String.fromCharCode(b);
    }
    const base64 = btoa(binary);
    return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, '');
  }
}
