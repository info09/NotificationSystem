import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="layout">
      <header class="header">
        <div class="header-left">
          <div class="brand">AdminUI</div>
          <nav class="nav" aria-label="Điều hướng chính">
            <a routerLink="/dashboard" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Tổng quan</a>
            <a routerLink="/notifications" routerLinkActive="active">Thông báo</a>
            <a routerLink="/dead-letter-queues" routerLinkActive="active">Hàng đợi thư chết</a>
            <a *ngIf="authService.isSystemAdministrator()" routerLink="/user-accounts" routerLinkActive="active">Tài khoản người dùng</a>
            <a *ngIf="authService.isSystemAdministrator()" routerLink="/end-user-profiles" routerLinkActive="active">Hồ sơ EndUser</a>
          </nav>
        </div>
        <button type="button" class="logout-button" (click)="onLogout()">Đăng xuất</button>
      </header>
      <main class="content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .layout { min-height: 100vh; background: #f8fafc; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; }
    .header { display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 12px 24px; background: #ffffff; border-bottom: 1px solid #cbd5e1; }
    .header-left { display: flex; align-items: center; gap: 24px; min-width: 0; }
    .brand { font-size: 16px; font-weight: 600; color: #0f172a; }
    .nav { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
    .nav a { color: #475569; text-decoration: none; font-size: 14px; padding: 6px 10px; border-radius: 6px; }
    .nav a:hover { background: #f1f5f9; color: #0f172a; }
    .nav a.active { background: #e0e7ff; color: #1d4ed8; font-weight: 600; }
    .nav a:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
    .logout-button { background: #ffffff; color: #0f172a; border: 1px solid #cbd5e1; border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; white-space: nowrap; }
    .logout-button:hover { background: #f1f5f9; }
    .logout-button:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
    .content { min-height: calc(100vh - 57px); }
  `]
})
export class MainLayoutComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  onLogout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
