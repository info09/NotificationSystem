import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { EndUserProfilesComponent } from './end-user-profiles.component';
import { EndUserProfileService } from '../../core/end-user-profiles/end-user-profile.service';

describe('EndUserProfilesComponent', () => {
  let fixture: ComponentFixture<EndUserProfilesComponent>;
  let serviceSpy: jasmine.SpyObj<EndUserProfileService>;

  beforeEach(async () => {
    serviceSpy = jasmine.createSpyObj<EndUserProfileService>('EndUserProfileService', [
      'getEndUserProfiles', 'getById', 'getByExternalUserId', 'create', 'update', 'lock', 'unlock'
    ]);

    await TestBed.configureTestingModule({
      imports: [EndUserProfilesComponent, NoopAnimationsModule],
      providers: [{ provide: EndUserProfileService, useValue: serviceSpy }]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(EndUserProfilesComponent);
    return fixture.componentInstance;
  }

  it('should load profiles on init and render success state', () => {
    serviceSpy.getEndUserProfiles.and.returnValue(of({
      items: [{ id: '1', externalUserId: 'ext-1', displayName: 'User 1', email: 'u1@example.com', phoneNumber: null, status: 'Active', createdAtUtc: '2026-01-01T00:00:00Z', updatedAtUtc: '2026-01-01T00:00:00Z' }],
      page: 1, pageSize: 20, totalCount: 1
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('success');
    expect(component.profiles.length).toBe(1);
  });

  it('should render empty state when no profiles returned', () => {
    serviceSpy.getEndUserProfiles.and.returnValue(of({ items: [], page: 1, pageSize: 20, totalCount: 0 }));
    const component = createComponent();
    fixture.detectChanges();
    expect(component.state).toBe('empty');
  });

  it('should render error state when loading fails', () => {
    serviceSpy.getEndUserProfiles.and.returnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
    const component = createComponent();
    fixture.detectChanges();
    expect(component.state).toBe('error');
  });
});
