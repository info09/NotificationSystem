import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

/**
 * Vùng hiển thị toast UI feedback.
 */
@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" aria-live="polite" aria-atomic="true">
      <div *ngFor="let toast of toastService.toasts" class="toast" [ngClass]="'toast-' + toast.tone" [attr.role]="toast.tone === 'error' ? 'alert' : 'status'">
        <span class="toast-message">{{ toast.message }}</span>
        <button type="button" class="toast-close" (click)="toastService.dismiss(toast.id)" aria-label="Đóng thông báo">×</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container { position: fixed; top: 16px; right: 16px; display: grid; gap: 8px; z-index: 1100; width: min(360px, calc(100vw - 32px)); }
    .toast { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; padding: 12px 14px; border-radius: 8px; border: 1px solid #cbd5e1; background: #fff; color: #0f172a; box-shadow: 0 8px 20px rgba(15, 23, 42, 0.12); font-size: 14px; }
    .toast-success { background: #f0fdf4; border-color: #86efac; color: #166534; }
    .toast-error { background: #fef2f2; border-color: #fca5a5; color: #991b1b; }
    .toast-warning { background: #fffbeb; border-color: #fcd34d; color: #92400e; }
    .toast-info { background: #eff6ff; border-color: #93c5fd; color: #1e40af; }
    .toast-message { line-height: 1.4; }
    .toast-close { background: transparent; border: none; color: inherit; font-size: 18px; line-height: 1; cursor: pointer; padding: 0; }
    .toast-close:focus { outline: 2px solid #2563eb; outline-offset: 2px; border-radius: 4px; }
  `]
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);
}