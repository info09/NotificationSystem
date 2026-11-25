import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DashboardComponent } from './dashboard.component';
import { NotificationService } from '../../core/notifications/notification.service';
import { DeadLetterQueueService } from '../../core/dead-letter-queue/dead-letter-queue.service';

describe('DashboardComponent', () => {
  let fixture: ComponentFixture<DashboardComponent>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;
  let deadLetterQueueServiceSpy: jasmine.SpyObj<DeadLetterQueueService>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    notificationServiceSpy = jasmine.createSpyObj<NotificationService>('NotificationService', ['getNotifications']);
    deadLetterQueueServiceSpy = jasmine.createSpyObj<DeadLetterQueueService>('DeadLetterQueueService', ['getMessages']);

    await TestBed.configureTestingModule({
      imports: [DashboardComponent, HttpClientTestingModule],
      providers: [
        provideRouter([]),
        { provide: NotificationService, useValue: notificationServiceSpy },
        { provide: DeadLetterQueueService, useValue: deadLetterQueueServiceSpy }
      ]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function setupSuccess(): void {
    notificationServiceSpy.getNotifications.and.callFake((query) => {
      const countByStatus: Record<string, number> = { Pending: 2, Processing: 1, Sent: 5, Failed: 0 };
      return of({ items: [], totalCount: countByStatus[query.status ?? ''] ?? 0, page: 1, pageSize: 1 });
    });
    deadLetterQueueServiceSpy.getMessages.and.callFake((queueName: string) => {
      const count = queueName === 'notifications.email.dlq' ? 3 : 0;
      return of({ queueName, items: [], count });
    });
  }

  it('should render success state when data is loaded', () => {
    setupSuccess();

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    const healthReq = httpMock.expectOne('/health/ready');
    healthReq.flush('Healthy');

    const component = fixture.componentInstance;
    expect(component.notificationsState).toBe('success');
    expect(component.dlqState).toBe('success');
    expect(component.healthState).toBe('success');
    expect(component.dlqTotal).toBe(3);
  });

  it('should render empty notification state when no notifications exist', () => {
    notificationServiceSpy.getNotifications.and.returnValue(of({ items: [], totalCount: 0, page: 1, pageSize: 1 }));
    deadLetterQueueServiceSpy.getMessages.and.returnValue(of({ queueName: 'notifications.email.dlq', items: [], count: 0 }));
    deadLetterQueueServiceSpy.getMessages.and.callFake((queueName: string) =>
      of({ queueName, items: [], count: 0 })
    );

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    const healthReq = httpMock.expectOne('/health/ready');
    healthReq.flush('Healthy');

    const component = fixture.componentInstance;
    expect(component.notificationsState).toBe('empty');
  });

  it('should render error states when requests fail', () => {
    notificationServiceSpy.getNotifications.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );
    deadLetterQueueServiceSpy.getMessages.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 403, statusText: 'Forbidden' }))
    );

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    const healthReq = httpMock.expectOne('/health/ready');
    healthReq.flush('Unhealthy', { status: 503, statusText: 'Service Unavailable' });

    const component = fixture.componentInstance;
    expect(component.notificationsState).toBe('error');
    expect(component.dlqState).toBe('error');
    expect(component.healthState).toBe('success');
    expect(component.healthToneClass).toBe('health-bad');
  });
});
`