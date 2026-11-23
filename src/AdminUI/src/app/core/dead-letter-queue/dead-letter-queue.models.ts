export interface DeadLetterQueueMessage {
  queueName: string;
  messageId: string;
  notificationId: string | null;
  channel: string | null;
  routingKey: string | null;
  eventType: string | null;
  reason: string | null;
  deadLetteredAtUtc: string | null;
  deathCount: number | null;
  payload: string | null;
}

export interface GetDeadLetterQueueMessagesResponse {
  queueName: string;
  items: DeadLetterQueueMessage[];
  count: number;
}
