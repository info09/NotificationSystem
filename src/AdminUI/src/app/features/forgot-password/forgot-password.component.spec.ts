import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ForgotPasswordComponent } from './forgot-password.component';
import { AuthService } from '../../core/auth/auth.service';

describe('ForgotPasswordComponent', () => {
  let fixture: ComponentFixture<ForgotPasswordComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', ['requestPasswordReset']);
    await TestBed.configureTestingModule({
      imports: [ForgotPasswordComponent, NoopAnimationsModule],
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceSpy }]
    }).compileComponents();
  });

  it('should submit request and show success state', () => {
    authServiceSpy.requestPasswordReset.and.returnValue(of({ accepted: true }));
    fixture = TestBed.createComponent(ForgotPasswordComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.form.setValue({ identifier: 'admin@example.com' });
    component.onSubmit();

    expect(authServiceSpy.requestPasswordReset).toHaveBeenCalledWith({ identifier: 'admin@example.com' });
    expect(component.state).toBe('success');
  });
});
