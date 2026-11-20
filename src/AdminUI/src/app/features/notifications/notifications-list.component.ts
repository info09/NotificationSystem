import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NotificationService } from '../../core/notifications/notification.service';
import { GetNotificationsResponse, NotificationItem } from '../../core/notifications/notification.models';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-notifications-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page" aria-labelledby="notifications-title">
      <header class="page-header">
        <div>
          <h1 id="notifications-title">Thông báo</h1>
          <p class="subtitle">Tra cứu danh sách Thông báo theo trạng thái, kênh và thời gian.</p>
        </div>
      </header>

      <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()" aria-label="Bộ lọc Thông báo">
        <div class="field">
          <label for="status">Trạng thái thông báo</label>
          <select id="status" formControlName="status">
            <option value="">Tất cả</option>
            <option value="Pending">Pending (Chờ xử lý)</option>
            <option value="Processing">Processing (Đang xử lý)</option>
            <option value="Sent">Sent (Đã gửi)</option>
            <option value="Failed">Failed (Thất bại)</option>
          </select>
        </div>

        <div class="field">
          <label for="channel">Kênh thông báo</label>
          <select id="channel" formControlName="channel">
            <option value="">Tất cả</option>
            <option value="email">Email</option>
            <option value="inapp">Thông báo trong ứng dụng</option>
          </select>
        </div>

        <div class="field">
          <label for="createdFrom">Từ thời gian</label>
          <input id="createdFrom" type="datetime-local" formControlName="createdFrom" />
        </div>

        <div class="field">
          <label for="createdTo">Đến thời gian</label>
          <input id="createdTo" type="datetime-local" formControlName="createdTo" />
        </div>

        <div class="filter-actions">
          <button type="submit" class="primary">Áp dụng bộ lọc</button>
          <button type="button" class="secondary" (click)="onClearFilters()">Xóa bộ lọc</button>
        </div>
      </form>

      <div *ngIf="state === 'error'" class="status-box status-error" role="alert">
        <p>{{ errorMessage }}</p>
        <button type="button" class="primary" (click)="retry()">Thử lại</button>
      </div>

      <div *ngIf="state === 'loading'" class="table-wrap" aria-busy="true" aria-live="polite">
        <div class="skeleton" *ngFor="let row of skeletonRows"></div>
      </div>

      <div *ngIf="state === 'empty'" class="status-box status-empty">
        <h2>Không có kết quả phù hợp</h2>
        <p>Chưa có Thông báo theo bộ lọc hiện tại. Vui lòng điều chỉnh hoặc xóa bộ lọc.</p>
        <button type="button" class="secondary" (click)="onClearFilters()">Xóa bộ lọc</button>
      </div>

      <div *ngIf="state === 'success'" class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>Mã Thông báo</th>
              <th>Người nhận</th>
              <th>Kênh thông báo</th>
              <th>Trạng thái thông báo</th>
              <th>Thời gian tạo</th>
              <th>Nội dung</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of items; trackBy: trackById">
              <td class="mono" title="{{ item.id }}">
                <a [routerLink]="['/notifications', item.id]" class="row-link">{{ item.id }}</a>
              </td>
              <td>{{ item.recipient }}</td>
              <td>{{ displayChannel(item.channel) }}</td>
              <td>
                <span class="badge" [ngClass]="statusClass(item.status)">
                  {{ displayStatus(item.status) }}
                </span>
              </td>
              <td>{{ formatDate(item.createdAt) }}</td>
              <td class="content-cell" [title]="item.content || ''">{{ item.content || '—' }}</td>
            </tr>
          </tbody>
        </table>

        <div class="pagination" aria-label="Phân trang">
          <div class="pagination-info">
            Tổng: {{ totalCount }} · Trang {{ page }} / {{ totalPages || 1 }}
          </div>
          <div class="pagination-actions">
            <button type="button" class="secondary" (click)="prevPage()" [disabled]="page <= 1">Trước</button>
            <button type="button" class="secondary" (click)="nextPage()" [disabled]="page >= totalPages">Sau</button>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .page { padding: 24px; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; color: #0f172a; }
    .page-header { margin-bottom: 16px; }
    h1 { margin: 0 0 4px; font-size: 24px; font-weight: 600; }
    .subtitle { margin: 0; color: #475569; font-size: 14px; }
    .filters { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 12px; align-items: end; background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 16px; margin-bottom: 16px; }
    .field { display: flex; flex-direction: column; gap: 6px; }
    label { font-size: 14px; font-weight: 600; color: #0f172a; }
    input, select { padding: 8px 10px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 14px; font-family: inherit; }
    input:focus, select:focus { outline: 2px solid #2563eb; outline-offset: 1px; border-color: #2563eb; }
    .filter-actions { display: flex; gap: 8px; flex-wrap: wrap; }
    button { border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; font-family: inherit; }
    button:disabled { opacity: 0.6; cursor: not-allowed; }
    .primary { background: #2563eb; color: #fff; border: 1px solid #2563eb; }
    .primary:hover:not(:disabled) { background: #1d4ed8; }
    .secondary { background: #fff; color: #0f172a; border: 1px solid #cbd5e1; }
    .secondary:hover:not(:disabled) { background: #f1f5f9; }
    .status-box { background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 20px; }
    .status-error { border-color: #fecaca; background: #fef2f2; color: #991b1b; }
    .status-error p { margin: 0 0 12px; color: #991b1b; }
    .status-empty h2 { margin: 0 0 8px; font-size: 16px; }
    .status-empty p { margin: 0 0 12px; color: #475569; }
    .table-wrap { background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; overflow: hidden; }
    .data-table { width: 100%; border-collapse: collapse; }
    th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e2e8f0; font-size: 14px; vertical-align: top; }
    th { background: #f8fafc; font-weight: 600; color: #0f172a; }
    tbody tr:last-child td { border-bottom: none; }
    .mono { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 12px; word-break: break-all; }
    .row-link { color: #2563eb; text-decoration: none; }
    .row-link:hover { text-decoration: underline; }
    .row-link:focus { outline: 2px solid #2563eb; outline-offset: 2px; border-radius: 4px; }
    .content-cell { max-width: 280px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .badge { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: 12px; font-weight: 600; border: 1px solid transparent; }
    .badge-pending { background: #fffbeb; color: #92400e; border-color: #fde68a; }
    .badge-processing { background: #e0f2fe; color: #075985; border-color: #bae6fd; }
    .badge-sent { background: #dcfce7; color: #166534; border-color: #bbf7d0; }
    .badge-failed { background: #fee2e2; color: #991b1b; border-color: #fecaca; }
    .badge-default { background: #f1f5f9; color: #334155; border-color: #cbd5e1; }
    .pagination { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 12px; border-top: 1px solid #e2e8f0; flex-wrap: wrap; }
    .pagination-info { color: #475569; font-size: 14px; }
    .pagination-actions { display: flex; gap: 8px; }
    .skeleton { height: 44px; margin: 12px; border-radius: 6px; background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%); background-size: 200% 100%; animation: shimmer 1.2s infinite; }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }
    @media (max-width: 1023px) {
      .table-wrap { overflow-x: auto; }
      .data-table { min-width: 760px; }
    }
  `]
})
export class NotificationsListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly notificationService = inject(NotificationService);

  readonly pageSize = 20;
  readonly skeletonRows = Array.from({ length: 6 });

  state: ViewState = 'loading';
  errorMessage = '';
  items: NotificationItem[] = [];
  totalCount = 0;
  page = 1;

  readonly filterForm = this.fb.nonNullable.group({
    status: [''],
    channel: [''],
    createdFrom: [''],
    createdTo: ['']
  });

  private activeQuery = {
    status: '',
    channel: '',
    createdFrom: '',
    createdTo: ''
  };

  ngOnInit(): void {
    this.loadNotifications();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  onApplyFilters(): void {
    const value = this.filterForm.getRawValue();
    this.activeQuery = {
      status: value.status ?? '',
      channel: value.channel ?? '',
      createdFrom: this.toIsoString(value.createdFrom),
      createdTo: this.toIsoString(value.createdTo)
    };
    this.page = 1;
    this.loadNotifications();
  }

  onClearFilters(): void {
    this.filterForm.reset({ status: '', channel: '', createdFrom: '', createdTo: '' });
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

  private toIsoString(value: string | null | undefined): string {
    if (!value) {
      return '';
    }
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }
    return date.toISOString();
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
