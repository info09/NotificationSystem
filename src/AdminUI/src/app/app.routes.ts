import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';
import { systemAdminGuard } from './core/auth/system-admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./core/layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/notifications-list.component').then(
            (m) => m.NotificationsListComponent
          )
      },
      {
        path: 'notifications/:id',
        loadComponent: () =>
          import('./features/notifications/notification-detail.component').then(
            (m) => m.NotificationDetailComponent
          )
      },
      {
        path: 'dead-letter-queues',
        loadComponent: () =>
          import('./features/dead-letter-queue/dead-letter-queue-list.component').then(
            (m) => m.DeadLetterQueueListComponent
          )
      },
      {
        path: 'user-accounts',
        canActivate: [systemAdminGuard],
        loadComponent: () =>
          import('./features/user-accounts/user-accounts.component').then(
            (m) => m.UserAccountsComponent
          )
      },
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];


