import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export type StatusBadgeTone = 'pending' | 'processing' | 'sent' | 'failed' | 'unread' | 'read' | 'default' | 'info' | 'success' | 'warning' | 'danger';

/**
 * Badge hiển thị trạng thái theo chuẩn UI.
 */
@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="status-badge" [ngClass]="'tone-' + tone">{{ label }}</span>`,
  styles: [`
    .status-badge { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: 12px; font-weight: 600; border: 1px solid transparent; white-space: nowrap; }
    .tone-pending, .tone-warning { background: #fffbeb; color: #92400e; border-color: #fde68a; }
    .tone-processing, .tone-info { background: #e0f2fe; color: #075985; border-color: #bae6fd; }
    .tone-sent, .tone-success, .tone-read { background: #dcfce7; color: #166534; border-color: #bbf7d0; }
    .tone-failed, .tone-danger { background: #fee2e2; color: #991b1b; border-color: #fecaca; }
    .tone-unread { background: #e0e7ff; color: #3730a3; border-color: #c7d2fe; }
    .tone-default { background: #f1f5f9; color: #334155; border-color: #cbd5e1; }
  `]
})
export class StatusBadgeComponent {
  @Input() label = '';
  @Input() tone: StatusBadgeTone = 'default';
}