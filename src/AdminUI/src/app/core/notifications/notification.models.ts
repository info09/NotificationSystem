export type NotificationStatus = 'Pending' | 'Processing' | 'Sent' | 'Failed';
export type NotificationChannel = 'Email' | 'InApp';

export interface NotificationItem {
  id: string;
  recipient: string;
  channel: string;
  status: string;
  content: string | null;
  notificationTemplateId: string | null;
  createdAt: string;
}

export interface GetNotificationsResponse {
  items: NotificationItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface NotificationListQuery {
  status?: string | null;
  channel?: string | null;
  createdFrom?: string | null;
  createdTo?: string | null;
  page: number;
  pageSize: number;
}

export interface NotificationDeliveryLogItem {
  id: string;
  notificationId: string;
  channel: string;
  result: string;
  errorMessage: string | null;
  retryCount: number;
  correlationId: string | null;
  createdAtUtc: string;
}

export interface NotificationDeliveryAttemptItem {
  id: string;
  notificationId: string;
  channel: string;
  attemptNumber: number;
  result: string;
  errorMessage: string | null;
  correlationId: string | null;
  attemptedAtUtc: string;
}

export interface NotificationDeliveryAttemptsResponse {
  items: NotificationDeliveryAttemptItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface NotificationDeliveryHistoryResponse {
  logs: NotificationDeliveryLogItem[];
  attempts: NotificationDeliveryAttemptsResponse;
}
