import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { DashboardComponent } from './dashboard.component';

describe('DashboardComponent', () => {
  let fixture: ComponentFixture<DashboardComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent, HttpClientTestingModule],
      providers: [provideRouter([])]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function flushSummary(overrides?: Partial<{
    notifications: { pending: number; processing: number; sent: number; failed: number; error?: string | null };
    deadLetterQueues: Array<{ queueName: string; count: number; error?: string | null }>;
    health: { status: string; details?: string | null };
  }>): void {
    const req = httpMock.expectOne('/api/admin/dashboard/summary');
    req.flush({
      notifications: { pending: 2, processing: 1, sent: 5, failed: 0, error: null },
      deadLetterQueues: [
        { queueName: 'notifications.email.dlq', count: 3, error: null },
        { queueName: 'notifications.inapp.dlq', count: 0, error: null }
      ],
      health: { status: 'Healthy', details: 'database:Healthy' },
      ...overrides
    });
  }

  it('should render success state when summary is loaded', () => {
    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    flushSummary();

    const component = fixture.componentInstance;
    expect(component.notificationsState).toBe('success');
    expect(component.dlqState).toBe('success');
    expect(component.healthState).toBe('success');
    expect(component.dlqTotal).toBe(3);
    expect(component.notificationCards.length).toBe(4);
  });

  it('should render empty notification state when no notifications exist', () => {
    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    flushSummary({
      notifications: { pending: 0, processing: 0, sent: 0, failed: 0, error: null }
    });

    const component = fixture.componentInstance;
    expect(component.notificationsState).toBe('empty');
    expect(component.dlqState).toBe('success');
  });

  it('should render per-block errors from summary response', () => {
    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    flushSummary({
      notifications: { pending: 0, processing: 0, sent: 0, failed: 0, error: 'Không thể tải tổng hợp Trạng thái thông báo.' },
      deadLetterQueues: [{ queueName: 'notifications.email.dlq', count: 0, error: 'Không thể tải tổng hợp Hàng đợi thư chết.' }],
      health: { status: 'Unhealthy', details: 'database:Unhealthy' }
    });

    const component = fixture.componentInstance;
    expect(component.notificationsState).toBe('error');
    expect(component.dlqState).toBe('error');
    expect(component.healthState).toBe('success');
    expect(component.healthToneClass).toBe('health-bad');
  });

  it('should render error states when summary request fails', () => {
    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    const req = httpMock.expectOne('/api/admin/dashboard/summary');
    req.flush({ detail: 'Forbidden' }, { status: 403, statusText: 'Forbidden' });

    const component = fixture.componentInstance;
    expect(component.notificationsState).toBe('error');
    expect(component.dlqState).toBe('error');
    expect(component.healthState).toBe('error');
    expect(component.notificationsError).toContain('quyền');
  });

  it('should refresh summary when refresh is called', () => {
    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();
    flushSummary();

    fixture.componentInstance.refresh();
    flushSummary();

    expect(fixture.componentInstance.notificationsState).toBe('success');
  });
});
