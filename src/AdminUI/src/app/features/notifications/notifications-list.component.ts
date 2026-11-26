import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NotificationService } from '../../core/notifications/notification.service';
import { GetNotificationsResponse, NotificationItem } from '../../core/notifications/notification.models';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-notifications-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatSelectModule, MatInputModule, MatButtonModule, MatIconModule, MatTableModule, MatDatepickerModule, MatNativeDateModule, EmptyStateComponent, ErrorStateComponent, LoadingSkeletonComponent, StatusBadgeComponent],
  template: `
    <section class="page-container" aria-labelledby="notifications-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Vận hành</p>
          <h1 id="notifications-title" class="page-title">Thông báo</h1>
          <p class="page-subtitle">Tra cứu danh sách Thông báo theo trạng thái, kênh và thời gian.</p>
        </div>
      </header>

      <mat-card class="panel-card filter-card">
        <form [formGroup]="filterForm" (ngSubmit)="onApplyFilters()" class="filters" aria-label="Bộ lọc Thông báo">
          <mat-form-field appearance="outline">
            <mat-label>Trạng thái thông báo</mat-label>
            <mat-select formControlName="status">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="Pending">Pending (Chờ xử lý)</mat-option>
              <mat-option value="Processing">Processing (Đang xử lý)</mat-option>
              <mat-option value="Sent">Sent (Đã gửi)</mat-option>
              <mat-option value="Failed">Failed (Thất bại)</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Kênh thông báo</mat-label>
            <mat-select formControlName="channel">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="email">Email</mat-option>
              <mat-option value="inapp">Thông báo trong ứng dụng</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Từ ngày</mat-label>
            <input matInput [matDatepicker]="fromPicker" formControlName="createdFrom" readonly (click)="fromPicker.open()" (focus)="fromPicker.open()" />
            <mat-datepicker-toggle matIconSuffix [for]="fromPicker"></mat-datepicker-toggle>
            <mat-datepicker #fromPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Đến ngày</mat-label>
            <input matInput [matDatepicker]="toPicker" formControlName="createdTo" readonly (click)="toPicker.open()" (focus)="toPicker.open()" />
            <mat-datepicker-toggle matIconSuffix [for]="toPicker"></mat-datepicker-toggle>
            <mat-datepicker #toPicker></mat-datepicker>
            <mat-error *ngIf="filterForm.controls.createdTo.hasError('dateRange')">Đến ngày phải lớn hơn hoặc bằng Từ ngày.</mat-error>
          </mat-form-field>

          <div class="filter-actions">
            <button mat-flat-button color="primary" type="submit">Áp dụng</button>
            <button mat-stroked-button type="button" (click)="onClearFilters()">Xóa bộ lọc</button>
          </div>
        </form>
      </mat-card>

      <mat-card class="panel-card">
        <mat-card-content>
          <app-error-state *ngIf="state === 'error'" [message]="errorMessage" (retry)="retry()"></app-error-state>
          <app-loading-skeleton *ngIf="state === 'loading'" [rows]="6" variant="line"></app-loading-skeleton>
          <app-empty-state
            *ngIf="state === 'empty'"
            title="Không có kết quả phù hợp"
            description="Chưa có Thông báo theo bộ lọc hiện tại. Vui lòng điều chỉnh hoặc xóa bộ lọc."
            actionLabel="Xóa bộ lọc"
            (action)="onClearFilters()"
          ></app-empty-state>

          <div *ngIf="state === 'success'" class="table-wrap">
            <table mat-table [dataSource]="items" class="data-table">
              <ng-container matColumnDef="id">
                <th mat-header-cell *matHeaderCellDef>Mã Thông báo</th>
                <td mat-cell *matCellDef="let item" class="mono"><a [routerLink]="['/notifications', item.id]" class="row-link">{{ item.id }}</a></td>
              </ng-container>

              <ng-container matColumnDef="recipient">
                <th mat-header-cell *matHeaderCellDef>Người nhận</th>
                <td mat-cell *matCellDef="let item">{{ item.recipient }}</td>
              </ng-container>

              <ng-container matColumnDef="channel">
                <th mat-header-cell *matHeaderCellDef>Kênh thông báo</th>
                <td mat-cell *matCellDef="let item">{{ displayChannel(item.channel) }}</td>
              </ng-container>

              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Trạng thái thông báo</th>
                <td mat-cell *matCellDef="let item"><app-status-badge [label]="displayStatus(item.status)" [tone]="statusTone(item.status)"></app-status-badge></td>
              </ng-container>

              <ng-container matColumnDef="createdAt">
                <th mat-header-cell *matHeaderCellDef>Thời gian tạo</th>
                <td mat-cell *matCellDef="let item">{{ formatDate(item.createdAt) }}</td>
              </ng-container>

              <ng-container matColumnDef="content">
                <th mat-header-cell *matHeaderCellDef>Nội dung</th>
                <td mat-cell *matCellDef="let item" class="content-cell" [title]="item.content || ''">{{ item.content || '—' }}</td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>

            <div class="pagination">
              <div class="pagination-info">Tổng: {{ totalCount }} · Trang {{ page }} / {{ totalPages || 1 }}</div>
              <div class="pagination-actions">
                <button mat-stroked-button type="button" (click)="prevPage()" [disabled]="page <= 1">Trước</button>
                <button mat-stroked-button type="button" (click)="nextPage()" [disabled]="page >= totalPages">Sau</button>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </section>
  `,
  styles: [`
    .eyebrow { margin: 0 0 8px; font-size: 12px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; color: var(--app-primary); }
    .panel-card { border-radius: 16px !important; border: 1px solid var(--app-border); box-shadow: var(--shadow-card) !important; }
    .filter-card { margin-bottom: 20px; }
    .filters { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 12px; align-items: start; }
    .filter-actions { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; padding-top: 8px; }
    .table-wrap { overflow-x: auto; }
    .data-table { width: 100%; }
    .mono { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 12px; }
    .row-link { color: var(--app-primary); text-decoration: none; }
    .row-link:hover { text-decoration: underline; }
    .content-cell { max-width: 280px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .pagination { display: flex; justify-content: space-between; align-items: center; gap: 12px; padding-top: 16px; flex-wrap: wrap; }
    .pagination-info { color: var(--app-muted); font-size: 14px; }
    .pagination-actions { display: flex; gap: 8px; }
  `]
})
export class NotificationsListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly notificationService = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);

  readonly pageSize = 20;
  readonly displayedColumns = ['id', 'recipient', 'channel', 'status', 'createdAt', 'content'];

  state: ViewState = 'loading';
  errorMessage = '';
  items: NotificationItem[] = [];
  totalCount = 0;
  page = 1;

  readonly filterForm = this.fb.group({
    status: this.fb.nonNullable.control(''),
    channel: this.fb.nonNullable.control(''),
    createdFrom: this.fb.control<Date | null>(null),
    createdTo: this.fb.control<Date | null>(null)
  });

  private activeQuery = {
    status: '',
    channel: '',
    createdFrom: '',
    createdTo: ''
  };

  ngOnInit(): void {
    const status = this.route.snapshot.queryParamMap.get('status') ?? '';
    const channel = this.route.snapshot.queryParamMap.get('channel') ?? '';
    if (status || channel) {
      this.filterForm.patchValue({ status, channel });
      this.activeQuery = {
        ...this.activeQuery,
        status,
        channel
      };
    }
    this.loadNotifications();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  onApplyFilters(): void {
    const value = this.filterForm.getRawValue();
    const createdFrom = value.createdFrom;
    const createdTo = value.createdTo;
    if (createdFrom && createdTo && this.toStartOfDay(createdTo) < this.toStartOfDay(createdFrom)) {
      this.filterForm.controls.createdTo.setErrors({ dateRange: true });
      this.filterForm.controls.createdTo.markAsTouched();
      return;
    }

    this.filterForm.controls.createdTo.setErrors(null);
    this.activeQuery = {
      status: value.status ?? '',
      channel: value.channel ?? '',
      createdFrom: this.toIsoString(createdFrom),
      createdTo: this.toIsoString(createdTo, true)
    };
    this.page = 1;
    this.loadNotifications();
  }

  onClearFilters(): void {
    this.filterForm.reset({ status: '', channel: '', createdFrom: null, createdTo: null });
    this.filterForm.controls.createdTo.setErrors(null);
    this.activeQuery = { status: '', channel: '', createdFrom: '', createdTo: '' };
    this.page = 1;
    this.loadNotifications();
  }

  retry(): void {
    this.loadNotifications();
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadNotifications();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page += 1;
      this.loadNotifications();
    }
  }

  trackById(_: number, item: NotificationItem): string {
    return item.id;
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

  statusTone(status: string): 'pending' | 'processing' | 'sent' | 'failed' | 'default' {
    switch (status) {
      case 'Pending':
        return 'pending';
      case 'Processing':
        return 'processing';
      case 'Sent':
        return 'sent';
      case 'Failed':
        return 'failed';
      default:
        return 'default';
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

  private loadNotifications(): void {
    this.state = 'loading';
    this.errorMessage = '';

    this.notificationService
      .getNotifications({
        status: this.activeQuery.status || null,
        channel: this.activeQuery.channel || null,
        createdFrom: this.activeQuery.createdFrom || null,
        createdTo: this.activeQuery.createdTo || null,
        page: this.page,
        pageSize: this.pageSize
      })
      .pipe(finalize(() => {
        // state set in handlers
      }))
      .subscribe({
        next: (response: GetNotificationsResponse) => {
          this.items = response.items ?? [];
          this.totalCount = response.totalCount ?? 0;
          this.page = response.page ?? this.page;
          this.state = this.items.length === 0 ? 'empty' : 'success';
        },
        error: (error: unknown) => {
          this.state = 'error';
          this.errorMessage = this.resolveErrorMessage(error);
        }
      });
  }

  private toIsoString(value: Date | string | null | undefined, endOfDay = false): string {
    if (!value) {
      return '';
    }
    const date = value instanceof Date ? new Date(value.getTime()) : new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }
    if (endOfDay) {
      date.setHours(23, 59, 59, 999);
    } else {
      date.setHours(0, 0, 0, 0);
    }
    return date.toISOString();
  }

  private toStartOfDay(value: Date): number {
    const date = new Date(value.getTime());
    date.setHours(0, 0, 0, 0);
    return date.getTime();
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401 || error.status === 403) {
        return 'Không có quyền truy cập danh sách Thông báo. Vui lòng kiểm tra phiên đăng nhập và quyền RBAC.';
      }
      if (error.status === 0) {
        return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      }

      const payload = error.error as { detail?: string; title?: string; errors?: Record<string, string[]> } | null;
      if (payload?.errors) {
        const messages = Object.values(payload.errors)
          .flat()
          .filter((m): m is string => typeof m === 'string');
        if (messages.length > 0) {
          return messages.join(' ');
        }
      }
      if (payload?.detail) {
        return payload.detail;
      }
      if (payload?.title) {
        return payload.title;
      }
      return 'Không tải được danh sách Thông báo. Vui lòng thử lại.';
    }
    return 'Không tải được danh sách Thông báo. Vui lòng thử lại.';
  }
}
