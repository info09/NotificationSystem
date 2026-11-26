import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';

type LoginViewState = 'idle' | 'submitting' | 'success' | 'error';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatProgressBarModule],
  template: `
    <main class="auth-page">
      <mat-card class="auth-card" aria-labelledby="login-title">
        <div class="auth-brand">
          <div class="brand-mark">NS</div>
          <div>
            <p class="brand-kicker">Notification System</p>
            <h1 id="login-title">Đăng nhập AdminUI</h1>
          </div>
        </div>
        <p class="subtitle">Truy cập hệ thống quản trị theo vai trò được phân quyền.</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate class="auth-form">
          <mat-form-field appearance="outline">
            <mat-label>Tên đăng nhập</mat-label>
            <input matInput type="text" formControlName="userName" autocomplete="username" />
            <mat-error *ngIf="showFieldError('userName')">Vui lòng nhập tên đăng nhập.</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Mật khẩu</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password" />
            <mat-error *ngIf="showFieldError('password')">Vui lòng nhập mật khẩu.</mat-error>
          </mat-form-field>

          <div *ngIf="state === 'error' && errorMessage" class="status status-error" role="alert">{{ errorMessage }}</div>
          <div *ngIf="state === 'success'" class="status status-success" role="status">Đăng nhập thành công. Đang chuyển đến Tổng quan...</div>

          <mat-progress-bar *ngIf="state === 'submitting'" mode="indeterminate"></mat-progress-bar>

          <div class="actions">
            <a routerLink="/forgot-password" class="link">Quên mật khẩu?</a>
            <button mat-flat-button color="primary" type="submit" [disabled]="state === 'submitting'">
              {{ state === 'submitting' ? 'Đang đăng nhập...' : 'Đăng nhập' }}
            </button>
          </div>
        </form>
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
    .status { padding: 12px 14px; border-radius: 10px; font-size: 14px; }
    .status-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
    .status-success { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
    .actions { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-top: 8px; }
    .link { color: var(--app-primary); text-decoration: none; font-size: 14px; font-weight: 500; }
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
