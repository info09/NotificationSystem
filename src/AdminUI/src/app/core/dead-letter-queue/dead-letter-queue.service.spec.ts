import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DeadLetterQueueService } from './dead-letter-queue.service';
import { GetDeadLetterQueueMessagesResponse } from './dead-letter-queue.models';

describe('DeadLetterQueueService', () => {
  let service: DeadLetterQueueService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(DeadLetterQueueService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request dead letter messages by queue and limit', () => {
    const mockResponse: GetDeadLetterQueueMessagesResponse = {
      queueName: 'notifications.email.dlq',
      count: 1,
      items: [
        {
          queueName: 'notifications.email.dlq',
          messageId: 'msg-1',
          notificationId: '11111111-1111-1111-1111-111111111111',
          channel: 'Email',
          routingKey: 'notification.email.created',
          eventType: 'NotificationCreated',
          reason: 'rejected',
          deadLetteredAtUtc: '2026-04-03T00:00:00Z',
          deathCount: 1,
          payload: '{}'
        }
      ]
    };

    service.getMessages('notifications.email.dlq', 20).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne((request) =>
      request.url === '/api/admin/dead-letter-queues/notifications.email.dlq/messages' &&
      request.params.get('limit') === '20'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});
