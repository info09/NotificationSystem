import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GetNotificationsResponse, NotificationListQuery } from './notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);

  getNotifications(query: NotificationListQuery): Observable<GetNotificationsResponse> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.status) {
      params = params.set('status', query.status);
    }
    if (query.channel) {
      params = params.set('channel', query.channel);
    }
    if (query.createdFrom) {
      params = params.set('createdFrom', query.createdFrom);
    }
    if (query.createdTo) {
      params = params.set('createdTo', query.createdTo);
    }

    return this.http.get<GetNotificationsResponse>('/api/notifications', { params });
  }
}
