import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateEndUserProfileRequest,
  EndUserProfile,
  GetEndUserProfilesResponse,
  UpdateEndUserProfileRequest
} from './end-user-profile.models';

@Injectable({ providedIn: 'root' })
export class EndUserProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/admin/end-user-profiles';

  getEndUserProfiles(params: { status?: string; externalUserId?: string; page: number; pageSize: number }): Observable<GetEndUserProfilesResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.externalUserId) httpParams = httpParams.set('externalUserId', params.externalUserId);
    return this.http.get<GetEndUserProfilesResponse>(this.baseUrl, { params: httpParams });
  }

  getById(id: string): Observable<EndUserProfile> {
    return this.http.get<EndUserProfile>(`${this.baseUrl}/${id}`);
  }

  getByExternalUserId(externalUserId: string): Observable<EndUserProfile> {
    return this.http.get<EndUserProfile>(`${this.baseUrl}/by-external-id/${encodeURIComponent(externalUserId)}`);
  }

  create(request: CreateEndUserProfileRequest): Observable<EndUserProfile> {
    return this.http.post<EndUserProfile>(this.baseUrl, request);
  }

  update(id: string, request: UpdateEndUserProfileRequest): Observable<EndUserProfile> {
    return this.http.put<EndUserProfile>(`${this.baseUrl}/${id}`, request);
  }

  lock(id: string): Observable<EndUserProfile> {
    return this.http.post<EndUserProfile>(`${this.baseUrl}/${id}/lock`, {});
  }

  unlock(id: string): Observable<EndUserProfile> {
    return this.http.post<EndUserProfile>(`${this.baseUrl}/${id}/unlock`, {});
  }
}
