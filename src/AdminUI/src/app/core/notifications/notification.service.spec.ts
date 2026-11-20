import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NotificationService } from './notification.service';
import { GetNotificationsResponse, NotificationItem } from './notification.models';

describe('NotificationService', () => {
  let service: NotificationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request notifications with paging and filters', () => {
    const mockResponse: GetNotificationsResponse = {
      items: [],
      totalCount: 0,
      page: 2,
      pageSize: 20
    };

    service
      .getNotifications({
        status: 'Pending',
        channel: 'email',
        createdFrom: '2026-04-01T00:00:00.000Z',
        createdTo: '2026-04-02T00:00:00.000Z',
        page: 2,
        pageSize: 20
      })
      .subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

    const req = httpMock.expectOne((request) =>
      request.url === '/api/notifications' &&
      request.params.get('page') === '2' &&
      request.params.get('pageSize') === '20' &&
      request.params.get('status') === 'Pending' &&
      request.params.get('channel') === 'email' &&
      request.params.get('createdFrom') === '2026-04-01T00:00:00.000Z' &&
      request.params.get('createdTo') === '2026-04-02T00:00:00.000Z'
    );

    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should request notification detail by id', () => {
    const mockItem: NotificationItem = {
      id: '11111111-1111-1111-1111-111111111111',
      recipient: 'user@example.com',
      channel: 'Email',
      status: 'Pending',
      content: 'Hello',
      notificationTemplateId: null,
      createdAt: '2026-04-03T00:00:00Z'
    };

    service.getNotificationById(mockItem.id).subscribe((response) => {
      expect(response).toEqual(mockItem);
    });

    const req = httpMock.expectOne(`/api/notifications/${mockItem.id}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockItem);
  });
});
