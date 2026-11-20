import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="layout">
      <header class="header">
        <div class="brand">AdminUI</div>
        <button type="button" class="logout-button" (click)="onLogout()">Đăng xuất</button>
      </header>
      <main class="content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .layout { min-height: 100vh; background: #f8fafc; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; }
    .header { display: flex; align-items: center; justify-content: space-between; padding: 12px 24px; background: #ffffff; border-bottom: 1px solid #cbd5e1; }
    .brand { font-size: 16px; font-weight: 600; color: #0f172a; }
    .logout-button { background: #ffffff; color: #0f172a; border: 1px solid #cbd5e1; border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; }
    .logout-button:hover { background: #f1f5f9; }
    .logout-button:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
    .content { min-height: calc(100vh - 57px); }
  `]
})
export class MainLayoutComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  onLogout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
