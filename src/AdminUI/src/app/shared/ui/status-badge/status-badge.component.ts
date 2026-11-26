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
    .status-badge { display: inline-flex; align-items: center; padding: 4px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; border: 1px solid transparent; white-space: nowrap; letter-spacing: 0.01em; }
    .tone-pending, .tone-warning { background: #fef3c7; color: #92400e; border-color: #fde68a; }
    .tone-processing, .tone-info { background: #dbeafe; color: #1e40af; border-color: #bfdbfe; }
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
