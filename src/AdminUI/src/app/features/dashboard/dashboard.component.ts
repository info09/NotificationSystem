import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';

type BlockState = 'loading' | 'success' | 'empty' | 'error';

interface NotificationSummaryCard {
  status: string;
  label: string;
  count: number;
  tone: 'warning' | 'info' | 'success' | 'danger';
}

interface DlqSummaryItem {
  queueName: string;
  label: string;
  count: number;
  error?: string | null;
}

interface DashboardSummaryResponse {
  notifications: {
    pending: number;
    processing: number;
    sent: number;
    failed: number;
    error?: string | null;
  };
  deadLetterQueues: Array<{
    queueName: string;
    count: number;
    error?: string | null;
  }>;
  health: {
    status: string;
    details?: string | null;
  };
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent, ErrorStateComponent, LoadingSkeletonComponent],
  template: `
    <section class="page" aria-labelledby="dashboard-title">
      <header class="page-header">
        <div>
          <h1 id="dashboard-title">Tổng quan</h1>
          <p class="subtitle">Theo dõi nhanh tình trạng Thông báo, Hàng đợi thư chết và Kiểm tra sức khỏe.</p>
        </div>
        <button type="button" class="primary" (click)="refresh()">Làm mới</button>
      </header>

      <div class="grid">
        <article class="panel" aria-labelledby="notification-summary-title">
          <div class="panel-header">
            <div>
              <h2 id="notification-summary-title">Trạng thái thông báo</h2>
              <p>Tóm tắt số lượng Thông báo theo trạng thái.</p>
            </div>
            <a routerLink="/notifications" class="link">Xem Thông báo</a>
          </div>

          <app-loading-skeleton *ngIf="notificationsState === 'loading'" [rows]="4" variant="card"></app-loading-skeleton>
          <app-error-state *ngIf="notificationsState === 'error'" [message]="notificationsError" (retry)="refresh()"></app-error-state>
          <app-empty-state *ngIf="notificationsState === 'empty'" title="Chưa có dữ liệu" description="Chưa có Thông báo trong hệ thống."></app-empty-state>

          <div *ngIf="notificationsState === 'success'" class="summary-cards">
            <a
              *ngFor="let card of notificationCards"
              class="summary-card"
              [ngClass]="'tone-' + card.tone"
              [routerLink]="['/notifications']"
              [queryParams]="{ status: card.status }"
            >
              <span class="card-label">{{ card.label }}</span>
              <strong class="card-count">{{ card.count }}</strong>
            </a>
          </div>
        </article>

        <article class="panel" aria-labelledby="dlq-summary-title">
          <div class="panel-header">
            <div>
              <h2 id="dlq-summary-title">Hàng đợi thư chết</h2>
              <p>Số Thông điệp cần chú ý trong Hàng đợi thư chết.</p>
            </div>
            <a routerLink="/dead-letter-queues" class="link">Xem Hàng đợi thư chết</a>
          </div>

          <app-loading-skeleton *ngIf="dlqState === 'loading'" [rows]="2" variant="card"></app-loading-skeleton>
          <app-error-state *ngIf="dlqState === 'error'" [message]="dlqError" (retry)="refresh()"></app-error-state>
          <app-empty-state *ngIf="dlqState === 'empty'" title="Chưa có dữ liệu" description="Không có dữ liệu Hàng đợi thư chết."></app-empty-state>

          <div *ngIf="dlqState === 'success'" class="summary-cards">
            <a
              *ngFor="let item of dlqItems"
              class="summary-card"
              [ngClass]="item.count > 0 ? 'tone-danger' : 'tone-success'"
              [routerLink]="['/dead-letter-queues']"
              [queryParams]="{ queueName: item.queueName }"
            >
              <span class="card-label">{{ item.label }}</span>
              <strong class="card-count">{{ item.count }}</strong>
            </a>
            <div class="summary-card tone-info">
              <span class="card-label">Tổng cần chú ý</span>
              <strong class="card-count">{{ dlqTotal }}</strong>
            </div>
          </div>
        </article>

        <article class="panel" aria-labelledby="health-summary-title">
          <div class="panel-header">
            <div>
              <h2 id="health-summary-title">Kiểm tra sức khỏe</h2>
              <p>Tình trạng sẵn sàng của API và phụ thuộc.</p>
            </div>
          </div>

          <app-loading-skeleton *ngIf="healthState === 'loading'" [rows]="2" variant="line"></app-loading-skeleton>
          <app-error-state *ngIf="healthState === 'error'" [message]="healthError" (retry)="refresh()"></app-error-state>
          <app-empty-state *ngIf="healthState === 'empty'" title="Chưa có dữ liệu" description="Không nhận được dữ liệu Kiểm tra sức khỏe."></app-empty-state>

          <div *ngIf="healthState === 'success'" class="health-block">
            <div class="health-status" [ngClass]="healthToneClass">
              <span class="health-dot" aria-hidden="true"></span>
              <span>{{ healthLabel }}</span>
            </div>
            <p class="health-detail">{{ healthDetail }}</p>
          </div>
        </article>
      </div>
    </section>
  `,
  styles: [`
    .page { padding: 24px; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; color: #0f172a; }
    .page-header { display: flex; justify-content: space-between; gap: 16px; align-items: flex-start; margin-bottom: 20px; }
    h1 { margin: 0 0 4px; font-size: 24px; }
    .subtitle { margin: 0; color: #475569; font-size: 14px; }
    .grid { display: grid; gap: 16px; }
    .panel { background: #ffffff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 16px; }
    .panel-header { display: flex; justify-content: space-between; gap: 12px; align-items: flex-start; margin-bottom: 16px; }
    .panel-header h2 { margin: 0 0 4px; font-size: 18px; }
    .panel-header p { margin: 0; color: #475569; font-size: 14px; }
    .link { color: #2563eb; text-decoration: none; font-size: 14px; white-space: nowrap; }
    .link:hover { text-decoration: underline; }
    .summary-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; }
    .summary-card { display: flex; flex-direction: column; gap: 8px; padding: 14px; border-radius: 8px; border: 1px solid #cbd5e1; background: #f8fafc; text-decoration: none; color: inherit; }
    a.summary-card:hover { border-color: #94a3b8; background: #f1f5f9; }
    .card-label { font-size: 13px; color: #475569; }
    .card-count { font-size: 28px; line-height: 1; color: #0f172a; }
    .tone-warning { border-color: #fcd34d; background: #fffbeb; }
    .tone-info { border-color: #93c5fd; background: #eff6ff; }
    .tone-success { border-color: #86efac; background: #f0fdf4; }
    .tone-danger { border-color: #fca5a5; background: #fef2f2; }
    .primary { border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; border: 1px solid #2563eb; background: #2563eb; color: #ffffff; font-family: inherit; }
    .primary:hover { background: #1d4ed8; }
    .primary:focus, .link:focus, .summary-card:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
    .health-block { display: grid; gap: 8px; }
    .health-status { display: inline-flex; align-items: center; gap: 8px; font-size: 15px; font-weight: 600; width: fit-content; padding: 8px 12px; border-radius: 999px; }
    .health-dot { width: 10px; height: 10px; border-radius: 50%; background: currentColor; }
    .health-ok { color: #166534; background: #dcfce7; }
    .health-warn { color: #92400e; background: #fef3c7; }
    .health-bad { color: #991b1b; background: #fee2e2; }
    .health-detail { margin: 0; color: #475569; font-size: 14px; }
    @media (max-width: 640px) {
      .page-header, .panel-header { flex-direction: column; align-items: stretch; }
    }
  `]
})
export class DashboardComponent implements OnInit {
  private readonly http = inject(HttpClient);

