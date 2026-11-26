export interface NotificationChannelConfig {
  id: string;
  channel: string;
  displayName: string;
  description?: string | null;
  isEnabled: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface GetNotificationChannelConfigsResponse {
  items: NotificationChannelConfig[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateNotificationChannelConfigRequest {
  channel: string;
  displayName: string;
  description?: string | null;
  isEnabled: boolean;
}

export interface UpdateNotificationChannelConfigRequest {
  displayName: string;
  description?: string | null;
  isEnabled: boolean;
}
