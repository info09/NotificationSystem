import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

/**
 * Trạng thái lỗi chuẩn với hành động thử lại.
 */
@Component({
  selector: 'app-error-state',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div class="error-state" role="alert">
      <div class="error-icon"><mat-icon>error_outline</mat-icon></div>
      <div class="error-content">
        <p class="error-message">{{ message }}</p>
        <button *ngIf="retryLabel" mat-flat-button color="primary" type="button" (click)="retry.emit()">{{ retryLabel }}</button>
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .error-state { display: flex; gap: 12px; background: #fef2f2; border: 1px solid #fecaca; border-radius: 12px; padding: 16px; color: #991b1b; }
    .error-icon { color: #dc2626; display: flex; align-items: flex-start; }
    .error-content { flex: 1; }
    .error-message { margin: 0 0 12px; font-size: 14px; line-height: 1.5; }
    .error-message:last-child { margin-bottom: 0; }
  `]
})
export class ErrorStateComponent {
  @Input() message = 'Đã xảy ra lỗi. Vui lòng thử lại.';
  @Input() retryLabel = 'Thử lại';
  @Output() retry = new EventEmitter<void>();
}
