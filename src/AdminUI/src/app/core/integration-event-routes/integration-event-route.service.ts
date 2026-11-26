import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateIntegrationEventRouteRequest,
  GetIntegrationEventRoutesResponse,
  IntegrationEventRoute,
  UpdateIntegrationEventRouteRequest
} from './integration-event-route.models';

@Injectable({ providedIn: 'root' })
export class IntegrationEventRouteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/admin/integration-event-routes';

  getIntegrationEventRoutes(params: {
    eventType?: string;
    channel?: string;
    isActive?: boolean;
    page: number;
    pageSize: number;
  }): Observable<GetIntegrationEventRoutesResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));

    if (params.eventType) {
      httpParams = httpParams.set('eventType', params.eventType);
    }
    if (params.channel) {
      httpParams = httpParams.set('channel', params.channel);
    }
    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', String(params.isActive));
    }

    return this.http.get<GetIntegrationEventRoutesResponse>(this.baseUrl, { params: httpParams });
  }

  getIntegrationEventRouteById(id: string): Observable<IntegrationEventRoute> {
    return this.http.get<IntegrationEventRoute>(`${this.baseUrl}/${id}`);
  }

  createIntegrationEventRoute(request: CreateIntegrationEventRouteRequest): Observable<IntegrationEventRoute> {
    return this.http.post<IntegrationEventRoute>(this.baseUrl, request);
  }

  updateIntegrationEventRoute(id: string, request: UpdateIntegrationEventRouteRequest): Observable<IntegrationEventRoute> {
    return this.http.put<IntegrationEventRoute>(`${this.baseUrl}/${id}`, request);
  }

  deleteIntegrationEventRoute(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/${id}`);
  }
}
