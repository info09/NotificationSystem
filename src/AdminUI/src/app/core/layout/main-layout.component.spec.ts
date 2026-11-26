import { BreakpointObserver } from '@angular/cdk/layout';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { MainLayoutComponent } from './main-layout.component';

describe('MainLayoutComponent', () => {
  let fixture: ComponentFixture<MainLayoutComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', ['logout', 'isSystemAdministrator']);
    authServiceSpy.isSystemAdministrator.and.returnValue(false);

    await TestBed.configureTestingModule({
      imports: [MainLayoutComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: BreakpointObserver, useValue: { observe: () => of({ matches: false, breakpoints: {} }) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MainLayoutComponent);
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);
    fixture.detectChanges();
  });

  it('should render main navigation items', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Tổng quan');
    expect(text).toContain('Thông báo');
    expect(text).toContain('Hàng đợi thư chết');
  });

  it('should hide admin navigation for non-admin user', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).not.toContain('Tài khoản người dùng');
    expect(text).not.toContain('Hồ sơ người nhận EndUser');
  });

  it('should logout and navigate to login', () => {
    fixture.componentInstance.onLogout();
    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
