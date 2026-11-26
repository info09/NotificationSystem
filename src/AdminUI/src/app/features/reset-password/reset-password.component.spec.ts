import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ResetPasswordComponent } from './reset-password.component';
import { AuthService } from '../../core/auth/auth.service';

describe('ResetPasswordComponent', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  async function setup(token: string | null) {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', ['confirmPasswordReset']);
    await TestBed.configureTestingModule({
      imports: [ResetPasswordComponent, NoopAnimationsModule],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: { get: (key: string) => (key === 'token' ? token : null) } } }
        }
      ]
    }).compileComponents();
    const fixture = TestBed.createComponent(ResetPasswordComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('should show missing-token state when token is absent', async () => {
    const fixture = await setup(null);
    expect(fixture.componentInstance.state).toBe('missing-token');
  });

  it('should submit reset and show success state', async () => {
    const fixture = await setup('token-1');
    authServiceSpy.confirmPasswordReset.and.returnValue(of({ reset: true }));
    const component = fixture.componentInstance;

    component.form.setValue({ newPassword: 'NewPassword123!', confirmPassword: 'NewPassword123!' });
    component.onSubmit();

    expect(authServiceSpy.confirmPasswordReset).toHaveBeenCalledWith({ token: 'token-1', newPassword: 'NewPassword123!' });
    expect(component.state).toBe('success');
  });
});
