import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';

type LoginViewState = 'idle' | 'submitting' | 'success' | 'error';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <main class="login-page">
      <section class="login-card" aria-labelledby="login-title">
        <h1 id="login-title">Đăng nhập</h1>
        <p class="subtitle">Truy cập AdminUI theo vai trò được phân quyền.</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate>
          <div class="field">
            <label for="userName">Tên đăng nhập <span aria-hidden="true">*</span></label>
            <input
              id="userName"
              type="text"
              formControlName="userName"
              autocomplete="username"
              [attr.aria-invalid]="showFieldError('userName')"
              [attr.aria-describedby]="showFieldError('userName') ? 'userName-error' : null"
            />
            <p *ngIf="showFieldError('userName')" id="userName-error" class="field-error">
              Vui lòng nhập tên đăng nhập.
            </p>
          </div>

          <div class="field">
            <label for="password">Mật khẩu <span aria-hidden="true">*</span></label>
            <input
              id="password"
              type="password"
              formControlName="password"
              autocomplete="current-password"
              [attr.aria-invalid]="showFieldError('password')"
              [attr.aria-describedby]="showFieldError('password') ? 'password-error' : null"
            />
            <p *ngIf="showFieldError('password')" id="password-error" class="field-error">
              Vui lòng nhập mật khẩu.
            </p>
          </div>

          <div *ngIf="state === 'error' && errorMessage" class="status status-error" role="alert">
            {{ errorMessage }}
          </div>

          <div *ngIf="state === 'success'" class="status status-success" role="status">
            Đăng nhập thành công. Đang chuyển đến Tổng quan...
          </div>

          <div class="actions">
            <button type="submit" [disabled]="state === 'submitting'">
              {{ state === 'submitting' ? 'Đang đăng nhập...' : 'Đăng nhập' }}
            </button>
          </div>
        </form>

        <p *ngIf="state === 'idle' && form.pristine" class="hint">Nhập thông tin xác thực để tiếp tục.</p>
      </section>
    </main>
  `,
  styles: [`
    .login-page { min-height: 100vh; display: grid; place-items: center; background: #f8fafc; padding: 24px; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; }
    .login-card { width: 100%; max-width: 420px; background: #ffffff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 24px; box-sizing: border-box; }
    h1 { margin: 0 0 8px; font-size: 24px; color: #0f172a; }
    .subtitle { margin: 0 0 20px; color: #475569; font-size: 14px; }
    .field { margin-bottom: 16px; }
    label { display: block; margin-bottom: 6px; color: #0f172a; font-size: 14px; font-weight: 600; }
    input { width: 100%; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 14px; box-sizing: border-box; }
    input:focus { outline: 2px solid #2563eb; outline-offset: 1px; border-color: #2563eb; }
    .field-error { margin: 6px 0 0; color: #dc2626; font-size: 12px; }
    .status { margin: 8px 0 16px; padding: 10px 12px; border-radius: 6px; font-size: 14px; }
    .status-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
    .status-success { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
    .actions { display: flex; justify-content: flex-end; }
    button { background: #2563eb; color: #fff; border: none; border-radius: 6px; padding: 10px 16px; font-size: 14px; font-weight: 600; cursor: pointer; }
    button:hover:not(:disabled) { background: #1d4ed8; }
    button:disabled { opacity: 0.7; cursor: not-allowed; }
    .hint { margin: 16px 0 0; color: #475569; font-size: 12px; }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  state: LoginViewState = 'idle';
  errorMessage = '';

  readonly form = this.fb.nonNullable.group({
    userName: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  showFieldError(controlName: 'userName' | 'password'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }

  onSubmit(): void {
    if (this.state === 'submitting') {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.state = 'idle';
      return;
    }

    this.state = 'submitting';
    this.errorMessage = '';

    const request = this.form.getRawValue();

    this.authService
      .login(request)
      .pipe(finalize(() => {
        if (this.state === 'submitting') {
          this.state = 'idle';
        }
      }))
      .subscribe({
        next: () => {
          this.state = 'success';
          void this.router.navigate(['/dashboard']);
        },
        error: (error: unknown) => {
          this.state = 'error';
          this.errorMessage = this.resolveErrorMessage(error);
        }
      });
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401 || error.status === 403) {
        return 'Thông tin xác thực không hợp lệ hoặc không có quyền truy cập. Vui lòng kiểm tra và thử lại.';
      }

      if (error.status === 0) {
        return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      }

      const detail = (error.error as { detail?: string; title?: string } | null)?.detail
        ?? (error.error as { detail?: string; title?: string } | null)?.title;
      if (detail) {
        return detail;
      }

      return 'Đăng nhập không thành công. Vui lòng thử lại.';
    }

    return 'Đăng nhập không thành công. Vui lòng thử lại.';
  }
}
