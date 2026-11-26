import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateNotificationChannelConfigRequest,
  GetNotificationChannelConfigsResponse,
  NotificationChannelConfig,
  UpdateNotificationChannelConfigRequest
} from './notification-channel-config.models';

@Injectable({ providedIn: 'root' })
export class NotificationChannelConfigService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/admin/notification-channel-configs';

  getNotificationChannelConfigs(params: {
    channel?: string;
    isEnabled?: boolean;
    page: number;
    pageSize: number;
  }): Observable<GetNotificationChannelConfigsResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));

    if (params.channel) {
      httpParams = httpParams.set('channel', params.channel);
    }
    if (params.isEnabled !== undefined) {
      httpParams = httpParams.set('isEnabled', String(params.isEnabled));
    }

    return this.http.get<GetNotificationChannelConfigsResponse>(this.baseUrl, { params: httpParams });
  }

  getNotificationChannelConfigById(id: string): Observable<NotificationChannelConfig> {
    return this.http.get<NotificationChannelConfig>(`${this.baseUrl}/${id}`);
  }

  createNotificationChannelConfig(request: CreateNotificationChannelConfigRequest): Observable<NotificationChannelConfig> {
    return this.http.post<NotificationChannelConfig>(this.baseUrl, request);
  }

  updateNotificationChannelConfig(id: string, request: UpdateNotificationChannelConfigRequest): Observable<NotificationChannelConfig> {
    return this.http.put<NotificationChannelConfig>(`${this.baseUrl}/${id}`, request);
  }

  deleteNotificationChannelConfig(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/${id}`);
  }
}
