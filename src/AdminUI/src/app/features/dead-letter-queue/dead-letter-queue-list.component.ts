import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { DeadLetterQueueService } from '../../core/dead-letter-queue/dead-letter-queue.service';
import { DeadLetterQueueMessage } from '../../core/dead-letter-queue/dead-letter-queue.models';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-dead-letter-queue-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page" aria-labelledby="dlq-title">
      <header class="page-header">
        <div>
          <h1 id="dlq-title">Hàng đợi thư chết</h1>
          <p class="subtitle">Tra cứu Thông điệp lỗi trong Hàng đợi thư chết và xem chi tiết tóm tắt.</p>
        </div>
      </header>

      <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()" aria-label="Bộ lọc Hàng đợi thư chết">
        <div class="field">
          <label for="queueName">Hàng đợi thư chết</label>
          <select id="queueName" formControlName="queueName">
            <option value="notifications.email.dlq">notifications.email.dlq</option>
            <option value="notifications.inapp.dlq">notifications.inapp.dlq</option>
          </select>
        </div>

        <div class="field">
          <label for="limit">Số lượng</label>
          <select id="limit" formControlName="limit">
            <option [ngValue]="20">20</option>
            <option [ngValue]="50">50</option>
            <option [ngValue]="100">100</option>
          </select>
        </div>

        <div class="field">
          <label for="reason">Lý do (lọc tạm)</label>
          <input id="reason" type="text" formControlName="reason" placeholder="Nhập lý do..." />
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
        <h2>Không có Thông điệp trong Hàng đợi thư chết</h2>
        <p>Hiện không có mục lỗi theo bộ lọc hiện tại. Đây là trạng thái tốt.</p>
        <button type="button" class="secondary" (click)="onClearFilters()">Xóa bộ lọc</button>
      </div>

      <div *ngIf="state === 'success'" class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>MessageId</th>
              <th>NotificationId</th>
              <th>Kênh</th>
              <th>Lý do</th>
              <th>Dead-letter lúc</th>
              <th>Số lần</th>
              <th>Chi tiết</th>
            </tr>
          </thead>
          <tbody>
            <ng-container *ngFor="let item of filteredItems; trackBy: trackByMessageId">
              <tr>
                <td class="mono" [title]="item.messageId">{{ item.messageId || '—' }}</td>
                <td class="mono" [title]="item.notificationId || ''">{{ item.notificationId || '—' }}</td>
                <td>{{ displayChannel(item.channel) }}</td>
                <td>{{ item.reason || '—' }}</td>
                <td>{{ formatDate(item.deadLetteredAtUtc) }}</td>
                <td>{{ item.deathCount ?? '—' }}</td>
                <td>
                  <button type="button" class="secondary" (click)="toggleDetail(item)">
                    {{ selectedMessageId === item.messageId ? 'Ẩn' : 'Xem' }}
                  </button>
                </td>
              </tr>
              <tr *ngIf="selectedMessageId === item.messageId" class="detail-row">
                <td colspan="7">
                  <div class="detail-panel">
                    <h2>Chi tiết DLQ</h2>
                    <dl class="detail-grid">
                      <dt>Queue</dt><dd>{{ item.queueName }}</dd>
                      <dt>MessageId</dt><dd class="mono">{{ item.messageId || '—' }}</dd>
                      <dt>NotificationId</dt><dd class="mono">{{ item.notificationId || '—' }}</dd>
                      <dt>Kênh</dt><dd>{{ displayChannel(item.channel) }}</dd>
                      <dt>Routing key</dt><dd>{{ item.routingKey || '—' }}</dd>
                      <dt>Event type</dt><dd>{{ item.eventType || '—' }}</dd>
                      <dt>Lý do</dt><dd>{{ item.reason || '—' }}</dd>
                      <dt>Dead-letter lúc</dt><dd>{{ formatDate(item.deadLetteredAtUtc) }}</dd>
                      <dt>Số lần</dt><dd>{{ item.deathCount ?? '—' }}</dd>
                    </dl>
                    <div class="payload-block">
                      <h3>Payload</h3>
                      <pre>{{ item.payload || '—' }}</pre>
                    </div>

                    <div class="detail-actions">
                      <button type="button" class="primary" (click)="openResendModal(item)">Gửi lại</button>
                    </div>

                    <div *ngIf="resendSuccessMessage && selectedMessageId === item.messageId" class="inline-success" role="status">
                      {{ resendSuccessMessage }}
                    </div>
                    <div *ngIf="resendErrorMessage && selectedMessageId === item.messageId" class="inline-error" role="alert">
                      <p>{{ resendErrorMessage }}</p>
                      <button type="button" class="secondary" (click)="openResendModal(item)">Thử lại</button>
                    </div>
                  </div>
                </td>
              </tr>
            </ng-container>
          </tbody>
        </table>

        <div class="pagination" aria-label="Thông tin danh sách">
          <div class="pagination-info">
            Hiển thị: {{ filteredItems.length }} / {{ items.length }} mục (limit {{ filterForm.controls.limit.value }})
          </div>
        </div>
      </div>

      <div *ngIf="resendModalOpen" class="modal-backdrop" role="presentation">
        <div class="modal" role="dialog" aria-modal="true" aria-labelledby="resend-modal-title">
          <h2 id="resend-modal-title">Xác nhận Gửi lại</h2>
          <p>Bạn có chắc muốn gửi lại Thông điệp này từ Hàng đợi thư chết?</p>
          <dl class="detail-grid modal-grid" *ngIf="resendCandidate">
            <dt>Queue</dt><dd>{{ resendCandidate.queueName }}</dd>
            <dt>MessageId</dt><dd class="mono">{{ resendCandidate.messageId }}</dd>
            <dt>NotificationId</dt><dd class="mono">{{ resendCandidate.notificationId || '—' }}</dd>
          </dl>

          <div *ngIf="resendProcessing" class="modal-status" aria-live="polite">Đang gửi lại...</div>
          <div *ngIf="resendErrorMessage && resendModalOpen" class="inline-error" role="alert">
            <p>{{ resendErrorMessage }}</p>
          </div>

          <div class="modal-actions">
            <button type="button" class="secondary" (click)="closeResendModal()" [disabled]="resendProcessing">Hủy</button>
            <button type="button" class="primary" (click)="confirmResend()" [disabled]="resendProcessing">Xác nhận</button>
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
    input:focus, select:focus, button:focus { outline: 2px solid #2563eb; outline-offset: 1px; border-color: #2563eb; }
    .filter-actions { display: flex; gap: 8px; flex-wrap: wrap; }
    button { border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; font-family: inherit; }
    .primary { background: #2563eb; color: #fff; border: 1px solid #2563eb; }
    .primary:hover { background: #1d4ed8; }
    .secondary { background: #fff; color: #0f172a; border: 1px solid #cbd5e1; }
    .secondary:hover { background: #f1f5f9; }
    .status-box { background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 20px; }
    .status-error { border-color: #fecaca; background: #fef2f2; color: #991b1b; }
    .status-error p { margin: 0 0 12px; color: #991b1b; }
    .status-empty h2 { margin: 0 0 8px; font-size: 16px; }
    .status-empty p { margin: 0 0 12px; color: #475569; }
    .table-wrap { background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; overflow: hidden; }
    .data-table { width: 100%; border-collapse: collapse; }
    th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e2e8f0; font-size: 14px; vertical-align: top; }
    th { background: #f8fafc; font-weight: 600; color: #0f172a; }
    .mono { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 12px; word-break: break-all; }
    .detail-row td { background: #f8fafc; }
    .detail-panel h2 { margin: 0 0 12px; font-size: 16px; }
    .detail-panel h3 { margin: 16px 0 8px; font-size: 14px; }
    .detail-grid { display: grid; grid-template-columns: 180px 1fr; gap: 8px 16px; margin: 0; }
    .detail-grid dt { font-weight: 600; color: #475569; }
    .detail-grid dd { margin: 0; word-break: break-word; }
    .payload-block pre { margin: 0; padding: 12px; background: #fff; border: 1px solid #cbd5e1; border-radius: 6px; white-space: pre-wrap; word-break: break-word; font-size: 12px; font-family: ui-monospace, SFMono-Regular, Consolas, monospace; }
    .detail-actions { margin-top: 16px; display: flex; gap: 8px; }
    .inline-success { margin-top: 12px; padding: 10px 12px; border-radius: 6px; background: #dcfce7; color: #166534; border: 1px solid #bbf7d0; font-size: 14px; }
    .inline-error { margin-top: 12px; padding: 10px 12px; border-radius: 6px; background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; font-size: 14px; }
    .inline-error p { margin: 0 0 8px; color: #991b1b; }
    .modal-backdrop { position: fixed; inset: 0; background: rgba(15, 23, 42, 0.45); display: flex; align-items: center; justify-content: center; padding: 16px; z-index: 50; }
    .modal { background: #fff; border-radius: 8px; border: 1px solid #cbd5e1; padding: 20px; width: min(560px, 100%); }
    .modal h2 { margin: 0 0 8px; font-size: 20px; }
    .modal p { margin: 0 0 12px; color: #475569; font-size: 14px; }
    .modal-grid { margin-bottom: 12px; }
    .modal-status { margin: 8px 0; color: #1d4ed8; font-size: 14px; font-weight: 600; }
    .modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
    button:disabled { opacity: 0.6; cursor: not-allowed; }
    .pagination { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 12px; border-top: 1px solid #e2e8f0; }
    .pagination-info { color: #475569; font-size: 14px; }
    .skeleton { height: 44px; margin: 12px; border-radius: 6px; background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%); background-size: 200% 100%; animation: shimmer 1.2s infinite; }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }
    @media (max-width: 1023px) { .table-wrap { overflow-x: auto; } .data-table { min-width: 900px; } .detail-grid { grid-template-columns: 1fr; } }
  `]
})
export class DeadLetterQueueListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly deadLetterQueueService = inject(DeadLetterQueueService);

  readonly skeletonRows = Array.from({ length: 6 });

  state: ViewState = 'loading';
  errorMessage = '';
  items: DeadLetterQueueMessage[] = [];
  filteredItems: DeadLetterQueueMessage[] = [];
  selectedMessageId: string | null = null;
  resendCandidate: DeadLetterQueueMessage | null = null;
  resendModalOpen = false;
  resendProcessing = false;
  resendSuccessMessage = '';
  resendErrorMessage = '';

  readonly filterForm = this.fb.nonNullable.group({
    queueName: ['notifications.email.dlq'],
    limit: [20],
    reason: [''],
    createdFrom: [''],
    createdTo: ['']
  });

  private activeFilters = {
    reason: '',
    createdFrom: '',
    createdTo: ''
  };

  /**
   * Khởi tạo component và tải danh sách DLQ.
   */
  ngOnInit(): void {
    this.loadMessages();
  }

  /**
   * Áp dụng bộ lọc và tải lại dữ liệu.
   */
  onApplyFilters(): void {
    const value = this.filterForm.getRawValue();
    this.activeFilters = {
      reason: (value.reason ?? '').trim().toLowerCase(),
      createdFrom: this.toIsoString(value.createdFrom),
      createdTo: this.toIsoString(value.createdTo)
    };
    this.loadMessages();
  }

  /**
   * Xóa bộ lọc và tải lại dữ liệu.
   */
  onClearFilters(): void {
    this.filterForm.reset({
      queueName: 'notifications.email.dlq',
      limit: 20,
      reason: '',
      createdFrom: '',
      createdTo: ''
    });
    this.activeFilters = { reason: '', createdFrom: '', createdTo: '' };
    this.selectedMessageId = null;
    this.loadMessages();
  }

  /**
   * Thử tải lại danh sách DLQ.
   */
  retry(): void {
    this.loadMessages();
  }

  /**
   * Mở modal xác nhận Gửi lại.
   * @param item Thông điệp DLQ.
   */
  openResendModal(item: DeadLetterQueueMessage): void {
    this.resendCandidate = item;
    this.resendModalOpen = true;
    this.resendErrorMessage = '';
    this.resendSuccessMessage = '';
  }

  /**
   * Đóng modal xác nhận Gửi lại.
   */
  closeResendModal(): void {
    if (this.resendProcessing) {
      return;
    }
    this.resendModalOpen = false;
    this.resendCandidate = null;
  }

  /**
   * Xác nhận và thực hiện Gửi lại Thông điệp DLQ.
   */
  confirmResend(): void {
    if (!this.resendCandidate || this.resendProcessing) {
      return;
    }

    const candidate = this.resendCandidate;
    this.resendProcessing = true;
    this.resendErrorMessage = '';
    this.resendSuccessMessage = '';

    this.deadLetterQueueService
      .resendMessage(candidate.queueName, candidate.messageId, { idempotencyKey: this.createIdempotencyKey() })
      .pipe(finalize(() => {
        this.resendProcessing = false;
      }))
      .subscribe({
        next: (response) => {
          this.resendSuccessMessage = `Gửi lại thành công. MessageId mới: ${response.resentMessageId}`;
          this.resendModalOpen = false;
          this.resendCandidate = null;
          this.loadMessages();
        },
        error: (error: unknown) => {
          this.resendErrorMessage = this.resolveErrorMessage(error, true);
        }
      });
  }

  /**
   * Tạo key theo MessageId cho danh sách.
   * @param _ Index của item.
   * @param item Thông điệp DLQ.
   * @returns MessageId.
   */
  trackByMessageId(_: number, item: DeadLetterQueueMessage): string {
    return item.messageId || `${item.queueName}-${_}`;
  }

  /**
   * Hiển thị tên kênh dễ đọc.
   * @param channel Kênh gửi.
   * @returns Chuỗi hiển thị.
   */
  displayChannel(channel: string | null): string {
    const normalized = (channel || '').toLowerCase();
    if (normalized === 'email') return 'Email';
    if (normalized === 'inapp') return 'Thông báo trong ứng dụng';
    return channel || '—';
  }

  /**
   * Định dạng thời gian hiển thị.
   * @param value Giá trị thời gian.
   * @returns Chuỗi thời gian định dạng.
   */
  formatDate(value: string | null): string {
    if (!value) return '—';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    return date.toLocaleString('vi-VN', { hour12: false });
  }

  /**
   * Tải danh sách Thông điệp DLQ từ API.
   */
  private loadMessages(): void {
    this.state = 'loading';
    this.errorMessage = '';
    this.selectedMessageId = null;
    this.resendSuccessMessage = '';
    this.resendErrorMessage = '';
    const value = this.filterForm.getRawValue();

    this.deadLetterQueueService
      .getMessages(value.queueName, Number(value.limit))
      .pipe(finalize(() => {
        // state set in handlers
      }))
      .subscribe({
        next: (response) => {
          this.items = response.items ?? [];
          this.applyClientFilters();
          this.state = this.filteredItems.length === 0 ? 'empty' : 'success';
        },
        error: (error: unknown) => {
          this.state = 'error';
          this.errorMessage = this.resolveErrorMessage(error);
        }
      });
  }

  /**
   * Áp dụng bộ lọc phía client trên dữ liệu đã tải.
   */
  private applyClientFilters(): void {
    const reason = this.activeFilters.reason;
    const from = this.activeFilters.createdFrom ? new Date(this.activeFilters.createdFrom).getTime() : null;
    const to = this.activeFilters.createdTo ? new Date(this.activeFilters.createdTo).getTime() : null;

    this.filteredItems = this.items.filter((item) => {
      if (reason && !(item.reason || '').toLowerCase().includes(reason)) {
        return false;
      }

      const time = item.deadLetteredAtUtc ? new Date(item.deadLetteredAtUtc).getTime() : null;
      if (from !== null && (time === null || time < from)) {
        return false;
      }
      if (to !== null && (time === null || time > to)) {
        return false;
      }

      return true;
    });
  }

  /**
   * Chuyển giá trị datetime-local sang ISO string.
   * @param value Giá trị datetime-local.
   * @returns Chuỗi ISO hoặc rỗng.
   */
  private toIsoString(value: string | null | undefined): string {
    if (!value) return '';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '';
    return date.toISOString();
  }

  /**
   * Tạo khóa idempotency cho thao tác Gửi lại.
   * @returns Khóa idempotency.
   */
  private createIdempotencyKey(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }

    return 'idem-' + Date.now() + '-' + Math.random().toString(16).slice(2);
  }

  /**
   * Chuyển lỗi HTTP thành thông báo thân thiện.
   * @param error Lỗi cần xử lý.
   * @param isResend Có phải lỗi từ thao tác Gửi lại hay không.
   * @returns Thông báo lỗi.
   */
  private resolveErrorMessage(error: unknown, isResend = false): string {
    const fallback = isResend
      ? 'Không gửi lại được Thông điệp. Vui lòng thử lại.'
      : 'Không tải được danh sách Hàng đợi thư chết. Vui lòng thử lại.';

    if (error instanceof HttpErrorResponse) {
      if (error.status === 401 || error.status === 403) {
        return isResend
          ? 'Không có quyền Gửi lại Thông điệp. Vui lòng kiểm tra phiên đăng nhập và quyền RBAC.'
          : 'Không có quyền truy cập Hàng đợi thư chết. Vui lòng kiểm tra phiên đăng nhập và quyền RBAC.';
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
      if (payload?.detail) return payload.detail;
      if (payload?.title) return payload.title;
      return fallback;
    }
    return fallback;
  }
}
