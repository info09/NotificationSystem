import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse } from './auth.models';

const ACCESS_TOKEN_STORAGE_KEY = 'adminui.access_token';

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

  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }
}
