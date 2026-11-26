import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
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
  icon: string;
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
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule, EmptyStateComponent, ErrorStateComponent, LoadingSkeletonComponent],
  template: `
    <section class="page-container" aria-labelledby="dashboard-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Trung tâm vận hành</p>
          <h1 id="dashboard-title" class="page-title">Tổng quan</h1>
          <p class="page-subtitle">Theo dõi nhanh tình trạng Thông báo, Hàng đợi thư chết và Kiểm tra sức khỏe.</p>
        </div>
        <button mat-flat-button color="primary" type="button" (click)="refresh()">
          <mat-icon>refresh</mat-icon>
          <span>Làm mới</span>
        </button>
      </header>

      <div class="dashboard-grid">
        <mat-card class="panel-card" aria-labelledby="notification-summary-title">
          <mat-card-header class="panel-header">
            <div>
              <mat-card-title id="notification-summary-title">Trạng thái thông báo</mat-card-title>
              <mat-card-subtitle>Tóm tắt số lượng Thông báo theo trạng thái.</mat-card-subtitle>
            </div>
            <a mat-stroked-button routerLink="/notifications">Xem Thông báo</a>
          </mat-card-header>
          <mat-card-content>
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
                <div class="card-top">
                  <span class="card-label">{{ card.label }}</span>
                  <mat-icon>{{ card.icon }}</mat-icon>
                </div>
                <strong class="card-count">{{ card.count }}</strong>
              </a>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="panel-card" aria-labelledby="dlq-summary-title">
          <mat-card-header class="panel-header">
            <div>
              <mat-card-title id="dlq-summary-title">Hàng đợi thư chết</mat-card-title>
              <mat-card-subtitle>Số Thông điệp cần chú ý trong Hàng đợi thư chết.</mat-card-subtitle>
            </div>
            <a mat-stroked-button routerLink="/dead-letter-queues">Xem Hàng đợi thư chết</a>
          </mat-card-header>
          <mat-card-content>
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
                <div class="card-top">
                  <span class="card-label">{{ item.label }}</span>
                  <mat-icon>{{ item.count > 0 ? 'warning_amber' : 'check_circle' }}</mat-icon>
                </div>
                <strong class="card-count">{{ item.count }}</strong>
              </a>
              <div class="summary-card tone-info">
                <div class="card-top">
                  <span class="card-label">Tổng cần chú ý</span>
                  <mat-icon>inventory_2</mat-icon>
                </div>
                <strong class="card-count">{{ dlqTotal }}</strong>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="panel-card" aria-labelledby="health-summary-title">
          <mat-card-header class="panel-header">
            <div>
              <mat-card-title id="health-summary-title">Kiểm tra sức khỏe</mat-card-title>
              <mat-card-subtitle>Tình trạng sẵn sàng của API và phụ thuộc.</mat-card-subtitle>
            </div>
          </mat-card-header>
          <mat-card-content>
            <app-loading-skeleton *ngIf="healthState === 'loading'" [rows]="2" variant="line"></app-loading-skeleton>
            <app-error-state *ngIf="healthState === 'error'" [message]="healthError" (retry)="refresh()"></app-error-state>
            <app-empty-state *ngIf="healthState === 'empty'" title="Chưa có dữ liệu" description="Không nhận được dữ liệu Kiểm tra sức khỏe."></app-empty-state>

            <div *ngIf="healthState === 'success'" class="health-block">
              <div class="health-status" [ngClass]="healthToneClass">
                <mat-icon class="health-icon">{{ healthIcon }}</mat-icon>
                <span>{{ healthLabel }}</span>
              </div>
              <p class="health-detail">{{ healthDetail }}</p>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </section>
  `,
  styles: [`
    .eyebrow { margin: 0 0 8px; font-size: 12px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; color: var(--app-primary); }
    .dashboard-grid { display: grid; gap: 20px; }
    .panel-card { border-radius: 16px !important; border: 1px solid var(--app-border); box-shadow: var(--shadow-card) !important; }
    .panel-header { display: flex; justify-content: space-between; gap: 16px; align-items: flex-start; padding: 20px 20px 0; margin-bottom: 16px; }
    :host ::ng-deep .mat-mdc-card-header-text { margin: 0; }
    .summary-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 14px; }
    .summary-card { display: flex; flex-direction: column; gap: 12px; padding: 18px; border-radius: 14px; border: 1px solid var(--app-border); background: #f8fafc; text-decoration: none; color: inherit; transition: transform .15s ease, box-shadow .15s ease; }
    a.summary-card:hover { transform: translateY(-1px); box-shadow: 0 8px 20px rgba(15,23,42,0.06); }
    .card-top { display: flex; justify-content: space-between; gap: 12px; align-items: flex-start; }
    .card-label { font-size: 13px; color: var(--app-muted); font-weight: 500; }
    .card-count { font-size: 32px; line-height: 1; color: var(--app-text); letter-spacing: -0.03em; }
    .tone-warning { border-color: #fde68a; background: #fffbeb; }
    .tone-info { border-color: #bfdbfe; background: #eff6ff; }
    .tone-success { border-color: #bbf7d0; background: #f0fdf4; }
    .tone-danger { border-color: #fecaca; background: #fef2f2; }
    .health-block { display: grid; gap: 12px; }
    .health-status { display: inline-flex; align-items: center; gap: 10px; font-size: 15px; font-weight: 600; width: fit-content; padding: 10px 14px; border-radius: 999px; }
    .health-icon { font-size: 20px; width: 20px; height: 20px; }
    .health-ok { color: #166534; background: #dcfce7; }
    .health-warn { color: #92400e; background: #fef3c7; }
    .health-bad { color: #991b1b; background: #fee2e2; }
    .health-detail { margin: 0; color: var(--app-muted); font-size: 14px; line-height: 1.6; }
    @media (max-width: 768px) { .panel-header { flex-direction: column; align-items: stretch; padding: 16px 16px 0; } }
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
  healthIcon = 'warning';

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
      { status: 'Pending', label: 'Pending (Chờ xử lý)', count: response.notifications.pending, tone: 'warning', icon: 'schedule' },
      { status: 'Processing', label: 'Processing (Đang xử lý)', count: response.notifications.processing, tone: 'info', icon: 'sync' },
      { status: 'Sent', label: 'Sent (Đã gửi)', count: response.notifications.sent, tone: 'success', icon: 'check_circle' },
      { status: 'Failed', label: 'Failed (Thất bại)', count: response.notifications.failed, tone: 'danger', icon: 'error' }
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
      this.healthIcon = 'check_circle';
      this.healthDetail = response.health.details ?? 'API và các phụ thuộc readiness đang hoạt động bình thường.';
    } else if (status.includes('degraded')) {
      this.healthLabel = 'Degraded (Suy giảm)';
      this.healthToneClass = 'health-warn';
      this.healthIcon = 'warning';
      this.healthDetail = response.health.details ?? 'Hệ thống đang chạy nhưng có phụ thuộc suy giảm. Cần kiểm tra thêm.';
    } else {
      this.healthLabel = 'Unhealthy (Không sẵn sàng)';
      this.healthToneClass = 'health-bad';
      this.healthIcon = 'cancel';
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
