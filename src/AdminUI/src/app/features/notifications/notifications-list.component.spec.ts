import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { NotificationsListComponent } from './notifications-list.component';
import { NotificationService } from '../../core/notifications/notification.service';

describe('NotificationsListComponent', () => {
  let fixture: ComponentFixture<NotificationsListComponent>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  beforeEach(async () => {
    notificationServiceSpy = jasmine.createSpyObj<NotificationService>('NotificationService', ['getNotifications']);

    await TestBed.configureTestingModule({
      imports: [NotificationsListComponent, NoopAnimationsModule],
      providers: [
        provideRouter([]),
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(NotificationsListComponent);
    return fixture.componentInstance;
  }

  it('should load notifications on init and render success state', () => {
    notificationServiceSpy.getNotifications.and.returnValue(of({
      items: [
        {
          id: '11111111-1111-1111-1111-111111111111',
          recipient: 'user@example.com',
          channel: 'Email',
          status: 'Pending',
          content: 'Hello',
          notificationTemplateId: null,
          createdAt: '2026-04-03T00:00:00Z'
        }
      ],
      totalCount: 1,
      page: 1,
      pageSize: 20
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(notificationServiceSpy.getNotifications).toHaveBeenCalled();
    expect(component.state).toBe('success');
    expect(component.items.length).toBe(1);
  });

  it('should render empty state when no notifications returned', () => {
    notificationServiceSpy.getNotifications.and.returnValue(of({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('empty');
  });

  it('should render error state when loading fails', () => {
    notificationServiceSpy.getNotifications.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('error');
    expect(component.errorMessage).toContain('Không tải được danh sách Thông báo');
  });
});
