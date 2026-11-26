export interface NotificationTemplate {
  id: string;
  templateKey: string;
  name: string;
  description?: string | null;
  channel: string;
  subject?: string | null;
  body: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface GetNotificationTemplatesResponse {
  items: NotificationTemplate[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateNotificationTemplateRequest {
  templateKey: string;
  name: string;
  description?: string | null;
  channel: string;
  subject?: string | null;
  body: string;
  isActive: boolean;
}

export interface UpdateNotificationTemplateRequest {
  name: string;
  description?: string | null;
  channel: string;
  subject?: string | null;
  body: string;
  isActive: boolean;
}
