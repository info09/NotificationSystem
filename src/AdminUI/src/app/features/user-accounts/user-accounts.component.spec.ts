import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { UserAccountsComponent } from './user-accounts.component';
import { UserAccountService } from '../../core/user-accounts/user-account.service';

describe('UserAccountsComponent', () => {
  let fixture: ComponentFixture<UserAccountsComponent>;
  let userAccountServiceSpy: jasmine.SpyObj<UserAccountService>;

  beforeEach(async () => {
    userAccountServiceSpy = jasmine.createSpyObj<UserAccountService>('UserAccountService', [
      'getUserAccounts',
      'getUserAccountById',
      'createUserAccount',
      'updateUserAccount',
      'lockUserAccount',
      'unlockUserAccount',
      'deleteUserAccount',
      'getCredentials',
      'createCredential',
      'rotateCredential',
      'revokeCredential'
    ]);

    await TestBed.configureTestingModule({
      imports: [UserAccountsComponent, NoopAnimationsModule],
      providers: [{ provide: UserAccountService, useValue: userAccountServiceSpy }]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(UserAccountsComponent);
    return fixture.componentInstance;
  }

  it('should load accounts on init and render success state', () => {
    userAccountServiceSpy.getUserAccounts.and.returnValue(of({
      items: [
        {
          id: '11111111-1111-1111-1111-111111111111',
          userName: 'admin1',
          email: 'admin1@example.com',
          displayName: 'Admin 1',
          role: 'SystemAdministrator',
          status: 'Active',
          createdAtUtc: '2026-04-03T00:00:00Z',
          updatedAtUtc: '2026-04-03T00:00:00Z'
        }
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1
    }));

    const component = createComponent();
    fixture.detectChanges();

    expect(userAccountServiceSpy.getUserAccounts).toHaveBeenCalled();
    expect(component.state).toBe('success');
    expect(component.accounts.length).toBe(1);
  });

  it('should render empty state when no accounts returned', () => {
    userAccountServiceSpy.getUserAccounts.and.returnValue(of({
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
    userAccountServiceSpy.getUserAccounts.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500, statusText: 'Server Error' }))
    );

    const component = createComponent();
    fixture.detectChanges();

    expect(component.state).toBe('error');
  });
});
