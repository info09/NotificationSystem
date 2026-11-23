import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { DeadLetterQueueListComponent } from './dead-letter-queue-list.component';
import { DeadLetterQueueService } from '../../core/dead-letter-queue/dead-letter-queue.service';

describe('DeadLetterQueueListComponent', () => {
  let fixture: ComponentFixture<DeadLetterQueueListComponent>;
  let serviceSpy: jasmine.SpyObj<DeadLetterQueueService>;

  beforeEach(async () => {
    serviceSpy = jasmine.createSpyObj<DeadLetterQueueService>('DeadLetterQueueService', ['getMessages']);

    await TestBed.configureTestingModule({
      imports: [DeadLetterQueueListComponent],
      providers: [{ provide: DeadLetterQueueService, useValue: serviceSpy }]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(DeadLetterQueueListComponent);
    return fixture.componentInstance;
  }

  it('should load messages and render success state', () => {
    serviceSpy.getMessages.and.returnValue(of({
      queueName: 'notifications.email.dlq',
      count: 1,
      items: [{
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
      }]
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(serviceSpy.getMessages).toHaveBeenCalled();
    expect(component.state).toBe('success');
    expect(component.filteredItems.length).toBe(1);
  });

  it('should render empty state when no messages returned', () => {
    serviceSpy.getMessages.and.returnValue(of({
      queueName: 'notifications.email.dlq',
      count: 0,
      items: []
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('empty');
  });

  it('should render error state when loading fails', () => {
    serviceSpy.getMessages.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('error');
    expect(component.errorMessage).toContain('Không tải được danh sách Hàng đợi thư chết');
  });
});
