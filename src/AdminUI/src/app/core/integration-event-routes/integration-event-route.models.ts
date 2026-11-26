export interface IntegrationEventRoute {
  id: string;
  eventType: string;
  notificationTemplateId?: string | null;
  channel?: string | null;
  description?: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface GetIntegrationEventRoutesResponse {
  items: IntegrationEventRoute[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateIntegrationEventRouteRequest {
  eventType: string;
  notificationTemplateId?: string | null;
  channel?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface UpdateIntegrationEventRouteRequest {
  eventType: string;
  notificationTemplateId?: string | null;
  channel?: string | null;
  description?: string | null;
  isActive: boolean;
}
