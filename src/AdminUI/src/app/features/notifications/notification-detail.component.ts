import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NotificationService } from '../../core/notifications/notification.service';
import { NotificationItem } from '../../core/notifications/notification.models';

type ViewState = 'loading' | 'success' | 'error';

@Component({
  selector: 'app-notification-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="page" aria-labelledby="notification-detail-title">
      <header class="page-header">
        <div>
          <a routerLink="/notifications" class="back-link">← Quay lại danh sách Thông báo</a>
          <h1 id="notification-detail-title">Chi tiết Thông báo</h1>
          <p class="subtitle">Thông tin, Trạng thái thông báo và Mã tương quan.</p>
        </div>
      </header>

      <div *ngIf="state === 'loading'" class="card" aria-busy="true" aria-live="polite">
        <div class="skeleton title"></div>
        <div class="skeleton line" *ngFor="let row of skeletonRows"></div>
      </div>

      <div *ngIf="state === 'error'" class="status-box status-error" role="alert">
        <h2>Không tải được chi tiết Thông báo</h2>
        <p>{{ errorMessage }}</p>
        <div class="actions">
          <button type="button" class="primary" (click)="retry()">Thử lại</button>
          <a routerLink="/notifications" class="secondary-link">Quay lại danh sách</a>
        </div>
      </div>

      <div *ngIf="state === 'success' && notification as item" class="card">
        <div class="card-header">
          <div>
            <h2>Thông tin Thông báo</h2>
            <p class="muted">Mã Thông báo: <span class="mono">{{ item.id }}</span></p>
          </div>
          <span class="badge" [ngClass]="statusClass(item.status)">
            {{ displayStatus(item.status) }}
          </span>
        </div>

        <dl class="details-grid">
          <div class="detail-item">
            <dt>Mã Thông báo (Mã tương quan)</dt>
            <dd class="mono">{{ item.id }}</dd>
          </div>
          <div class="detail-item">
            <dt>Người nhận</dt>
            <dd>{{ item.recipient || '—' }}</dd>
          </div>
          <div class="detail-item">
            <dt>Kênh thông báo</dt>
            <dd>{{ displayChannel(item.channel) }}</dd>
          </div>
          <div class="detail-item">
            <dt>Trạng thái thông báo</dt>
            <dd>{{ displayStatus(item.status) }}</dd>
          </div>
          <div class="detail-item">
            <dt>Thời gian tạo</dt>
            <dd>{{ formatDate(item.createdAt) }}</dd>
          </div>
          <div class="detail-item">
            <dt>Mã mẫu thông báo (Mã tương quan)</dt>
            <dd class="mono">{{ item.notificationTemplateId || '—' }}</dd>
          </div>
          <div class="detail-item full">
            <dt>Nội dung</dt>
            <dd class="content">{{ item.content || '—' }}</dd>
          </div>
        </dl>
      </div>
    </section>
  `,
  styles: [`
    .page { padding: 24px; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; color: #0f172a; }
    .page-header { margin-bottom: 16px; }
    .back-link { display: inline-block; margin-bottom: 8px; color: #2563eb; text-decoration: none; font-size: 14px; }
    .back-link:hover { text-decoration: underline; }
    .back-link:focus { outline: 2px solid #2563eb; outline-offset: 2px; border-radius: 4px; }
    h1 { margin: 0 0 4px; font-size: 24px; font-weight: 600; }
    .subtitle { margin: 0; color: #475569; font-size: 14px; }
    .card, .status-box { background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 20px; }
    .status-error { border-color: #fecaca; background: #fef2f2; }
    .status-error h2 { margin: 0 0 8px; font-size: 18px; color: #991b1b; }
    .status-error p { margin: 0 0 12px; color: #991b1b; }
    .actions { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; }
    .primary { background: #2563eb; color: #fff; border: 1px solid #2563eb; border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; font-family: inherit; }
    .primary:hover { background: #1d4ed8; }
    .secondary-link { display: inline-block; background: #fff; color: #0f172a; border: 1px solid #cbd5e1; border-radius: 6px; padding: 8px 12px; font-size: 14px; text-decoration: none; }
    .secondary-link:hover { background: #f1f5f9; }
    .card-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; margin-bottom: 16px; flex-wrap: wrap; }
    .card-header h2 { margin: 0 0 4px; font-size: 18px; }
    .muted { margin: 0; color: #475569; font-size: 14px; }
    .details-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 16px; margin: 0; }
    .detail-item { margin: 0; }
    .detail-item.full { grid-column: 1 / -1; }
    dt { font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 4px; }
    dd { margin: 0; font-size: 14px; color: #0f172a; word-break: break-word; }
    .content { white-space: pre-wrap; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 12px; }
    .mono { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 13px; }
    .badge { display: inline-block; padding: 4px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; border: 1px solid transparent; white-space: nowrap; }
    .badge-pending { background: #fffbeb; color: #92400e; border-color: #fde68a; }
    .badge-processing { background: #e0f2fe; color: #075985; border-color: #bae6fd; }
    .badge-sent { background: #dcfce7; color: #166534; border-color: #bbf7d0; }
    .badge-failed { background: #fee2e2; color: #991b1b; border-color: #fecaca; }
    .badge-default { background: #f1f5f9; color: #334155; border-color: #cbd5e1; }
    .skeleton { border-radius: 6px; background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%); background-size: 200% 100%; animation: shimmer 1.2s infinite; }
    .skeleton.title { height: 24px; width: 260px; margin-bottom: 16px; }
    .skeleton.line { height: 18px; margin-bottom: 12px; }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }
  `]
})
export class NotificationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly notificationService = inject(NotificationService);

  readonly skeletonRows = Array.from({ length: 6 });

  state: ViewState = 'loading';
  errorMessage = '';
  notification: NotificationItem | null = null;
  private notificationId = '';

  ngOnInit(): void {
    this.notificationId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadNotification();
  }

  retry(): void {
    this.loadNotification();
  }

  displayChannel(channel: string): string {
    const normalized = (channel || '').toLowerCase();
    if (normalized === 'email') {
      return 'Email';
    }
    if (normalized === 'inapp') {
      return 'Thông báo trong ứng dụng';
    }
    return channel || '—';
  }

  displayStatus(status: string): string {
    switch (status) {
      case 'Pending':
        return 'Pending (Chờ xử lý)';
      case 'Processing':
        return 'Processing (Đang xử lý)';
      case 'Sent':
        return 'Sent (Đã gửi)';
      case 'Failed':
        return 'Failed (Thất bại)';
      default:
        return status || '—';
    }
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'badge-pending';
      case 'Processing':
        return 'badge-processing';
      case 'Sent':
        return 'badge-sent';
      case 'Failed':
        return 'badge-failed';
      default:
        return 'badge-default';
    }
  }

  formatDate(value: string): string {
    if (!value) {
      return '—';
    }
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }
    return date.toLocaleString('vi-VN', { hour12: false });
  }

  private loadNotification(): void {
    if (!this.notificationId) {
      this.state = 'error';
      this.errorMessage = 'Mã Thông báo không hợp lệ.';
      return;
    }

    this.state = 'loading';
    this.errorMessage = '';
    this.notification = null;

    this.notificationService
      .getNotificationById(this.notificationId)
      .pipe(finalize(() => {}))
      .subscribe({
        next: (item) => {
          this.notification = item;
          this.state = 'success';
        },
        error: (error: unknown) => {
          this.state = 'error';
          this.errorMessage = this.resolveErrorMessage(error);
        }
      });
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 404) {
        return 'Không tìm thấy Thông báo hoặc không có quyền truy cập.';
      }
      if (error.status === 401 || error.status === 403) {
        return 'Không có quyền truy cập chi tiết Thông báo. Vui lòng kiểm tra phiên đăng nhập và quyền RBAC.';
      }
      if (error.status === 0) {
        return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      }
      const payload = error.error as { detail?: string; title?: string } | null;
      if (payload?.detail) {
        return payload.detail;
      }
      if (payload?.title) {
        return payload.title;
      }
      return 'Không tải được chi tiết Thông báo. Vui lòng thử lại.';
    }
    return 'Không tải được chi tiết Thông báo. Vui lòng thử lại.';
  }
}
