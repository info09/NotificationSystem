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
import { IntegrationEventRoute } from '../../core/integration-event-routes/integration-event-route.models';
import { IntegrationEventRouteService } from '../../core/integration-event-routes/integration-event-route.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { ToastService } from '../../shared/ui/toast/toast.service';
import { IntegrationEventRouteDialogComponent } from './integration-event-route-dialog.component';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-integration-event-routes',
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
    <section class="page-container" aria-labelledby="integration-event-routes-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Quản trị</p>
          <h1 id="integration-event-routes-title" class="page-title">Định tuyến sự kiện tích hợp</h1>
          <p class="page-subtitle">Quản lý ánh xạ sự kiện tích hợp tới Mẫu thông báo và/hoặc Kênh thông báo.</p>
        </div>
        <button mat-flat-button color="primary" type="button" (click)="openCreateDialog()">Tạo định tuyến</button>
      </header>

      <mat-card class="panel-card filter-card">
        <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()">
          <mat-form-field appearance="outline">
            <mat-label>Loại sự kiện</mat-label>
            <input matInput formControlName="eventType" placeholder="Ví dụ: OrderCreated" />
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
          <app-error-state *ngIf="state === 'error'" [message]="errorMessage" (retry)="loadRoutes()"></app-error-state>
          <app-loading-skeleton *ngIf="state === 'loading'" [rows]="5" variant="line"></app-loading-skeleton>
          <app-empty-state *ngIf="state === 'empty'" title="Không có Định tuyến sự kiện tích hợp" description="Chưa có định tuyến phù hợp với bộ lọc hiện tại."></app-empty-state>

          <div *ngIf="state === 'success'" class="table-wrap">
            <table mat-table [dataSource]="routes" class="data-table">
              <ng-container matColumnDef="eventType">
                <th mat-header-cell *matHeaderCellDef>Loại sự kiện</th>
                <td mat-cell *matCellDef="let r">{{ r.eventType }}</td>
              </ng-container>
              <ng-container matColumnDef="channel">
                <th mat-header-cell *matHeaderCellDef>Kênh</th>
                <td mat-cell *matCellDef="let r">{{ r.channel || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="notificationTemplateId">
                <th mat-header-cell *matHeaderCellDef>Mẫu thông báo</th>
                <td mat-cell *matCellDef="let r">{{ r.notificationTemplateId || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Trạng thái</th>
                <td mat-cell *matCellDef="let r">
                  <app-status-badge [label]="r.isActive ? 'Đang kích hoạt' : 'Ngừng kích hoạt'" [tone]="r.isActive ? 'success' : 'default'"></app-status-badge>
                </td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Thao tác</th>
                <td mat-cell *matCellDef="let r">
                  <div class="actions">
                    <button mat-stroked-button type="button" (click)="openEditDialog(r)">Sửa</button>
                    <button mat-stroked-button color="warn" type="button" (click)="deleteRoute(r)" [disabled]="actionLoadingId === r.id">Xóa</button>
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
export class IntegrationEventRoutesComponent implements OnInit {
  private readonly routeService = inject(IntegrationEventRouteService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly toastService = inject(ToastService);

  readonly displayedColumns = ['eventType', 'channel', 'notificationTemplateId', 'status', 'actions'];

  state: ViewState = 'loading';
  routes: IntegrationEventRoute[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  errorMessage = 'Không thể tải danh sách Định tuyến sự kiện tích hợp.';
  actionLoadingId: string | null = null;

  readonly filterForm = this.fb.nonNullable.group({
    eventType: [''],
    channel: [''],
    isActive: ['']
  });

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  ngOnInit(): void {
    this.loadRoutes();
  }

  loadRoutes(): void {
    this.state = 'loading';
    this.errorMessage = 'Không thể tải danh sách Định tuyến sự kiện tích hợp.';
    const { eventType, channel, isActive } = this.filterForm.getRawValue();

    this.routeService.getIntegrationEventRoutes({
      eventType: eventType || undefined,
      channel: channel || undefined,
      isActive: isActive === '' ? undefined : isActive === 'true',
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.routes = response.items;
        this.totalCount = response.totalCount;
        this.page = response.page;
        this.pageSize = response.pageSize;
        this.state = this.routes.length === 0 ? 'empty' : 'success';
      },
      error: (error: unknown) => {
        this.state = 'error';
        this.errorMessage = this.getErrorMessage(error, 'Không thể tải danh sách Định tuyến sự kiện tích hợp.');
      }
    });
  }

  onApplyFilters(): void {
    this.page = 1;
    this.loadRoutes();
  }

  clearFilters(): void {
    this.filterForm.reset({ eventType: '', channel: '', isActive: '' });
    this.page = 1;
    this.loadRoutes();
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(IntegrationEventRouteDialogComponent, {
      data: { route: null },
      width: '760px',
      maxWidth: '95vw'
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.routeService.createIntegrationEventRoute(result).subscribe({
        next: () => {
          this.toastService.success('Tạo Định tuyến sự kiện tích hợp thành công.');
          this.loadRoutes();
        },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu Định tuyến sự kiện tích hợp.'))
      });
    });
  }

  openEditDialog(route: IntegrationEventRoute): void {
    const ref = this.dialog.open(IntegrationEventRouteDialogComponent, {
      data: { route },
      width: '760px',
      maxWidth: '95vw'
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.routeService.updateIntegrationEventRoute(route.id, result).subscribe({
        next: () => {
          this.toastService.success('Cập nhật Định tuyến sự kiện tích hợp thành công.');
          this.loadRoutes();
        },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu Định tuyến sự kiện tích hợp.'))
      });
    });
  }

  deleteRoute(route: IntegrationEventRoute): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Xóa Định tuyến sự kiện tích hợp',
        message: `Xóa định tuyến cho sự kiện "${route.eventType}"?`,
        confirmLabel: 'Xóa',
        tone: 'warn'
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.actionLoadingId = route.id;
      this.routeService.deleteIntegrationEventRoute(route.id)
        .pipe(finalize(() => (this.actionLoadingId = null)))
        .subscribe({
          next: () => {
            this.toastService.success('Xóa Định tuyến sự kiện tích hợp thành công.');
            this.loadRoutes();
          },
          error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể xóa Định tuyến sự kiện tích hợp.'))
        });
    });
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadRoutes();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page += 1;
      this.loadRoutes();
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
