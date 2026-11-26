import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map, shareReplay } from 'rxjs';
import { AuthService } from '../auth/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  adminOnly?: boolean;
  exact?: boolean;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule
  ],
  template: `
    <mat-sidenav-container class="app-shell">
      <mat-sidenav
        #drawer
        class="app-sidenav"
        [mode]="(isHandset$ | async) ? 'over' : 'side'"
        [opened]="(isHandset$ | async) === false"
        [fixedInViewport]="(isHandset$ | async) === true"
        fixedTopGap="0"
      >
        <div class="sidenav-brand">
          <div class="brand-mark">NS</div>
          <div class="brand-text">
            <div class="brand-title">Notification System</div>
            <div class="brand-subtitle">Admin Console</div>
          </div>
        </div>

        <mat-nav-list class="sidenav-nav" aria-label="Điều hướng chính">
          <ng-container *ngFor="let item of navItems">
            <a
              mat-list-item
              *ngIf="!item.adminOnly || authService.isSystemAdministrator()"
              [routerLink]="item.route"
              routerLinkActive="active-nav"
              [routerLinkActiveOptions]="{ exact: !!item.exact }"
              (click)="closeOnMobile(drawer)"
              class="nav-item"
            >
              <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
              <span matListItemTitle>{{ item.label }}</span>
            </a>
          </ng-container>
        </mat-nav-list>

        <div class="sidenav-footer">
          <div class="footer-caption">Môi trường vận hành</div>
          <div class="footer-value">Enterprise AdminUI</div>
        </div>
      </mat-sidenav>

      <mat-sidenav-content class="app-content-shell">
        <mat-toolbar class="app-toolbar">
          <button
            mat-icon-button
            type="button"
            class="menu-button"
            aria-label="Mở menu điều hướng"
            (click)="drawer.toggle()"
            *ngIf="isHandset$ | async"
          >
            <mat-icon>menu</mat-icon>
          </button>

          <div class="toolbar-title">
            <span class="toolbar-kicker">Notification System</span>
            <span class="toolbar-heading">Bảng điều hành</span>
          </div>

          <span class="toolbar-spacer"></span>

          <button mat-stroked-button type="button" class="logout-button" (click)="onLogout()">
            <mat-icon>logout</mat-icon>
            <span>Đăng xuất</span>
          </button>
        </mat-toolbar>

        <main class="app-main">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .app-shell { min-height: 100vh; background: var(--app-bg); }
    .app-sidenav {
      width: 280px;
      background: linear-gradient(180deg, #0f172a 0%, #111827 100%);
      color: var(--sidebar-text);
      border-right: 1px solid rgba(148, 163, 184, 0.12);
    }
    .sidenav-brand {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 24px 20px;
      border-bottom: 1px solid rgba(148, 163, 184, 0.14);
    }
    .brand-mark {
      width: 40px;
      height: 40px;
      border-radius: 10px;
      display: grid;
      place-items: center;
      background: linear-gradient(135deg, #2563eb, #60a5fa);
      color: #fff;
      font-weight: 700;
      letter-spacing: -0.02em;
    }
    .brand-title { color: #f8fafc; font-size: 15px; font-weight: 700; }
    .brand-subtitle { color: #94a3b8; font-size: 12px; margin-top: 2px; }
    .sidenav-nav { padding: 12px; }
    .nav-item {
      border-radius: 10px;
      margin-bottom: 4px;
      color: var(--sidebar-text) !important;
    }
    :host ::ng-deep .app-sidenav .mat-mdc-list-item {
      color: var(--sidebar-text);
    }
    :host ::ng-deep .app-sidenav .mat-mdc-list-item .mat-mdc-list-item-icon,
    :host ::ng-deep .app-sidenav .mat-mdc-list-item .mdc-list-item__primary-text {
      color: var(--sidebar-text);
    }
    :host ::ng-deep .app-sidenav .mat-mdc-list-item:hover {
      background: rgba(148, 163, 184, 0.12);
    }
    :host ::ng-deep .app-sidenav .active-nav {
      background: rgba(37, 99, 235, 0.18) !important;
    }
    :host ::ng-deep .app-sidenav .active-nav .mat-mdc-list-item-icon,
    :host ::ng-deep .app-sidenav .active-nav .mdc-list-item__primary-text {
      color: #ffffff !important;
      font-weight: 600;
    }
    .sidenav-footer {
      margin-top: auto;
      padding: 20px;
      border-top: 1px solid rgba(148, 163, 184, 0.14);
      color: #94a3b8;
      font-size: 12px;
    }
    .footer-value { color: #e2e8f0; margin-top: 4px; font-weight: 500; }
    .app-content-shell { background: var(--app-bg); }
    .app-toolbar {
      position: sticky;
      top: 0;
      z-index: 20;
      background: rgba(255, 255, 255, 0.92);
      backdrop-filter: blur(8px);
      border-bottom: 1px solid var(--app-border);
      color: var(--app-text);
      gap: 12px;
    }
    .menu-button { margin-right: 4px; }
    .toolbar-title { display: flex; flex-direction: column; line-height: 1.2; }
    .toolbar-kicker { font-size: 11px; color: var(--app-muted); text-transform: uppercase; letter-spacing: 0.08em; }
    .toolbar-heading { font-size: 16px; font-weight: 600; }
    .toolbar-spacer { flex: 1; }
    .logout-button { border-color: var(--app-border); }
    .logout-button mat-icon { margin-right: 6px; font-size: 18px; width: 18px; height: 18px; }
    .app-main { min-height: calc(100vh - 64px); }
    @media (max-width: 768px) {
      .toolbar-heading { font-size: 15px; }
      .logout-button span { display: none; }
      .logout-button mat-icon { margin-right: 0; }
    }
  `]
})
export class MainLayoutComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly isHandset$ = this.breakpointObserver.observe('(max-width: 1023px)').pipe(
    map((result) => result.matches),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  readonly navItems: NavItem[] = [
    { label: 'Tổng quan', icon: 'dashboard', route: '/dashboard', exact: true },
    { label: 'Thông báo', icon: 'notifications', route: '/notifications' },
    { label: 'Hàng đợi thư chết', icon: 'inbox', route: '/dead-letter-queues' },
    { label: 'Tài khoản người dùng', icon: 'manage_accounts', route: '/user-accounts', adminOnly: true },
    { label: 'Hồ sơ người nhận EndUser', icon: 'groups', route: '/end-user-profiles', adminOnly: true },
    { label: 'Mẫu thông báo', icon: 'description', route: '/notification-templates', adminOnly: true },
    { label: 'Cấu hình kênh thông báo', icon: 'tune', route: '/notification-channel-configs', adminOnly: true },
    { label: 'Định tuyến sự kiện tích hợp', icon: 'alt_route', route: '/integration-event-routes', adminOnly: true }
  ];

  onLogout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }

  closeOnMobile(drawer: { close: () => void }): void {
    this.breakpointObserver.observe('(max-width: 1023px)').subscribe((state) => {
      if (state.matches) {
        drawer.close();
      }
    }).unsubscribe();
  }
}
