import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DeadLetterQueueMessage } from '../../core/dead-letter-queue/dead-letter-queue.models';

export interface DeadLetterQueueDetailDialogData {
  item: DeadLetterQueueMessage;
  resendProcessing: boolean;
  resendErrorMessage: string;
  resendSuccessMessage: string;
}

export type DeadLetterQueueDetailDialogResult = 'resend' | 'close';

@Component({
  selector: 'app-dead-letter-queue-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Chi tiết Hàng đợi thư chết</h2>
    <mat-dialog-content class="dialog-content">
      <dl class="detail-grid">
        <dt>Hàng đợi thư chết</dt><dd>{{ data.item.queueName }}</dd>
        <dt>MessageId</dt><dd class="mono">{{ data.item.messageId || '—' }}</dd>
        <dt>NotificationId</dt><dd class="mono">{{ data.item.notificationId || '—' }}</dd>
        <dt>Kênh</dt><dd>{{ displayChannel(data.item.channel) }}</dd>
        <dt>Routing key</dt><dd>{{ data.item.routingKey || '—' }}</dd>
        <dt>Event type</dt><dd>{{ data.item.eventType || '—' }}</dd>
        <dt>Lý do</dt><dd>{{ data.item.reason || '—' }}</dd>
        <dt>Thời gian vào Hàng đợi thư chết</dt><dd>{{ formatDate(data.item.deadLetteredAtUtc) }}</dd>
        <dt>Số lần</dt><dd>{{ data.item.deathCount ?? '—' }}</dd>
      </dl>
      <div class="payload-block">
        <h3>Payload</h3>
        <pre>{{ data.item.payload || '—' }}</pre>
      </div>
      <div *ngIf="data.resendSuccessMessage" class="status status-success" role="status">{{ data.resendSuccessMessage }}</div>
      <div *ngIf="data.resendErrorMessage" class="status status-error" role="alert">{{ data.resendErrorMessage }}</div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close('close')" [disabled]="data.resendProcessing">Đóng</button>
      <button mat-flat-button color="primary" type="button" (click)="dialogRef.close('resend')" [disabled]="data.resendProcessing">
        {{ data.resendProcessing ? 'Đang gửi lại...' : 'Gửi lại' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-content { min-width: min(720px, 90vw); }
    .detail-grid { display: grid; grid-template-columns: 220px 1fr; gap: 10px 16px; margin: 0; }
    dt { font-size: 12px; font-weight: 600; color: var(--app-muted); text-transform: uppercase; letter-spacing: .04em; }
    dd { margin: 0; font-size: 14px; word-break: break-word; }
    .mono { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 12px; }
    .payload-block { margin-top: 16px; }
    .payload-block h3 { margin: 0 0 8px; font-size: 14px; }
    pre { margin: 0; padding: 12px; background: #f8fafc; border: 1px solid var(--app-border); border-radius: 10px; white-space: pre-wrap; word-break: break-word; font-size: 12px; font-family: ui-monospace, SFMono-Regular, Consolas, monospace; }
    .status { margin-top: 12px; padding: 12px 14px; border-radius: 10px; font-size: 14px; }
    .status-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
    .status-success { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
    @media (max-width: 768px) { .detail-grid { grid-template-columns: 1fr; } .dialog-content { min-width: auto; } }
  `]
})
export class DeadLetterQueueDetailDialogComponent {
  readonly dialogRef = inject(MatDialogRef<DeadLetterQueueDetailDialogComponent, DeadLetterQueueDetailDialogResult>);
  readonly data = inject<DeadLetterQueueDetailDialogData>(MAT_DIALOG_DATA);

  displayChannel(channel: string | null): string {
    const normalized = (channel || '').toLowerCase();
    if (normalized === 'email') return 'Email';
    if (normalized === 'inapp') return 'Thông báo trong ứng dụng';
    return channel || '—';
  }

  formatDate(value: string | null): string {
    if (!value) return '—';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    return date.toLocaleString('vi-VN', { hour12: false });
  }
}
