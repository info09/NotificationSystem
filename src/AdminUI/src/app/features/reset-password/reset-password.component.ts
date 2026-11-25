import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';

type ViewState = 'idle' | 'submitting' | 'success' | 'error' | 'missing-token';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <main class="login-page">
      <section class="login-card" aria-labelledby="reset-title">
        <h1 id="reset-title">Đặt lại mật khẩu</h1>
        <p class="subtitle">Nhập mật khẩu mới cho tài khoản của bạn.</p>

        <div *ngIf="state === 'missing-token'" class="status status-error" role="alert">
          Liên kết đặt lại mật khẩu không hợp lệ hoặc thiếu token.
        </div>

        <form *ngIf="state !== 'missing-token'" [formGroup]="form" (ngSubmit)="onSubmit()" novalidate>
          <div class="field">
            <label for="newPassword">Mật khẩu mới <span aria-hidden="true">*</span></label>
            <input id="newPassword" type="password" formControlName="newPassword" autocomplete="new-password" />
            <p *ngIf="showFieldError('newPassword')" class="field-error">Mật khẩu mới phải có ít nhất 8 ký tự.</p>
          </div>

          <div class="field">
            <label for="confirmPassword">Xác nhận mật khẩu <span aria-hidden="true">*</span></label>
            <input id="confirmPassword" type="password" formControlName="confirmPassword" autocomplete="new-password" />
            <p *ngIf="showFieldError('confirmPassword')" class="field-error">Vui lòng xác nhận mật khẩu.</p>
            <p *ngIf="passwordMismatch" class="field-error">Mật khẩu xác nhận không khớp.</p>
          </div>

          <div *ngIf="state === 'error'" class="status status-error" role="alert">{{ errorMessage }}</div>
          <div *ngIf="state === 'success'" class="status status-success" role="status">
            Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.
          </div>

          <div class="actions">
            <a routerLink="/login" class="link">Quay lại đăng nhập</a>
            <button type="submit" [disabled]="state === 'submitting' || state === 'success'">
              {{ state === 'submitting' ? 'Đang xử lý...' : 'Đặt lại mật khẩu' }}
            </button>
          </div>
        </form>

        <div *ngIf="state === 'missing-token'" class="actions">
          <a routerLink="/forgot-password" class="link">Yêu cầu liên kết mới</a>
          <a routerLink="/login" class="link">Đăng nhập</a>
        </div>
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
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  state: ViewState = 'idle';
  errorMessage = '';
  token = '';
  passwordMismatch = false;

  readonly form = this.fb.nonNullable.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.token = (this.route.snapshot.queryParamMap.get('token') ?? '').trim();
    if (!this.token) {
      this.state = 'missing-token';
    }
  }

  showFieldError(controlName: 'newPassword' | 'confirmPassword'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }

  onSubmit(): void {
    if (this.state === 'submitting' || this.state === 'success') return;
    this.passwordMismatch = false;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { newPassword, confirmPassword } = this.form.getRawValue();
    if (newPassword !== confirmPassword) {
      this.passwordMismatch = true;
      return;
    }

    this.state = 'submitting';
    this.errorMessage = '';

    this.authService.confirmPasswordReset({ token: this.token, newPassword })
      .pipe(finalize(() => { if (this.state === 'submitting') this.state = 'idle'; }))
      .subscribe({
        next: () => { this.state = 'success'; this.form.disable(); },
        error: (error: unknown) => {
          this.state = 'error';
          this.errorMessage = this.resolveErrorMessage(error);
        }
      });
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      if (error.status === 400 || error.status === 422) return 'Token không hợp lệ, đã hết hạn, hoặc mật khẩu không đáp ứng yêu cầu.';
    }
    return 'Không thể đặt lại mật khẩu. Vui lòng thử lại.';
  }
}
