import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NotificationService } from '../../core/notifications/notification.service';
import { NotificationDeliveryHistoryResponse, NotificationItem } from '../../core/notifications/notification.models';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';

type ViewState = 'loading' | 'success' | 'error';

@Component({
  selector: 'app-notification-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule, MatTableModule, ErrorStateComponent, LoadingSkeletonComponent, StatusBadgeComponent],
  template: `
    <section class="page-container" aria-labelledby="notification-detail-title">
      <header class="page-header">
        <div>
          <a routerLink="/notifications" class="back-link"><mat-icon>arrow_back</mat-icon>Quay lại danh sách Thông báo</a>
          <p class="eyebrow">Vận hành</p>
          <h1 id="notification-detail-title" class="page-title">Chi tiết Thông báo</h1>
          <p class="page-subtitle">Thông tin, Trạng thái thông báo và Mã tương quan.</p>
        </div>
      </header>

      <app-loading-skeleton *ngIf="state === 'loading'" [rows]="6" variant="line"></app-loading-skeleton>
      <app-error-state *ngIf="state === 'error'" [message]="errorMessage" (retry)="retry()"></app-error-state>

      <mat-card *ngIf="state === 'success' && notification as item" class="panel-card detail-card">
        <mat-card-header class="card-header">
          <div>
            <mat-card-title>Thông tin Thông báo</mat-card-title>
            <mat-card-subtitle>Mã Thông báo: <span class="mono">{{ item.id }}</span></mat-card-subtitle>
          </div>
          <app-status-badge [label]="displayStatus(item.status)" [tone]="statusTone(item.status)"></app-status-badge>
        </mat-card-header>
        <mat-card-content>
          <dl class="details-grid">
            <div><dt>Mã Thông báo (Mã tương quan)</dt><dd class="mono">{{ item.id }}</dd></div>
            <div><dt>Người nhận</dt><dd>{{ item.recipient || '—' }}</dd></div>
            <div><dt>Kênh thông báo</dt><dd>{{ displayChannel(item.channel) }}</dd></div>
            <div><dt>Trạng thái thông báo</dt><dd>{{ displayStatus(item.status) }}</dd></div>
            <div><dt>Thời gian tạo</dt><dd>{{ formatDate(item.createdAt) }}</dd></div>
            <div><dt>Mã mẫu thông báo (Mã tương quan)</dt><dd class="mono">{{ item.notificationTemplateId || '—' }}</dd></div>
            <div class="full"><dt>Nội dung</dt><dd class="content">{{ item.content || '—' }}</dd></div>
          </dl>
        </mat-card-content>
      </mat-card>

      <mat-card class="panel-card detail-card">
        <mat-card-header class="card-header">
          <div>
            <mat-card-title>Nhật ký gửi thông báo &amp; Lần thử gửi</mat-card-title>
            <mat-card-subtitle>Theo dõi kết quả gửi, lỗi, số lần thử lại và Mã tương quan.</mat-card-subtitle>
          </div>
          <button *ngIf="historyState === 'error'" mat-stroked-button type="button" (click)="retryHistory()">Thử lại</button>
        </mat-card-header>
        <mat-card-content>
          <app-loading-skeleton *ngIf="historyState === 'loading'" [rows]="4" variant="line"></app-loading-skeleton>
          <app-error-state *ngIf="historyState === 'error'" [message]="historyErrorMessage" (retry)="retryHistory()"></app-error-state>
          <div *ngIf="historyState === 'success' && isHistoryEmpty" class="muted-block">Chưa có Nhật ký gửi thông báo hoặc Lần thử gửi cho Thông báo này.</div>

          <ng-container *ngIf="historyState === 'success' && deliveryHistory && !isHistoryEmpty">
            <h3 class="section-title">Nhật ký gửi thông báo</h3>
            <div class="table-wrap" *ngIf="deliveryHistory.logs.length > 0; else emptyLogs">
              <table mat-table [dataSource]="deliveryHistory.logs" class="data-table">
                <ng-container matColumnDef="channel"><th mat-header-cell *matHeaderCellDef>Kênh</th><td mat-cell *matCellDef="let log">{{ displayChannel(log.channel) }}</td></ng-container>
                <ng-container matColumnDef="result"><th mat-header-cell *matHeaderCellDef>Kết quả</th><td mat-cell *matCellDef="let log"><app-status-badge [label]="displayResult(log.result)" [tone]="resultTone(log.result)"></app-status-badge></td></ng-container>
                <ng-container matColumnDef="retryCount"><th mat-header-cell *matHeaderCellDef>Số lần thử lại</th><td mat-cell *matCellDef="let log">{{ log.retryCount }}</td></ng-container>
                <ng-container matColumnDef="correlationId"><th mat-header-cell *matHeaderCellDef>Mã tương quan</th><td mat-cell *matCellDef="let log" class="mono">{{ log.correlationId || '—' }}</td></ng-container>
                <ng-container matColumnDef="createdAtUtc"><th mat-header-cell *matHeaderCellDef>Thời gian</th><td mat-cell *matCellDef="let log">{{ formatDate(log.createdAtUtc) }}</td></ng-container>
                <ng-container matColumnDef="errorMessage"><th mat-header-cell *matHeaderCellDef>Lỗi</th><td mat-cell *matCellDef="let log">{{ log.errorMessage || '—' }}</td></ng-container>
                <tr mat-header-row *matHeaderRowDef="logColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: logColumns;"></tr>
              </table>
            </div>
            <ng-template #emptyLogs><p class="muted-block">Chưa có Nhật ký gửi thông báo.</p></ng-template>

            <h3 class="section-title">Lần thử gửi</h3>
            <div class="table-wrap" *ngIf="deliveryHistory.attempts.items.length > 0; else emptyAttempts">
              <table mat-table [dataSource]="deliveryHistory.attempts.items" class="data-table">
                <ng-container matColumnDef="attemptNumber"><th mat-header-cell *matHeaderCellDef>Lần thử</th><td mat-cell *matCellDef="let a">#{{ a.attemptNumber }}</td></ng-container>
                <ng-container matColumnDef="channel"><th mat-header-cell *matHeaderCellDef>Kênh</th><td mat-cell *matCellDef="let a">{{ displayChannel(a.channel) }}</td></ng-container>
                <ng-container matColumnDef="result"><th mat-header-cell *matHeaderCellDef>Kết quả</th><td mat-cell *matCellDef="let a"><app-status-badge [label]="displayResult(a.result)" [tone]="resultTone(a.result)"></app-status-badge></td></ng-container>
                <ng-container matColumnDef="correlationId"><th mat-header-cell *matHeaderCellDef>Mã tương quan</th><td mat-cell *matCellDef="let a" class="mono">{{ a.correlationId || '—' }}</td></ng-container>
                <ng-container matColumnDef="attemptedAtUtc"><th mat-header-cell *matHeaderCellDef>Thời gian</th><td mat-cell *matCellDef="let a">{{ formatDate(a.attemptedAtUtc) }}</td></ng-container>
                <ng-container matColumnDef="errorMessage"><th mat-header-cell *matHeaderCellDef>Lỗi</th><td mat-cell *matCellDef="let a">{{ a.errorMessage || '—' }}</td></ng-container>
                <tr mat-header-row *matHeaderRowDef="attemptColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: attemptColumns;"></tr>
              </table>
            </div>
            <ng-template #emptyAttempts><p class="muted-block">Chưa có Lần thử gửi.</p></ng-template>
          </ng-container>
        </mat-card-content>
      </mat-card>
    </section>
  `,
  styles: [`
    .eyebrow { margin: 0 0 8px; font-size: 12px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; color: var(--app-primary); }
    .back-link { display: inline-flex; align-items: center; gap: 6px; margin-bottom: 12px; color: var(--app-primary); text-decoration: none; font-size: 14px; font-weight: 500; }
    .back-link mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .panel-card { border-radius: 16px !important; border: 1px solid var(--app-border); box-shadow: var(--shadow-card) !important; }
    .detail-card { margin-bottom: 20px; }
    .card-header { display:flex; justify-content:space-between; gap:16px; align-items:flex-start; padding:20px 20px 0; margin-bottom:16px; }
    :host ::ng-deep .mat-mdc-card-header-text { margin: 0; }
    .details-grid { display:grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 16px; margin:0; }
    dt { font-size: 12px; font-weight: 600; color: var(--app-muted); text-transform: uppercase; letter-spacing: .04em; margin-bottom: 4px; }
    dd { margin: 0; font-size: 14px; color: var(--app-text); word-break: break-word; }
    .full { grid-column: 1 / -1; }
    .content { white-space: pre-wrap; background: #f8fafc; border: 1px solid var(--app-border); border-radius: 10px; padding: 14px; }
    .mono { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 12px; }
    .section-title { margin: 20px 0 12px; font-size: 15px; font-weight: 600; }
    .table-wrap { overflow-x: auto; }
    .data-table { width: 100%; }
    .muted-block { color: var(--app-muted); font-size: 14px; padding: 8px 0; }
  `]
})
export class NotificationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly notificationService = inject(NotificationService);

  readonly logColumns = ['channel', 'result', 'retryCount', 'correlationId', 'createdAtUtc', 'errorMessage'];
  readonly attemptColumns = ['attemptNumber', 'channel', 'result', 'correlationId', 'attemptedAtUtc', 'errorMessage'];

  state: ViewState = 'loading';
  errorMessage = '';
  notification: NotificationItem | null = null;
  private notificationId = '';

  historyState: ViewState = 'loading';
  historyErrorMessage = '';
  deliveryHistory: NotificationDeliveryHistoryResponse | null = null;
  historyPage = 1;
  readonly historyPageSize = 20;

  ngOnInit(): void {
    this.notificationId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadNotification();
  }

  retry(): void { this.loadNotification(); }
  retryHistory(): void { this.loadDeliveryHistory(); }

  displayChannel(channel: string): string {
    const normalized = (channel || '').toLowerCase();
    if (normalized === 'email') return 'Email';
    if (normalized === 'inapp') return 'Thông báo trong ứng dụng';
    return channel || '—';
  }

  displayStatus(status: string): string {
    switch (status) {
      case 'Pending': return 'Pending (Chờ xử lý)';
      case 'Processing': return 'Processing (Đang xử lý)';
      case 'Sent': return 'Sent (Đã gửi)';
      case 'Failed': return 'Failed (Thất bại)';
      default: return status || '—';
    }
  }

  statusTone(status: string): 'pending' | 'processing' | 'sent' | 'failed' | 'default' {
    switch (status) {
      case 'Pending': return 'pending';
      case 'Processing': return 'processing';
      case 'Sent': return 'sent';
      case 'Failed': return 'failed';
      default: return 'default';
    }
  }

  formatDate(value: string): string {
    if (!value) return '—';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    return date.toLocaleString('vi-VN', { hour12: false });
  }

  displayResult(result: string): string {
    switch (result) {
      case 'Success': return 'Thành công';
      case 'Failed': return 'Thất bại';
      default: return result || '—';
    }
  }

  resultTone(result: string): 'sent' | 'failed' | 'default' {
    switch (result) {
      case 'Success': return 'sent';
      case 'Failed': return 'failed';
      default: return 'default';
    }
  }

  get isHistoryEmpty(): boolean {
    return !!this.deliveryHistory
      && this.deliveryHistory.logs.length === 0
      && this.deliveryHistory.attempts.items.length === 0;
  }

  private loadNotification(): void {
    if (!this.notificationId) {
      this.state = 'error';
      this.errorMessage = 'Mã Thông báo không hợp lệ.';
      this.historyState = 'error';
      this.historyErrorMessage = 'Mã Thông báo không hợp lệ.';
      return;
    }

    this.state = 'loading';
    this.errorMessage = '';
    this.notification = null;
    this.loadDeliveryHistory();

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

  private loadDeliveryHistory(): void {
    if (!this.notificationId) {
      return;
    }

    this.historyState = 'loading';
    this.historyErrorMessage = '';
    this.deliveryHistory = null;

    this.notificationService
      .getNotificationDeliveryHistory(this.notificationId, this.historyPage, this.historyPageSize)
      .subscribe({
        next: (history) => {
          this.deliveryHistory = history;
          this.historyState = 'success';
        },
        error: (error: unknown) => {
          this.historyState = 'error';
          this.historyErrorMessage = this.resolveHistoryErrorMessage(error);
        }
      });
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 404) return 'Không tìm thấy Thông báo hoặc không có quyền truy cập.';
      if (error.status === 401 || error.status === 403) return 'Không có quyền truy cập chi tiết Thông báo. Vui lòng kiểm tra phiên đăng nhập và quyền RBAC.';
      if (error.status === 0) return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      const payload = error.error as { detail?: string; title?: string } | null;
      if (payload?.detail) return payload.detail;
      if (payload?.title) return payload.title;
      return 'Không tải được chi tiết Thông báo. Vui lòng thử lại.';
    }
    return 'Không tải được chi tiết Thông báo. Vui lòng thử lại.';
  }

  private resolveHistoryErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401 || error.status === 403) return 'Không có quyền truy cập lịch sử gửi Thông báo.';
      if (error.status === 0) return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      const payload = error.error as { detail?: string; title?: string } | null;
      if (payload?.detail) return payload.detail;
      if (payload?.title) return payload.title;
      return 'Không tải được lịch sử gửi Thông báo. Vui lòng thử lại.';
    }
    return 'Không tải được lịch sử gửi Thông báo. Vui lòng thử lại.';
  }
}
