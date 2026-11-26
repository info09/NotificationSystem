import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { NotificationTemplateService } from '../../core/notification-templates/notification-template.service';
import { NotificationTemplatesComponent } from './notification-templates.component';

describe('NotificationTemplatesComponent', () => {
  let fixture: ComponentFixture<NotificationTemplatesComponent>;
  let templateServiceSpy: jasmine.SpyObj<NotificationTemplateService>;

  beforeEach(async () => {
    templateServiceSpy = jasmine.createSpyObj<NotificationTemplateService>('NotificationTemplateService', [
      'getNotificationTemplates',
      'getNotificationTemplateById',
      'createNotificationTemplate',
      'updateNotificationTemplate',
      'deleteNotificationTemplate'
    ]);

    await TestBed.configureTestingModule({
      imports: [NotificationTemplatesComponent, NoopAnimationsModule],
      providers: [{ provide: NotificationTemplateService, useValue: templateServiceSpy }]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(NotificationTemplatesComponent);
    return fixture.componentInstance;
  }

  it('should load templates on init and render success state', () => {
    templateServiceSpy.getNotificationTemplates.and.returnValue(of({
      items: [
        {
          id: '11111111-1111-1111-1111-111111111111',
          templateKey: 'order-created',
          name: 'Order Created',
          description: 'Mô tả',
          channel: 'Email',
          subject: 'Subject',
          body: 'Body',
          isActive: true,
          createdAtUtc: '2026-04-27T00:00:00Z',
          updatedAtUtc: '2026-04-27T00:00:00Z'
        }
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(templateServiceSpy.getNotificationTemplates).toHaveBeenCalled();
    expect(component.state).toBe('success');
    expect(component.templates.length).toBe(1);
  });

  it('should render empty state when no templates returned', () => {
    templateServiceSpy.getNotificationTemplates.and.returnValue(of({
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('empty');
  });

  it('should render error state when loading fails', () => {
    templateServiceSpy.getNotificationTemplates.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('error');
  });
});
