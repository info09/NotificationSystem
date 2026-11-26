import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

/**
 * Trạng thái rỗng chuẩn cho danh sách/khu vực dữ liệu.
 */
@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div class="empty-state" [attr.role]="role">
      <div class="empty-icon"><mat-icon>inbox</mat-icon></div>
      <h2 *ngIf="title" class="empty-title">{{ title }}</h2>
      <p *ngIf="description" class="empty-description">{{ description }}</p>
      <button *ngIf="actionLabel" mat-stroked-button type="button" (click)="action.emit()">{{ actionLabel }}</button>
      <ng-content></ng-content>
    </div>
  `,
  styles: [`
    .empty-state { background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 12px; padding: 28px 20px; color: #475569; text-align: center; }
    .empty-icon { width: 48px; height: 48px; border-radius: 12px; background: #e2e8f0; color: #475569; display: inline-flex; align-items: center; justify-content: center; margin-bottom: 12px; }
    .empty-title { margin: 0 0 8px; font-size: 16px; color: #0f172a; font-weight: 600; }
    .empty-description { margin: 0 0 16px; font-size: 14px; }
  `]
})
export class EmptyStateComponent {
  @Input() title = '';
  @Input() description = '';
  @Input() actionLabel = '';
  @Input() role: string | null = null;
  @Output() action = new EventEmitter<void>();
}
