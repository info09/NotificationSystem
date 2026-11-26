import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { NotificationChannelConfig } from '../../core/notification-channel-configs/notification-channel-config.models';
import { NotificationChannelConfigService } from '../../core/notification-channel-configs/notification-channel-config.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { ToastService } from '../../shared/ui/toast/toast.service';
import { NotificationChannelConfigDialogComponent } from './notification-channel-config-dialog.component';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-notification-channel-configs',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatTableModule,
    MatDialogModule,
    EmptyStateComponent,
    ErrorStateComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent
  ],
  template: `
    <section class="page-container" aria-labelledby="notification-channel-configs-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Quản trị</p>
          <h1 id="notification-channel-configs-title" class="page-title">Cấu hình kênh thông báo</h1>
          <p class="page-subtitle">Quản lý trạng thái bật/tắt và thông tin cấu hình kênh thông báo.</p>
        </div>
        <button mat-flat-button color="primary" type="button" (click)="openCreateDialog()">Tạo cấu hình</button>
      </header>

      <mat-card class="panel-card filter-card">
        <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()">
          <mat-form-field appearance="outline">
            <mat-label>Kênh</mat-label>
            <mat-select formControlName="channel">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="Email">Email</mat-option>
              <mat-option value="InApp">InApp</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Trạng thái</mat-label>
            <mat-select formControlName="isEnabled">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="true">Đang bật</mat-option>
              <mat-option value="false">Đang tắt</mat-option>
            </mat-select>
          </mat-form-field>
          <div class="filter-actions">
            <button mat-flat-button color="primary" type="submit">Áp dụng</button>
            <button mat-stroked-button type="button" (click)="clearFilters()">Xóa bộ lọc</button>
          </div>
        </form>
      </mat-card>

      <mat-card class="panel-card">
        <mat-card-content>
          <app-error-state *ngIf="state === 'error'" [message]="errorMessage" (retry)="loadConfigs()"></app-error-state>
          <app-loading-skeleton *ngIf="state === 'loading'" [rows]="5" variant="line"></app-loading-skeleton>
          <app-empty-state *ngIf="state === 'empty'" title="Không có Cấu hình kênh thông báo" description="Chưa có cấu hình phù hợp với bộ lọc hiện tại."></app-empty-state>

          <div *ngIf="state === 'success'" class="table-wrap">
            <table mat-table [dataSource]="configs" class="data-table">
              <ng-container matColumnDef="channel">
                <th mat-header-cell *matHeaderCellDef>Kênh</th>
                <td mat-cell *matCellDef="let c">{{ c.channel }}</td>
              </ng-container>
              <ng-container matColumnDef="displayName">
                <th mat-header-cell *matHeaderCellDef>Tên hiển thị</th>
                <td mat-cell *matCellDef="let c">{{ c.displayName }}</td>
              </ng-container>
              <ng-container matColumnDef="description">
                <th mat-header-cell *matHeaderCellDef>Mô tả</th>
                <td mat-cell *matCellDef="let c">{{ c.description || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Trạng thái</th>
                <td mat-cell *matCellDef="let c">
                  <app-status-badge [label]="c.isEnabled ? 'Đang bật' : 'Đang tắt'" [tone]="c.isEnabled ? 'success' : 'default'"></app-status-badge>
                </td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Thao tác</th>
                <td mat-cell *matCellDef="let c">
                  <div class="actions">
                    <button mat-stroked-button type="button" (click)="openEditDialog(c)">Sửa</button>
                    <button mat-stroked-button color="warn" type="button" (click)="deleteConfig(c)" [disabled]="actionLoadingId === c.id">Xóa</button>
                  </div>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>

            <div class="pagination">
              <div>Tổng: {{ totalCount }} · Trang {{ page }} / {{ totalPages || 1 }}</div>
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
    .filter-actions, .actions, .pagination-actions { display:flex; gap:8px; flex-wrap:wrap; align-items:center; }
    .filter-actions { padding-top: 8px; }
    .table-wrap { overflow-x: auto; }
    .data-table { width: 100%; }
    .pagination { display:flex; justify-content:space-between; gap:12px; padding-top:16px; align-items:center; flex-wrap:wrap; color:var(--app-muted); font-size:14px; }
  `]
})
export class NotificationChannelConfigsComponent implements OnInit {
  private readonly configService = inject(NotificationChannelConfigService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly toastService = inject(ToastService);

  readonly displayedColumns = ['channel', 'displayName', 'description', 'status', 'actions'];

  state: ViewState = 'loading';
  configs: NotificationChannelConfig[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  errorMessage = 'Không thể tải danh sách Cấu hình kênh thông báo.';
  actionLoadingId: string | null = null;

  readonly filterForm = this.fb.nonNullable.group({
    channel: [''],
    isEnabled: ['']
  });

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  ngOnInit(): void {
    this.loadConfigs();
  }

  loadConfigs(): void {
    this.state = 'loading';
    this.errorMessage = 'Không thể tải danh sách Cấu hình kênh thông báo.';
    const { channel, isEnabled } = this.filterForm.getRawValue();

    this.configService.getNotificationChannelConfigs({
      channel: channel || undefined,
      isEnabled: isEnabled === '' ? undefined : isEnabled === 'true',
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.configs = response.items;
        this.totalCount = response.totalCount;
        this.page = response.page;
        this.pageSize = response.pageSize;
        this.state = this.configs.length === 0 ? 'empty' : 'success';
      },
      error: (error: unknown) => {
        this.state = 'error';
        this.errorMessage = this.getErrorMessage(error, 'Không thể tải danh sách Cấu hình kênh thông báo.');
      }
    });
  }

  onApplyFilters(): void {
    this.page = 1;
    this.loadConfigs();
  }

  clearFilters(): void {
    this.filterForm.reset({ channel: '', isEnabled: '' });
    this.page = 1;
    this.loadConfigs();
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(NotificationChannelConfigDialogComponent, {
      data: { config: null },
      width: '640px',
      maxWidth: '95vw'
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.configService.createNotificationChannelConfig(result).subscribe({
        next: () => {
          this.toastService.success('Tạo Cấu hình kênh thông báo thành công.');
          this.loadConfigs();
        },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu Cấu hình kênh thông báo.'))
      });
    });
  }

  openEditDialog(config: NotificationChannelConfig): void {
    const ref = this.dialog.open(NotificationChannelConfigDialogComponent, {
      data: { config },
      width: '640px',
      maxWidth: '95vw'
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.configService.updateNotificationChannelConfig(config.id, result).subscribe({
        next: () => {
          this.toastService.success('Cập nhật Cấu hình kênh thông báo thành công.');
          this.loadConfigs();
        },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu Cấu hình kênh thông báo.'))
      });
    });
  }

  deleteConfig(config: NotificationChannelConfig): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Xóa Cấu hình kênh thông báo',
        message: `Xóa cấu hình kênh "${config.displayName}"?`,
        confirmLabel: 'Xóa',
        tone: 'warn'
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.actionLoadingId = config.id;
      this.configService.deleteNotificationChannelConfig(config.id)
        .pipe(finalize(() => (this.actionLoadingId = null)))
        .subscribe({
          next: () => {
            this.toastService.success('Xóa Cấu hình kênh thông báo thành công.');
            this.loadConfigs();
          },
          error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể xóa Cấu hình kênh thông báo.'))
        });
    });
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadConfigs();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page += 1;
      this.loadConfigs();
    }
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 400 || error.status === 422) return 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.';
      if (error.status === 401) return 'Phiên đăng nhập không hợp lệ hoặc đã hết hạn.';
      if (error.status === 403) return 'Bạn không có quyền thực hiện thao tác này.';
      if (error.status === 409) return 'Dữ liệu bị xung đột. Vui lòng kiểm tra lại.';
      if (error.status === 404) return 'Không tìm thấy dữ liệu.';
    }
    return fallback;
  }
}
