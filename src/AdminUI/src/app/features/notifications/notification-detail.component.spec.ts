import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { NotificationDetailComponent } from './notification-detail.component';
import { NotificationService } from '../../core/notifications/notification.service';

describe('NotificationDetailComponent', () => {
  let fixture: ComponentFixture<NotificationDetailComponent>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  async function setup(id: string | null) {
    notificationServiceSpy = jasmine.createSpyObj<NotificationService>('NotificationService', ['getNotificationById', 'getNotificationDeliveryHistory']);
    notificationServiceSpy.getNotificationDeliveryHistory.and.returnValue(of({
      logs: [],
      attempts: { items: [], totalCount: 0, page: 1, pageSize: 20 }
    }));

    await TestBed.configureTestingModule({
      imports: [NotificationDetailComponent, NoopAnimationsModule],
      providers: [
        provideRouter([]),
        { provide: NotificationService, useValue: notificationServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: convertToParamMap(id ? { id } : {}) }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationDetailComponent);
  }

  it('should load notification detail and render success state', async () => {
    const id = '11111111-1111-1111-1111-111111111111';
    await setup(id);
    notificationServiceSpy.getNotificationById.and.returnValue(of({
      id,
      recipient: 'user@example.com',
      channel: 'Email',
      status: 'Pending',
      content: 'Hello',
      notificationTemplateId: '22222222-2222-2222-2222-222222222222',
      createdAt: '2026-04-03T00:00:00Z'
    }));

    const component = fixture.componentInstance;
    fixture.detectChanges();

    expect(notificationServiceSpy.getNotificationById).toHaveBeenCalledWith(id);
    expect(component.state).toBe('success');
    expect(component.notification?.id).toBe(id);
  });

  it('should render error state when notification is not found', async () => {
    const id = '11111111-1111-1111-1111-111111111111';
    await setup(id);
    notificationServiceSpy.getNotificationById.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 404, statusText: 'Not Found' }))
    );

    const component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.state).toBe('error');
    expect(component.errorMessage).toContain('Không tìm thấy Thông báo');
  });

  it('should render error state when id is missing', async () => {
    await setup(null);

    const component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.state).toBe('error');
    expect(component.errorMessage).toContain('không hợp lệ');
  });

  it('should render delivery history success state', async () => {
    const id = '11111111-1111-1111-1111-111111111111';
    await setup(id);
    notificationServiceSpy.getNotificationById.and.returnValue(of({
      id,
      recipient: 'user@example.com',
      channel: 'Email',
      status: 'Sent',
      content: 'Hello',
      notificationTemplateId: null,
      createdAt: '2026-04-03T00:00:00Z'
    }));
    notificationServiceSpy.getNotificationDeliveryHistory.and.returnValue(of({
      logs: [{ id: 'l1', notificationId: id, channel: 'Email', result: 'Success', errorMessage: null, retryCount: 0, correlationId: 'corr', createdAtUtc: '2026-04-03T00:00:00Z' }],
      attempts: { items: [{ id: 'a1', notificationId: id, channel: 'Email', attemptNumber: 1, result: 'Success', errorMessage: null, correlationId: 'corr', attemptedAtUtc: '2026-04-03T00:00:00Z' }], totalCount: 1, page: 1, pageSize: 20 }
    }));

    const component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.historyState).toBe('success');
    expect(component.deliveryHistory?.logs.length).toBe(1);
  });

  it('should render delivery history error state', async () => {
    const id = '11111111-1111-1111-1111-111111111111';
    await setup(id);
    notificationServiceSpy.getNotificationById.and.returnValue(of({
      id,
      recipient: 'user@example.com',
      channel: 'Email',
      status: 'Sent',
      content: 'Hello',
      notificationTemplateId: null,
      createdAt: '2026-04-03T00:00:00Z'
    }));
    notificationServiceSpy.getNotificationDeliveryHistory.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );

    const component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.historyState).toBe('error');
    expect(component.historyErrorMessage).toContain('Không tải được lịch sử gửi Thông báo');
  });
});
