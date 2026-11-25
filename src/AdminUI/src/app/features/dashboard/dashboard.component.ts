import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';
import { DeadLetterQueueService } from '../../core/dead-letter-queue/dead-letter-queue.service';
import { NotificationService } from '../../core/notifications/notification.service';

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
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
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

          <div *ngIf="notificationsState === 'loading'" class="skeleton-grid" aria-busy="true" aria-live="polite">
            <div class="skeleton card-skeleton" *ngFor="let item of skeletonCards"></div>
          </div>

          <div *ngIf="notificationsState === 'error'" class="status-box status-error" role="alert">
            <p>{{ notificationsError }}</p>
            <button type="button" class="secondary" (click)="loadNotificationSummary()">Thử lại</button>
          </div>

          <div *ngIf="notificationsState === 'empty'" class="status-box status-empty">
            <h3>Chưa có dữ liệu</h3>
            <p>Chưa có Thông báo trong hệ thống.</p>
          </div>

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
              <p>Số Thông điệp cần chú ý trong DLQ.</p>
            </div>
            <a routerLink="/dead-letter-queues" class="link">Xem DLQ</a>
          </div>

          <div *ngIf="dlqState === 'loading'" class="skeleton-grid" aria-busy="true" aria-live="polite">
            <div class="skeleton card-skeleton" *ngFor="let item of [1, 2]"></div>
          </div>

          <div *ngIf="dlqState === 'error'" class="status-box status-error" role="alert">
            <p>{{ dlqError }}</p>
            <button type="button" class="secondary" (click)="loadDlqSummary()">Thử lại</button>
          </div>

          <div *ngIf="dlqState === 'empty'" class="status-box status-empty">
            <h3>Chưa có dữ liệu</h3>
            <p>Không có dữ liệu Hàng đợi thư chết.</p>
          </div>

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

          <div *ngIf="healthState === 'loading'" class="skeleton-list" aria-busy="true" aria-live="polite">
            <div class="skeleton line-skeleton"></div>
            <div class="skeleton line-skeleton short"></div>
          </div>

          <div *ngIf="healthState === 'error'" class="status-box status-error" role="alert">
            <p>{{ healthError }}</p>
            <button type="button" class="secondary" (click)="loadHealth()">Thử lại</button>
          </div>

          <div *ngIf="healthState === 'empty'" class="status-box status-empty">
            <h3>Chưa có dữ liệu</h3>
            <p>Không nhận được dữ liệu Kiểm tra sức khỏe.</p>
          </div>

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
    .status-box { border-radius: 8px; padding: 16px; }
    .status-box h3 { margin: 0 0 4px; font-size: 16px; }
    .status-box p { margin: 0 0 12px; font-size: 14px; }
    .status-box p:last-child { margin-bottom: 0; }
    .status-error { background: #fee2e2; color: #991b1b; }
    .status-empty { background: #f1f5f9; color: #475569; }
    .primary, .secondary { border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; border: 1px solid transparent; }
    .primary { background: #2563eb; color: #ffffff; }
    .primary:hover { background: #1d4ed8; }
    .secondary { background: #ffffff; color: #0f172a; border-color: #cbd5e1; }
    .secondary:hover { background: #f8fafc; }
    .primary:focus, .secondary:focus, .link:focus, .summary-card:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
    .skeleton-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; }
    .skeleton { background: linear-gradient(90deg, #f1f5f9 25%, #e2e8f0 37%, #f1f5f9 63%); background-size: 400% 100%; animation: shimmer 1.2s infinite; border-radius: 6px; }
    .card-skeleton { height: 84px; }
    .skeleton-list { display: grid; gap: 10px; }
    .line-skeleton { height: 20px; width: 220px; }
    .line-skeleton.short { width: 320px; max-width: 100%; }
    .health-block { display: grid; gap: 8px; }
    .health-status { display: inline-flex; align-items: center; gap: 8px; font-size: 15px; font-weight: 600; width: fit-content; padding: 8px 12px; border-radius: 999px; }
    .health-dot { width: 10px; height: 10px; border-radius: 50%; background: currentColor; }
    .health-ok { color: #166534; background: #dcfce7; }
    .health-warn { color: #92400e; background: #fef3c7; }
    .health-bad { color: #991b1b; background: #fee2e2; }
    .health-detail { margin: 0; color: #475569; font-size: 14px; }
    @keyframes shimmer { 0% { background-position: 100% 0; } 100% { background-position: 0 0; } }
    @media (max-width: 640px) {
      .page-header, .panel-header { flex-direction: column; align-items: stretch; }
    }
  `]
})
export class DashboardComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly deadLetterQueueService = inject(DeadLetterQueueService);
  private readonly http = inject(HttpClient);

  readonly skeletonCards = [1, 2, 3, 4];

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
    this.loadNotificationSummary();
    this.loadDlqSummary();
    this.loadHealth();
  }

  loadNotificationSummary(): void {
    this.notificationsState = 'loading';
    this.notificationsError = 'Không thể tải tóm tắt Thông báo.';

    const statuses: Array<Pick<NotificationSummaryCard, 'status' | 'label' | 'tone'>> = [
      { status: 'Pending', label: 'Pending (Chờ xử lý)', tone: 'warning' },
      { status: 'Processing', label: 'Processing (Đang xử lý)', tone: 'info' },
      { status: 'Sent', label: 'Sent (Đã gửi)', tone: 'success' },
      { status: 'Failed', label: 'Failed (Thất bại)', tone: 'danger' }
    ];

    forkJoin(
      statuses.map((item) =>
        this.notificationService
          .getNotifications({ status: item.status, page: 1, pageSize: 1 })
          .pipe(
            map((response) => ({ ...item, count: response.totalCount })),
            catchError((error: unknown) => {
              this.notificationsError = this.toErrorMessage(error, 'Không thể tải tóm tắt Thông báo.');
              return of(null);
            })
          )
      )
    )
      .pipe(finalize(() => {
        if (this.notificationsState === 'loading') {
          this.notificationsState = this.notificationCards.length > 0 ? 'success' : 'error';
        }
      }))
      .subscribe((results) => {
        if (results.some((result) => result === null)) {
          this.notificationsState = 'error';
          return;
        }

        this.notificationCards = results as NotificationSummaryCard[];
        const hasData = this.notificationCards.some((card) => card.count > 0);
        this.notificationsState = hasData ? 'success' : 'empty';
      });
  }

  loadDlqSummary(): void {
    this.dlqState = 'loading';
    this.dlqError = 'Không thể tải tóm tắt Hàng đợi thư chết.';

    const queues: DlqSummaryItem[] = [
      { queueName: 'notifications.email.dlq', label: 'Email DLQ', count: 0 },
      { queueName: 'notifications.inapp.dlq', label: 'In-app DLQ', count: 0 }
    ];

    forkJoin(
      queues.map((queue) =>
        this.deadLetterQueueService.getMessages(queue.queueName, 1).pipe(
          map((response) => ({ ...queue, count: response.count })),
          catchError((error: unknown) => {
            this.dlqError = this.toErrorMessage(error, 'Không thể tải tóm tắt Hàng đợi thư chết.');
            return of(null);
          })
        )
      )
    )
      .pipe(finalize(() => {
        if (this.dlqState === 'loading') {
          this.dlqState = this.dlqItems.length > 0 ? 'success' : 'error';
        }
      }))
      .subscribe((results) => {
        if (results.some((result) => result === null)) {
          this.dlqState = 'error';
          return;
        }

        this.dlqItems = results as DlqSummaryItem[];
        this.dlqTotal = this.dlqItems.reduce((sum, item) => sum + item.count, 0);
        this.dlqState = 'success';
      });
  }

  loadHealth(): void {
    this.healthState = 'loading';
    this.healthError = 'Không thể tải trạng thái Kiểm tra sức khỏe.';

    this.http
      .get('/health/ready', { responseType: 'text' })
      .pipe(
        catchError((error: unknown) => {
          if (error instanceof HttpErrorResponse && typeof error.error === 'string' && error.error.trim()) {
            return of(error.error);
          }

          this.healthError = this.toErrorMessage(error, 'Không thể tải trạng thái Kiểm tra sức khỏe.');
          return of(null);
        }),
        finalize(() => {
          if (this.healthState === 'loading') {
            this.healthState = 'error';
          }
        })
      )
      .subscribe((response) => {
        if (response === null) {
          this.healthState = 'error';
          return;
        }

        const normalized = (response || '').trim();
        if (!normalized) {
          this.healthState = 'empty';
          return;
        }

        const status = normalized.toLowerCase();
        if (status.includes('healthy') && !status.includes('unhealthy')) {
          this.healthLabel = 'Healthy (Sẵn sàng)';
          this.healthToneClass = 'health-ok';
          this.healthDetail = 'API và các phụ thuộc readiness đang hoạt động bình thường.';
        } else if (status.includes('degraded')) {
          this.healthLabel = 'Degraded (Suy giảm)';
          this.healthToneClass = 'health-warn';
          this.healthDetail = 'Hệ thống đang chạy nhưng có phụ thuộc suy giảm. Cần kiểm tra thêm.';
        } else {
          this.healthLabel = 'Unhealthy (Không sẵn sàng)';
          this.healthToneClass = 'health-bad';
          this.healthDetail = 'Hệ thống hoặc phụ thuộc readiness chưa sẵn sàng.';
        }

        this.healthState = 'success';
      });
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