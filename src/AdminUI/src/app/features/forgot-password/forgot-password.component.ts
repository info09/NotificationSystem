import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';

type ViewState = 'idle' | 'submitting' | 'success' | 'error';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <main class="login-page">
      <section class="login-card" aria-labelledby="forgot-title">
        <h1 id="forgot-title">Quên mật khẩu</h1>
        <p class="subtitle">Nhập email hoặc tên đăng nhập để nhận liên kết đặt lại mật khẩu.</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate>
          <div class="field">
            <label for="identifier">Email hoặc tên đăng nhập <span aria-hidden="true">*</span></label>
            <input id="identifier" type="text" formControlName="identifier" autocomplete="username"
              [attr.aria-invalid]="showFieldError()" [attr.aria-describedby]="showFieldError() ? 'identifier-error' : null" />
            <p *ngIf="showFieldError()" id="identifier-error" class="field-error">Vui lòng nhập email hoặc tên đăng nhập.</p>
          </div>

          <div *ngIf="state === 'error'" class="status status-error" role="alert">{{ errorMessage }}</div>
          <div *ngIf="state === 'success'" class="status status-success" role="status">
            Nếu tài khoản hợp lệ, hệ thống đã gửi email đặt lại mật khẩu. Vui lòng kiểm tra hộp thư.
          </div>

          <div class="actions">
            <a routerLink="/login" class="link">Quay lại đăng nhập</a>
            <button type="submit" [disabled]="state === 'submitting'">
              {{ state === 'submitting' ? 'Đang gửi...' : 'Gửi yêu cầu' }}
            </button>
          </div>
        </form>
      </section>
    </main>
  `,
  styles: [`
    .login-page { min-height: 100vh; display: grid; place-items: center; background: #f8fafc; padding: 24px; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; }
    .login-card { width: 100%; max-width: 460px; background: #ffffff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 24px; box-sizing: border-box; }
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
    .actions { display: flex; justify-content: space-between; gap: 12px; align-items: center; }
    button { background: #2563eb; color: #fff; border: none; border-radius: 6px; padding: 10px 16px; font-size: 14px; font-weight: 600; cursor: pointer; }
    button:hover:not(:disabled) { background: #1d4ed8; }
    button:disabled { opacity: 0.7; cursor: not-allowed; }
    .link { color: #2563eb; text-decoration: none; font-size: 14px; }
  `]
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  state: ViewState = 'idle';
  errorMessage = '';

  readonly form = this.fb.nonNullable.group({
    identifier: ['', [Validators.required]]
  });

  showFieldError(): boolean {
    const control = this.form.controls.identifier;
    return control.invalid && (control.touched || control.dirty);
  }

  onSubmit(): void {
    if (this.state === 'submitting') return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.state = 'submitting';
    this.errorMessage = '';
    const identifier = this.form.controls.identifier.value.trim();

    this.authService.requestPasswordReset({ identifier })
      .pipe(finalize(() => { if (this.state === 'submitting') this.state = 'idle'; }))
      .subscribe({
        next: () => { this.state = 'success'; },
        error: (error: unknown) => {
          this.state = 'error';
          this.errorMessage = this.resolveErrorMessage(error);
        }
      });
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      if (error.status === 400 || error.status === 422) return 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.';
    }
    return 'Không thể gửi yêu cầu đặt lại mật khẩu. Vui lòng thử lại.';
  }
}
