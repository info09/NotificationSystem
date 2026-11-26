import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DeadLetterQueueService } from '../../core/dead-letter-queue/dead-letter-queue.service';
import { DeadLetterQueueMessage } from '../../core/dead-letter-queue/dead-letter-queue.models';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { ToastService } from '../../shared/ui/toast/toast.service';
import { DeadLetterQueueDetailDialogComponent, DeadLetterQueueDetailDialogData } from './dead-letter-queue-detail-dialog.component';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-dead-letter-queue-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatTableModule, MatDialogModule, MatDatepickerModule, MatNativeDateModule, EmptyStateComponent, ErrorStateComponent, LoadingSkeletonComponent],
  template: `
    <section class="page-container" aria-labelledby="dlq-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Vận hành</p>
          <h1 id="dlq-title" class="page-title">Hàng đợi thư chết</h1>
          <p class="page-subtitle">Tra cứu Thông điệp lỗi trong Hàng đợi thư chết và xem chi tiết tóm tắt.</p>
        </div>
      </header>

      <mat-card class="panel-card filter-card">
        <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()" aria-label="Bộ lọc Hàng đợi thư chết">
          <mat-form-field appearance="outline">
            <mat-label>Hàng đợi thư chết</mat-label>
            <mat-select formControlName="queueName">
              <mat-option value="notifications.email.dlq">notifications.email.dlq</mat-option>
              <mat-option value="notifications.inapp.dlq">notifications.inapp.dlq</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Số lượng</mat-label>
            <mat-select formControlName="limit">
              <mat-option [value]="20">20</mat-option>
              <mat-option [value]="50">50</mat-option>
              <mat-option [value]="100">100</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Lý do (lọc tạm)</mat-label>
            <input matInput type="text" formControlName="reason" placeholder="Nhập lý do..." />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Từ ngày</mat-label>
            <input matInput [matDatepicker]="dlqFromPicker" formControlName="createdFrom" readonly (click)="dlqFromPicker.open()" (focus)="dlqFromPicker.open()" />
            <mat-datepicker-toggle matIconSuffix [for]="dlqFromPicker"></mat-datepicker-toggle>
            <mat-datepicker #dlqFromPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Đến ngày</mat-label>
            <input matInput [matDatepicker]="dlqToPicker" formControlName="createdTo" readonly (click)="dlqToPicker.open()" (focus)="dlqToPicker.open()" />
            <mat-datepicker-toggle matIconSuffix [for]="dlqToPicker"></mat-datepicker-toggle>
            <mat-datepicker #dlqToPicker></mat-datepicker>
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
            title="Không có Thông điệp trong Hàng đợi thư chết"
            description="Hiện không có mục lỗi theo bộ lọc hiện tại. Đây là trạng thái tốt."
            actionLabel="Xóa bộ lọc"
            (action)="onClearFilters()"
          ></app-empty-state>

          <div *ngIf="state === 'success'" class="table-wrap">
            <table mat-table [dataSource]="filteredItems" class="data-table">
              <ng-container matColumnDef="messageId">
                <th mat-header-cell *matHeaderCellDef>MessageId</th>
                <td mat-cell *matCellDef="let item" class="mono">{{ item.messageId || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="notificationId">
                <th mat-header-cell *matHeaderCellDef>NotificationId</th>
                <td mat-cell *matCellDef="let item" class="mono">{{ item.notificationId || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="channel">
                <th mat-header-cell *matHeaderCellDef>Kênh</th>
                <td mat-cell *matCellDef="let item">{{ displayChannel(item.channel) }}</td>
              </ng-container>
              <ng-container matColumnDef="reason">
                <th mat-header-cell *matHeaderCellDef>Lý do</th>
                <td mat-cell *matCellDef="let item">{{ item.reason || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="deadLetteredAtUtc">
                <th mat-header-cell *matHeaderCellDef>Thời gian vào Hàng đợi thư chết</th>
                <td mat-cell *matCellDef="let item">{{ formatDate(item.deadLetteredAtUtc) }}</td>
              </ng-container>
              <ng-container matColumnDef="deathCount">
                <th mat-header-cell *matHeaderCellDef>Số lần</th>
                <td mat-cell *matCellDef="let item">{{ item.deathCount ?? '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="detail">
                <th mat-header-cell *matHeaderCellDef>Chi tiết</th>
                <td mat-cell *matCellDef="let item">
                  <button mat-stroked-button type="button" (click)="openDetail(item)">Xem</button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>

            <div class="pagination-info">
              Hiển thị: {{ filteredItems.length }} / {{ items.length }} mục (limit {{ filterForm.controls.limit.value }})
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
    .pagination-info { color: var(--app-muted); font-size: 14px; padding-top: 16px; }
  `]
})
export class DeadLetterQueueListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly deadLetterQueueService = inject(DeadLetterQueueService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = inject(MatDialog);

  readonly displayedColumns = ['messageId', 'notificationId', 'channel', 'reason', 'deadLetteredAtUtc', 'deathCount', 'detail'];

  state: ViewState = 'loading';
  errorMessage = '';
  items: DeadLetterQueueMessage[] = [];
  filteredItems: DeadLetterQueueMessage[] = [];
  private resendProcessing = false;

  readonly filterForm = this.fb.group({
    queueName: this.fb.nonNullable.control('notifications.email.dlq'),
    limit: this.fb.nonNullable.control(20),
    reason: this.fb.nonNullable.control(''),
    createdFrom: this.fb.control<Date | null>(null),
    createdTo: this.fb.control<Date | null>(null)
  });

  private activeFilters = {
    reason: '',
    createdFrom: '',
    createdTo: ''
  };

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const queueName = params.get('queueName');
        if (queueName === 'notifications.email.dlq' || queueName === 'notifications.inapp.dlq') {
          if (this.filterForm.controls.queueName.value !== queueName) {
            this.filterForm.controls.queueName.setValue(queueName);
          }
        }

        this.loadMessages();
      });
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
    this.activeFilters = {
      reason: (value.reason ?? '').trim().toLowerCase(),
      createdFrom: this.toIsoString(createdFrom),
      createdTo: this.toIsoString(createdTo, true)
    };
    this.loadMessages();
  }

  onClearFilters(): void {
    this.filterForm.reset({
      queueName: 'notifications.email.dlq',
      limit: 20,
      reason: '',
      createdFrom: null,
      createdTo: null
    });
    this.filterForm.controls.createdTo.setErrors(null);
    this.activeFilters = { reason: '', createdFrom: '', createdTo: '' };
    this.loadMessages();
  }

  retry(): void {
    this.loadMessages();
  }

  openDetail(item: DeadLetterQueueMessage): void {
    const dialogRef = this.dialog.open(DeadLetterQueueDetailDialogComponent, {
      width: '760px',
      maxWidth: '95vw',
      data: {
        item,
        resendProcessing: false,
        resendErrorMessage: '',
        resendSuccessMessage: ''
      } as DeadLetterQueueDetailDialogData
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result === 'resend') {
        this.confirmResend(item, dialogRef.componentInstance);
      }
    });
  }

  private confirmResend(item: DeadLetterQueueMessage, detailDialog?: DeadLetterQueueDetailDialogComponent): void {
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Xác nhận Gửi lại',
        message: `Bạn có chắc muốn gửi lại Thông điệp ${item.messageId} từ Hàng đợi thư chết?`,
        confirmLabel: 'Gửi lại',
        cancelLabel: 'Hủy'
      }
    });

    confirmRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.executeResend(item, detailDialog);
      }
    });
  }

  private executeResend(item: DeadLetterQueueMessage, detailDialog?: DeadLetterQueueDetailDialogComponent): void {
    if (this.resendProcessing) return;
    this.resendProcessing = true;
    if (detailDialog) {
      detailDialog.data.resendProcessing = true;
      detailDialog.data.resendErrorMessage = '';
      detailDialog.data.resendSuccessMessage = '';
    }

    this.deadLetterQueueService
      .resendMessage(item.queueName, item.messageId, { idempotencyKey: this.createIdempotencyKey() })
      .pipe(finalize(() => {
        this.resendProcessing = false;
        if (detailDialog) detailDialog.data.resendProcessing = false;
      }))
      .subscribe({
        next: (response) => {
          const message = `Gửi lại thành công. MessageId mới: ${response.resentMessageId}`;
          this.toastService.success(message);
          if (detailDialog) detailDialog.data.resendSuccessMessage = message;
          this.loadMessages();
        },
        error: (error: unknown) => {
          const message = this.resolveErrorMessage(error, true);
          if (detailDialog) detailDialog.data.resendErrorMessage = message;
          this.toastService.error(message);
        }
      });
  }

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

  private loadMessages(): void {
    this.state = 'loading';
    this.errorMessage = '';
    const value = this.filterForm.getRawValue();

    this.deadLetterQueueService
      .getMessages(value.queueName, Number(value.limit))
      .pipe(finalize(() => {}))
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

  private applyClientFilters(): void {
    const reason = this.activeFilters.reason;
    const from = this.activeFilters.createdFrom ? new Date(this.activeFilters.createdFrom).getTime() : null;
    const to = this.activeFilters.createdTo ? new Date(this.activeFilters.createdTo).getTime() : null;

    this.filteredItems = this.items.filter((item) => {
      if (reason && !(item.reason || '').toLowerCase().includes(reason)) return false;
      const time = item.deadLetteredAtUtc ? new Date(item.deadLetteredAtUtc).getTime() : null;
      if (from !== null && (time === null || time < from)) return false;
      if (to !== null && (time === null || time > to)) return false;
      return true;
    });
  }

  private toIsoString(value: Date | string | null | undefined, endOfDay = false): string {
    if (!value) return '';
    const date = value instanceof Date ? new Date(value.getTime()) : new Date(value);
    if (Number.isNaN(date.getTime())) return '';
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

  private createIdempotencyKey(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }
    return 'idem-' + Date.now() + '-' + Math.random().toString(16).slice(2);
  }

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
      if (error.status === 0) return 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
      const payload = error.error as { detail?: string; title?: string; errors?: Record<string, string[]> } | null;
      if (payload?.errors) {
        const messages = Object.values(payload.errors).flat().filter((m): m is string => typeof m === 'string');
        if (messages.length > 0) return messages.join(' ');
      }
      if (payload?.detail) return payload.detail;
      if (payload?.title) return payload.title;
      return fallback;
    }
    return fallback;
  }
}
