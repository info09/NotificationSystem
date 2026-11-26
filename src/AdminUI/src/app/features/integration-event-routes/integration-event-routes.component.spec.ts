import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { IntegrationEventRouteService } from '../../core/integration-event-routes/integration-event-route.service';
import { IntegrationEventRoutesComponent } from './integration-event-routes.component';

describe('IntegrationEventRoutesComponent', () => {
  let fixture: ComponentFixture<IntegrationEventRoutesComponent>;
  let routeServiceSpy: jasmine.SpyObj<IntegrationEventRouteService>;

  beforeEach(async () => {
    routeServiceSpy = jasmine.createSpyObj<IntegrationEventRouteService>('IntegrationEventRouteService', [
      'getIntegrationEventRoutes',
      'getIntegrationEventRouteById',
      'createIntegrationEventRoute',
      'updateIntegrationEventRoute',
      'deleteIntegrationEventRoute'
    ]);

    await TestBed.configureTestingModule({
      imports: [IntegrationEventRoutesComponent, NoopAnimationsModule],
      providers: [{ provide: IntegrationEventRouteService, useValue: routeServiceSpy }]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(IntegrationEventRoutesComponent);
    return fixture.componentInstance;
  }

  it('should load routes on init and render success state', () => {
    routeServiceSpy.getIntegrationEventRoutes.and.returnValue(of({
      items: [
        {
          id: '11111111-1111-1111-1111-111111111111',
          eventType: 'OrderCreated',
          notificationTemplateId: null,
          channel: 'Email',
          description: 'Mô tả',
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

    expect(routeServiceSpy.getIntegrationEventRoutes).toHaveBeenCalled();
    expect(component.state).toBe('success');
    expect(component.routes.length).toBe(1);
  });

  it('should render empty state when no routes returned', () => {
    routeServiceSpy.getIntegrationEventRoutes.and.returnValue(of({
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
    routeServiceSpy.getIntegrationEventRoutes.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('error');
  });
});
