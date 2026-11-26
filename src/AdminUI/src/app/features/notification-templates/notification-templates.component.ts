import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { NotificationTemplate } from '../../core/notification-templates/notification-template.models';
import { NotificationTemplateService } from '../../core/notification-templates/notification-template.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { ToastService } from '../../shared/ui/toast/toast.service';
import { NotificationTemplateDialogComponent } from './notification-template-dialog.component';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-notification-templates',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatDialogModule,
    EmptyStateComponent,
    ErrorStateComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent
  ],
  template: `
    <section class="page-container" aria-labelledby="notification-templates-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Quản trị</p>
          <h1 id="notification-templates-title" class="page-title">Mẫu thông báo</h1>
          <p class="page-subtitle">Quản lý mẫu nội dung thông báo theo kênh và trạng thái kích hoạt.</p>
        </div>
        <button mat-flat-button color="primary" type="button" (click)="openCreateDialog()">Tạo mẫu</button>
      </header>

      <mat-card class="panel-card filter-card">
        <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()">
          <mat-form-field appearance="outline">
            <mat-label>Từ khóa</mat-label>
            <input matInput formControlName="keyword" placeholder="Tìm theo khóa hoặc tên" />
          </mat-form-field>
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
            <mat-select formControlName="isActive">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="true">Đang kích hoạt</mat-option>
              <mat-option value="false">Ngừng kích hoạt</mat-option>
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
          <app-error-state *ngIf="state === 'error'" [message]="errorMessage" (retry)="loadTemplates()"></app-error-state>
          <app-loading-skeleton *ngIf="state === 'loading'" [rows]="5" variant="line"></app-loading-skeleton>
          <app-empty-state *ngIf="state === 'empty'" title="Không có Mẫu thông báo" description="Chưa có Mẫu thông báo phù hợp với bộ lọc hiện tại."></app-empty-state>

          <div *ngIf="state === 'success'" class="table-wrap">
            <table mat-table [dataSource]="templates" class="data-table">
              <ng-container matColumnDef="templateKey">
                <th mat-header-cell *matHeaderCellDef>Khóa mẫu</th>
                <td mat-cell *matCellDef="let t">{{ t.templateKey }}</td>
              </ng-container>
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Tên</th>
                <td mat-cell *matCellDef="let t">{{ t.name }}</td>
              </ng-container>
              <ng-container matColumnDef="channel">
                <th mat-header-cell *matHeaderCellDef>Kênh</th>
                <td mat-cell *matCellDef="let t">{{ t.channel }}</td>
              </ng-container>
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Trạng thái</th>
                <td mat-cell *matCellDef="let t">
                  <app-status-badge [label]="t.isActive ? 'Đang kích hoạt' : 'Ngừng kích hoạt'" [tone]="t.isActive ? 'success' : 'default'"></app-status-badge>
                </td>
              </ng-container>
              <ng-container matColumnDef="updatedAtUtc">
                <th mat-header-cell *matHeaderCellDef>Cập nhật</th>
                <td mat-cell *matCellDef="let t">{{ t.updatedAtUtc | date:'dd/MM/yyyy HH:mm' }}</td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Thao tác</th>
                <td mat-cell *matCellDef="let t">
                  <div class="actions">
                    <button mat-stroked-button type="button" (click)="openEditDialog(t)">Sửa</button>
                    <button mat-stroked-button color="warn" type="button" (click)="deleteTemplate(t)" [disabled]="actionLoadingId === t.id">Xóa</button>
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
export class NotificationTemplatesComponent implements OnInit {
  private readonly templateService = inject(NotificationTemplateService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly toastService = inject(ToastService);

  readonly displayedColumns = ['templateKey', 'name', 'channel', 'status', 'updatedAtUtc', 'actions'];

  state: ViewState = 'loading';
  templates: NotificationTemplate[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  errorMessage = 'Không thể tải danh sách Mẫu thông báo.';
  actionLoadingId: string | null = null;

  readonly filterForm = this.fb.nonNullable.group({
    keyword: [''],
    channel: [''],
    isActive: ['']
  });

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.state = 'loading';
    this.errorMessage = 'Không thể tải danh sách Mẫu thông báo.';
    const { keyword, channel, isActive } = this.filterForm.getRawValue();

    this.templateService.getNotificationTemplates({
      keyword: keyword || undefined,
      channel: channel || undefined,
      isActive: isActive === '' ? undefined : isActive === 'true',
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.templates = response.items;
        this.totalCount = response.totalCount;
        this.page = response.page;
        this.pageSize = response.pageSize;
        this.state = this.templates.length === 0 ? 'empty' : 'success';
      },
      error: (error: unknown) => {
        this.state = 'error';
        this.errorMessage = this.getErrorMessage(error, 'Không thể tải danh sách Mẫu thông báo.');
      }
    });
  }

  onApplyFilters(): void {
    this.page = 1;
    this.loadTemplates();
  }

  clearFilters(): void {
    this.filterForm.reset({ keyword: '', channel: '', isActive: '' });
    this.page = 1;
    this.loadTemplates();
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(NotificationTemplateDialogComponent, {
      data: { template: null },
      width: '760px',
      maxWidth: '95vw'
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.templateService.createNotificationTemplate(result).subscribe({
        next: () => {
          this.toastService.success('Tạo Mẫu thông báo thành công.');
          this.loadTemplates();
        },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu Mẫu thông báo.'))
      });
    });
  }

  openEditDialog(template: NotificationTemplate): void {
    const ref = this.dialog.open(NotificationTemplateDialogComponent, {
      data: { template },
      width: '760px',
      maxWidth: '95vw'
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.templateService.updateNotificationTemplate(template.id, result).subscribe({
        next: () => {
          this.toastService.success('Cập nhật Mẫu thông báo thành công.');
          this.loadTemplates();
        },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu Mẫu thông báo.'))
      });
    });
  }

  deleteTemplate(template: NotificationTemplate): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Xóa Mẫu thông báo',
        message: `Xóa Mẫu thông báo "${template.name}"?`,
        confirmLabel: 'Xóa',
        tone: 'warn'
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.actionLoadingId = template.id;
      this.templateService.deleteNotificationTemplate(template.id)
        .pipe(finalize(() => (this.actionLoadingId = null)))
        .subscribe({
          next: () => {
            this.toastService.success('Xóa Mẫu thông báo thành công.');
            this.loadTemplates();
          },
          error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể xóa Mẫu thông báo.'))
        });
    });
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadTemplates();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page += 1;
      this.loadTemplates();
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
