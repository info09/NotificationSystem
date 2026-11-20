import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { NotificationDetailComponent } from './notification-detail.component';
import { NotificationService } from '../../core/notifications/notification.service';

describe('NotificationDetailComponent', () => {
  let fixture: ComponentFixture<NotificationDetailComponent>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  async function setup(id: string | null) {
    notificationServiceSpy = jasmine.createSpyObj<NotificationService>('NotificationService', ['getNotificationById']);

    await TestBed.configureTestingModule({
      imports: [NotificationDetailComponent],
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
});
