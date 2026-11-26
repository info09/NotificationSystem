import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';

type ViewState = 'idle' | 'submitting' | 'success' | 'error' | 'missing-token';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressBarModule],
  template: `
    <main class="auth-page">
      <mat-card class="auth-card" aria-labelledby="reset-title">
        <div class="auth-brand">
          <div class="brand-mark">NS</div>
          <div>
            <p class="brand-kicker">Notification System</p>
            <h1 id="reset-title">Đặt lại mật khẩu</h1>
          </div>
        </div>
        <p class="subtitle">Nhập mật khẩu mới cho tài khoản của bạn.</p>

        <div *ngIf="state === 'missing-token'" class="status status-error" role="alert">
          Liên kết đặt lại mật khẩu không hợp lệ hoặc thiếu token.
        </div>

        <form *ngIf="state !== 'missing-token'" [formGroup]="form" (ngSubmit)="onSubmit()" novalidate class="auth-form">
          <mat-form-field appearance="outline">
            <mat-label>Mật khẩu mới</mat-label>
            <input matInput type="password" formControlName="newPassword" autocomplete="new-password" />
            <mat-error *ngIf="showFieldError('newPassword')">Mật khẩu mới phải có ít nhất 8 ký tự.</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Xác nhận mật khẩu</mat-label>
            <input matInput type="password" formControlName="confirmPassword" autocomplete="new-password" />
            <mat-error *ngIf="showFieldError('confirmPassword')">Vui lòng xác nhận mật khẩu.</mat-error>
          </mat-form-field>

          <p *ngIf="passwordMismatch" class="field-error">Mật khẩu xác nhận không khớp.</p>
          <div *ngIf="state === 'error'" class="status status-error" role="alert">{{ errorMessage }}</div>
          <div *ngIf="state === 'success'" class="status status-success" role="status">
            Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.
          </div>

          <mat-progress-bar *ngIf="state === 'submitting'" mode="indeterminate"></mat-progress-bar>

          <div class="actions">
            <a routerLink="/login" class="link">Quay lại đăng nhập</a>
            <button mat-flat-button color="primary" type="submit" [disabled]="state === 'submitting' || state === 'success'">
              {{ state === 'submitting' ? 'Đang xử lý...' : 'Đặt lại mật khẩu' }}
            </button>
          </div>
        </form>

        <div *ngIf="state === 'missing-token'" class="actions">
          <a routerLink="/forgot-password" class="link">Yêu cầu liên kết mới</a>
          <a routerLink="/login" class="link">Đăng nhập</a>
        </div>
      </mat-card>
    </main>
  `,
  styles: [`
    .auth-page { min-height: 100vh; display: grid; place-items: center; padding: 24px; background: linear-gradient(180deg, #f8fafc 0%, #e2e8f0 100%); }
    .auth-card { width: 100%; max-width: 460px; padding: 32px; border-radius: 20px !important; border: 1px solid var(--app-border); box-shadow: 0 20px 40px rgba(15,23,42,0.08) !important; }
    .auth-brand { display: flex; gap: 14px; align-items: center; margin-bottom: 8px; }
    .brand-mark { width: 44px; height: 44px; border-radius: 12px; display: grid; place-items: center; background: linear-gradient(135deg,#2563eb,#60a5fa); color:#fff; font-weight:700; }
    .brand-kicker { margin:0; font-size:12px; color:var(--app-muted); text-transform:uppercase; letter-spacing:.08em; }
    h1 { margin: 2px 0 0; font-size: 28px; line-height: 1.2; letter-spacing: -0.02em; }
    .subtitle { margin: 0 0 24px; color: var(--app-muted); font-size: 14px; }
    .auth-form { display: grid; gap: 8px; }
    .field-error { margin: -4px 0 8px; color: #dc2626; font-size: 12px; }
    .status { padding: 12px 14px; border-radius: 10px; font-size: 14px; }
    .status-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
    .status-success { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
    .actions { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-top: 8px; }
    .link { color: var(--app-primary); text-decoration: none; font-size: 14px; font-weight: 500; }
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
