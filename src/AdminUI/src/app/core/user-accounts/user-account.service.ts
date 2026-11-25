import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateIntegrationClientCredentialRequest,
  CreateUserAccountRequest,
  CreatedIntegrationClientCredentialResponse,
  GetUserAccountsResponse,
  IntegrationClientCredential,
  RotatedIntegrationClientCredentialResponse,
  UpdateUserAccountRequest,
  UserAccount
} from './user-account.models';

@Injectable({ providedIn: 'root' })
export class UserAccountService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/admin/users';

  getUserAccounts(params: { status?: string; role?: string; page: number; pageSize: number }): Observable<GetUserAccountsResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));

    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params.role) {
      httpParams = httpParams.set('role', params.role);
    }

    return this.http.get<GetUserAccountsResponse>(this.baseUrl, { params: httpParams });
  }

  getUserAccountById(id: string): Observable<UserAccount> {
    return this.http.get<UserAccount>(`${this.baseUrl}/${id}`);
  }

  createUserAccount(request: CreateUserAccountRequest): Observable<UserAccount> {
    return this.http.post<UserAccount>(this.baseUrl, request);
  }

  updateUserAccount(id: string, request: UpdateUserAccountRequest): Observable<UserAccount> {
    return this.http.put<UserAccount>(`${this.baseUrl}/${id}`, request);
  }

  lockUserAccount(id: string): Observable<UserAccount> {
    return this.http.post<UserAccount>(`${this.baseUrl}/${id}/lock`, {});
  }

  unlockUserAccount(id: string): Observable<UserAccount> {
    return this.http.post<UserAccount>(`${this.baseUrl}/${id}/unlock`, {});
  }

  deleteUserAccount(id: string): Observable<UserAccount> {
    return this.http.delete<UserAccount>(`${this.baseUrl}/${id}`);
  }

  getCredentials(userId: string): Observable<IntegrationClientCredential[]> {
    return this.http.get<IntegrationClientCredential[]>(`${this.baseUrl}/${userId}/credentials`);
  }

  createCredential(userId: string, request: CreateIntegrationClientCredentialRequest): Observable<CreatedIntegrationClientCredentialResponse> {
    return this.http.post<CreatedIntegrationClientCredentialResponse>(`${this.baseUrl}/${userId}/credentials`, request);
  }

  rotateCredential(userId: string, credentialId: string): Observable<RotatedIntegrationClientCredentialResponse> {
    return this.http.post<RotatedIntegrationClientCredentialResponse>(`${this.baseUrl}/${userId}/credentials/${credentialId}/rotate`, {});
  }

  revokeCredential(userId: string, credentialId: string): Observable<IntegrationClientCredential> {
    return this.http.post<IntegrationClientCredential>(`${this.baseUrl}/${userId}/credentials/${credentialId}/revoke`, {});
  }
}
