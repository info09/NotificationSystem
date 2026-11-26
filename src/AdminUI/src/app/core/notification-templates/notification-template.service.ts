import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateNotificationTemplateRequest,
  GetNotificationTemplatesResponse,
  NotificationTemplate,
  UpdateNotificationTemplateRequest
} from './notification-template.models';

@Injectable({ providedIn: 'root' })
export class NotificationTemplateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/admin/notification-templates';

  getNotificationTemplates(params: {
    channel?: string;
    isActive?: boolean;
    keyword?: string;
    page: number;
    pageSize: number;
  }): Observable<GetNotificationTemplatesResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));

    if (params.channel) {
      httpParams = httpParams.set('channel', params.channel);
    }
    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', String(params.isActive));
    }
    if (params.keyword) {
      httpParams = httpParams.set('keyword', params.keyword);
    }

    return this.http.get<GetNotificationTemplatesResponse>(this.baseUrl, { params: httpParams });
  }

  getNotificationTemplateById(id: string): Observable<NotificationTemplate> {
    return this.http.get<NotificationTemplate>(`${this.baseUrl}/${id}`);
  }

  createNotificationTemplate(request: CreateNotificationTemplateRequest): Observable<NotificationTemplate> {
    return this.http.post<NotificationTemplate>(this.baseUrl, request);
  }

  updateNotificationTemplate(id: string, request: UpdateNotificationTemplateRequest): Observable<NotificationTemplate> {
    return this.http.put<NotificationTemplate>(`${this.baseUrl}/${id}`, request);
  }

  deleteNotificationTemplate(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/${id}`);
  }
}
