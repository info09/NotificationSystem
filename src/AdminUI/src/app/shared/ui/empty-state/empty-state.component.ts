import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

/**
 * Trạng thái rỗng chuẩn cho danh sách/khu vực dữ liệu.
 */
@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="empty-state" [attr.role]="role">
      <h2 *ngIf="title" class="empty-title">{{ title }}</h2>
      <p *ngIf="description" class="empty-description">{{ description }}</p>
      <button *ngIf="actionLabel" type="button" class="empty-action" (click)="action.emit()">{{ actionLabel }}</button>
      <ng-content></ng-content>
    </div>
  `,
  styles: [`
    .empty-state { background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; padding: 20px; color: #475569; }
    .empty-title { margin: 0 0 8px; font-size: 16px; color: #0f172a; }
    .empty-description { margin: 0 0 12px; font-size: 14px; }
    .empty-action { background: #fff; color: #0f172a; border: 1px solid #cbd5e1; border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; font-family: inherit; }
    .empty-action:hover { background: #f1f5f9; }
    .empty-action:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
  `]
})
export class EmptyStateComponent {
  @Input() title = '';
  @Input() description = '';
  @Input() actionLabel = '';
  @Input() role: string | null = null;
  @Output() action = new EventEmitter<void>();
}