  notificationsState: BlockState = 'loading';
  notificationsError = 'Không thể tải tóm tắt Thông báo.';
  notificationCards: NotificationSummaryCard[] = [];

  dlqState: BlockState = 'loading';
  dlqError = 'Không thể tải tóm tắt Hàng đợi thư chết.';
  dlqItems: DlqSummaryItem[] = [];
  dlqTotal = 0;

  healthState: BlockState = 'loading';
  healthError = 'Không thể tải trạng thái Kiểm tra sức khỏe.';
  healthLabel = '';
  healthDetail = '';
  healthToneClass = 'health-warn';

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.notificationsState = 'loading';
    this.dlqState = 'loading';
    this.healthState = 'loading';
    this.notificationsError = 'Không thể tải tóm tắt Thông báo.';
    this.dlqError = 'Không thể tải tóm tắt Hàng đợi thư chết.';
    this.healthError = 'Không thể tải trạng thái Kiểm tra sức khỏe.';

    this.http
      .get<DashboardSummaryResponse>('/api/admin/dashboard/summary')
      .pipe(
        catchError((error: unknown) => {
          const message = this.toErrorMessage(error, 'Không thể tải dữ liệu Tổng quan.');
          this.notificationsState = 'error';
          this.dlqState = 'error';
          this.healthState = 'error';
          this.notificationsError = message;
          this.dlqError = message;
          this.healthError = message;
          return of(null);
        }),
        finalize(() => {
          if (this.notificationsState === 'loading') {
            this.notificationsState = 'error';
          }
          if (this.dlqState === 'loading') {
            this.dlqState = 'error';
          }
          if (this.healthState === 'loading') {
            this.healthState = 'error';
          }
        })
      )
      .subscribe((response) => {
        if (response === null) {
          return;
        }

        this.applyNotificationSummary(response);
        this.applyDlqSummary(response);
        this.applyHealthSummary(response);
      });
  }

  private applyNotificationSummary(response: DashboardSummaryResponse): void {
    if (response.notifications.error) {
      this.notificationsState = 'error';
      this.notificationsError = response.notifications.error;
      return;
    }

    this.notificationCards = [
      { status: 'Pending', label: 'Pending (Chờ xử lý)', count: response.notifications.pending, tone: 'warning' },
      { status: 'Processing', label: 'Processing (Đang xử lý)', count: response.notifications.processing, tone: 'info' },
      { status: 'Sent', label: 'Sent (Đã gửi)', count: response.notifications.sent, tone: 'success' },
      { status: 'Failed', label: 'Failed (Thất bại)', count: response.notifications.failed, tone: 'danger' }
    ];
    this.notificationsState = this.notificationCards.some((card) => card.count > 0) ? 'success' : 'empty';
  }

  private applyDlqSummary(response: DashboardSummaryResponse): void {
    if (response.deadLetterQueues.some((item) => item.error)) {
      this.dlqState = 'error';
      this.dlqError = response.deadLetterQueues.find((item) => item.error)?.error ?? 'Không thể tải tóm tắt Hàng đợi thư chết.';
      return;
    }

    this.dlqItems = response.deadLetterQueues.map((item) => ({
      queueName: item.queueName,
      label: this.toDlqLabel(item.queueName),
      count: item.count,
      error: item.error
    }));
    this.dlqTotal = this.dlqItems.reduce((sum, item) => sum + item.count, 0);
    this.dlqState = 'success';
  }

  private applyHealthSummary(response: DashboardSummaryResponse): void {
    const status = (response.health.status || '').trim().toLowerCase();
    if (!status) {
      this.healthState = 'empty';
      return;
    }

    if (status.includes('healthy') && !status.includes('unhealthy')) {
      this.healthLabel = 'Healthy (Sẵn sàng)';
      this.healthToneClass = 'health-ok';
      this.healthDetail = response.health.details ?? 'API và các phụ thuộc readiness đang hoạt động bình thường.';
    } else if (status.includes('degraded')) {
      this.healthLabel = 'Degraded (Suy giảm)';
      this.healthToneClass = 'health-warn';
      this.healthDetail = response.health.details ?? 'Hệ thống đang chạy nhưng có phụ thuộc suy giảm. Cần kiểm tra thêm.';
    } else {
      this.healthLabel = 'Unhealthy (Không sẵn sàng)';
      this.healthToneClass = 'health-bad';
      this.healthDetail = response.health.details ?? 'Hệ thống hoặc phụ thuộc readiness chưa sẵn sàng.';
    }

    this.healthState = 'success';
  }

  private toDlqLabel(queueName: string): string {
    if (queueName === 'notifications.email.dlq') {
      return 'Email - Hàng đợi thư chết';
    }

    if (queueName === 'notifications.inapp.dlq') {
      return 'In-app - Hàng đợi thư chết';
    }

    return queueName;
  }

  private toErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) {
        return 'Phiên đăng nhập đã hết hạn hoặc không hợp lệ.';
      }
      if (error.status === 403) {
        return 'Bạn không có quyền xem dữ liệu này.';
      }
      if (error.status === 0) {
        return 'Không thể kết nối máy chủ. Vui lòng thử lại.';
      }

      const detail = (error.error as { detail?: string; title?: string } | null)?.detail
        ?? (error.error as { detail?: string; title?: string } | null)?.title;
      if (detail) {
        return detail;
      }
    }

    return fallback;
  }
}
