import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { NotificationChannelConfigService } from '../../core/notification-channel-configs/notification-channel-config.service';
import { NotificationChannelConfigsComponent } from './notification-channel-configs.component';

describe('NotificationChannelConfigsComponent', () => {
  let fixture: ComponentFixture<NotificationChannelConfigsComponent>;
  let configServiceSpy: jasmine.SpyObj<NotificationChannelConfigService>;

  beforeEach(async () => {
    configServiceSpy = jasmine.createSpyObj<NotificationChannelConfigService>('NotificationChannelConfigService', [
      'getNotificationChannelConfigs',
      'getNotificationChannelConfigById',
      'createNotificationChannelConfig',
      'updateNotificationChannelConfig',
      'deleteNotificationChannelConfig'
    ]);

    await TestBed.configureTestingModule({
      imports: [NotificationChannelConfigsComponent, NoopAnimationsModule],
      providers: [{ provide: NotificationChannelConfigService, useValue: configServiceSpy }]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(NotificationChannelConfigsComponent);
    return fixture.componentInstance;
  }

  it('should load configs on init and render success state', () => {
    configServiceSpy.getNotificationChannelConfigs.and.returnValue(of({
      items: [
        {
          id: '11111111-1111-1111-1111-111111111111',
          channel: 'Email',
          displayName: 'Email',
          description: 'Mô tả',
          isEnabled: true,
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

    expect(configServiceSpy.getNotificationChannelConfigs).toHaveBeenCalled();
    expect(component.state).toBe('success');
    expect(component.configs.length).toBe(1);
  });

  it('should render empty state when no configs returned', () => {
    configServiceSpy.getNotificationChannelConfigs.and.returnValue(of({
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
    configServiceSpy.getNotificationChannelConfigs.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('error');
  });
});
