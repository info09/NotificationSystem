import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

/**
 * Trạng thái lỗi chuẩn với hành động thử lại.
 */
@Component({
  selector: 'app-error-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="error-state" role="alert">
      <p class="error-message">{{ message }}</p>
      <button *ngIf="retryLabel" type="button" class="retry-button" (click)="retry.emit()">{{ retryLabel }}</button>
      <ng-content></ng-content>
    </div>
  `,
  styles: [`
    .error-state { background: #fee2e2; border: 1px solid #fecaca; border-radius: 8px; padding: 16px; color: #991b1b; }
    .error-message { margin: 0 0 12px; font-size: 14px; }
    .error-message:last-child { margin-bottom: 0; }
    .retry-button { background: #2563eb; color: #fff; border: 1px solid #2563eb; border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; font-family: inherit; }
    .retry-button:hover { background: #1d4ed8; }
    .retry-button:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
  `]
})
export class ErrorStateComponent {
  @Input() message = 'Đã xảy ra lỗi. Vui lòng thử lại.';
  @Input() retryLabel = 'Thử lại';
  @Output() retry = new EventEmitter<void>();
}