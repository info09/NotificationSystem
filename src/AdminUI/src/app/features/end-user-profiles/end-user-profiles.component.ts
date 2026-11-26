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
import { EndUserProfile } from '../../core/end-user-profiles/end-user-profile.models';
import { EndUserProfileService } from '../../core/end-user-profiles/end-user-profile.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { ToastService } from '../../shared/ui/toast/toast.service';
import { EndUserProfileDialogComponent } from './end-user-profile-dialog.component';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-end-user-profiles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatTableModule, MatDialogModule, EmptyStateComponent, ErrorStateComponent, LoadingSkeletonComponent, StatusBadgeComponent],
  template: `
    <section class="page-container" aria-labelledby="end-user-profiles-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Quản trị</p>
          <h1 id="end-user-profiles-title" class="page-title">Hồ sơ người nhận EndUser</h1>
          <p class="page-subtitle">Quản lý hồ sơ EndUser theo định danh hệ thống nguồn.</p>
        </div>
        <button mat-flat-button color="primary" type="button" (click)="openCreateDialog()">Tạo hồ sơ</button>
      </header>

      <mat-card class="panel-card filter-card">
        <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()">
          <mat-form-field appearance="outline">
            <mat-label>Trạng thái</mat-label>
            <mat-select formControlName="status">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="Active">Active</mat-option>
              <mat-option value="Locked">Locked</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Định danh nguồn</mat-label>
            <input matInput formControlName="externalUserId" />
          </mat-form-field>
          <div class="filter-actions">
            <button mat-flat-button color="primary" type="submit">Áp dụng</button>
            <button mat-stroked-button type="button" (click)="clearFilters()">Xóa bộ lọc</button>
          </div>
        </form>
      </mat-card>

      <mat-card class="panel-card">
        <mat-card-content>
          <app-error-state *ngIf="state === 'error'" [message]="errorMessage" (retry)="loadProfiles()"></app-error-state>
          <app-loading-skeleton *ngIf="state === 'loading'" [rows]="5" variant="line"></app-loading-skeleton>
          <app-empty-state *ngIf="state === 'empty'" title="Không có hồ sơ" description="Chưa có hồ sơ EndUser phù hợp với bộ lọc hiện tại."></app-empty-state>

          <div *ngIf="state === 'success'" class="table-wrap">
            <table mat-table [dataSource]="profiles" class="data-table">
              <ng-container matColumnDef="externalUserId"><th mat-header-cell *matHeaderCellDef>Định danh nguồn</th><td mat-cell *matCellDef="let p" class="mono">{{ p.externalUserId }}</td></ng-container>
              <ng-container matColumnDef="displayName"><th mat-header-cell *matHeaderCellDef>Tên hiển thị</th><td mat-cell *matCellDef="let p">{{ p.displayName }}</td></ng-container>
              <ng-container matColumnDef="email"><th mat-header-cell *matHeaderCellDef>Email</th><td mat-cell *matCellDef="let p">{{ p.email || '—' }}</td></ng-container>
              <ng-container matColumnDef="phoneNumber"><th mat-header-cell *matHeaderCellDef>Số điện thoại</th><td mat-cell *matCellDef="let p">{{ p.phoneNumber || '—' }}</td></ng-container>
              <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Trạng thái</th><td mat-cell *matCellDef="let p"><app-status-badge [label]="p.status" [tone]="p.status === 'Active' ? 'success' : 'warning'"></app-status-badge></td></ng-container>
              <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef>Thao tác</th><td mat-cell *matCellDef="let p">
                <div class="actions">
                  <button mat-stroked-button type="button" (click)="openEditDialog(p)">Sửa</button>
                  <button mat-stroked-button type="button" (click)="toggleLock(p)" [disabled]="actionLoadingId === p.id">{{ p.status === 'Locked' ? 'Mở khóa' : 'Khóa' }}</button>
                </div>
              </td></ng-container>
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
    .mono { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 12px; }
    .pagination { display:flex; justify-content:space-between; gap:12px; padding-top:16px; align-items:center; flex-wrap:wrap; color:var(--app-muted); font-size:14px; }
  `]
})
export class EndUserProfilesComponent implements OnInit {
  private readonly service = inject(EndUserProfileService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly toastService = inject(ToastService);

  readonly displayedColumns = ['externalUserId', 'displayName', 'email', 'phoneNumber', 'status', 'actions'];
  state: ViewState = 'loading';
  profiles: EndUserProfile[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  errorMessage = 'Không thể tải danh sách hồ sơ EndUser.';
  actionLoadingId: string | null = null;

  readonly filterForm = this.fb.nonNullable.group({
    status: [''],
    externalUserId: ['']
  });

  get totalPages(): number { return Math.ceil(this.totalCount / this.pageSize); }

  ngOnInit(): void { this.loadProfiles(); }

  loadProfiles(): void {
    this.state = 'loading';
    const { status, externalUserId } = this.filterForm.getRawValue();
    this.service.getEndUserProfiles({
      status: status || undefined,
      externalUserId: externalUserId?.trim() || undefined,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.profiles = response.items;
        this.totalCount = response.totalCount;
        this.page = response.page;
        this.pageSize = response.pageSize;
        this.state = this.profiles.length === 0 ? 'empty' : 'success';
      },
      error: (error: unknown) => {
        this.state = 'error';
        this.errorMessage = this.getErrorMessage(error, 'Không thể tải danh sách hồ sơ EndUser.');
      }
    });
  }

  onApplyFilters(): void { this.page = 1; this.loadProfiles(); }
  clearFilters(): void { this.filterForm.reset({ status: '', externalUserId: '' }); this.page = 1; this.loadProfiles(); }

  openCreateDialog(): void {
    const ref = this.dialog.open(EndUserProfileDialogComponent, { data: { profile: null }, width: '640px', maxWidth: '95vw' });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.service.create(result).subscribe({
        next: () => { this.toastService.success('Tạo hồ sơ EndUser thành công.'); this.loadProfiles(); },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu hồ sơ EndUser.'))
      });
    });
  }

  openEditDialog(profile: EndUserProfile): void {
    const ref = this.dialog.open(EndUserProfileDialogComponent, { data: { profile }, width: '640px', maxWidth: '95vw' });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.service.update(profile.id, {
        displayName: result.displayName,
        email: result.email,
        phoneNumber: result.phoneNumber
      }).subscribe({
        next: () => { this.toastService.success('Cập nhật hồ sơ EndUser thành công.'); this.loadProfiles(); },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu hồ sơ EndUser.'))
      });
    });
  }

  toggleLock(profile: EndUserProfile): void {
    const isLocked = profile.status === 'Locked';
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: isLocked ? 'Mở khóa hồ sơ EndUser' : 'Khóa hồ sơ EndUser',
        message: isLocked ? `Mở khóa hồ sơ "${profile.displayName}"?` : `Khóa hồ sơ "${profile.displayName}"?`,
        confirmLabel: isLocked ? 'Mở khóa' : 'Khóa'
      }
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.actionLoadingId = profile.id;
      const operation = isLocked ? this.service.unlock(profile.id) : this.service.lock(profile.id);
      operation.pipe(finalize(() => (this.actionLoadingId = null))).subscribe({
        next: () => {
          this.toastService.success(isLocked ? 'Mở khóa hồ sơ EndUser thành công.' : 'Khóa hồ sơ EndUser thành công.');
          this.loadProfiles();
        },
        error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể cập nhật trạng thái hồ sơ EndUser.'))
      });
    });
  }

  prevPage(): void { if (this.page > 1) { this.page -= 1; this.loadProfiles(); } }
  nextPage(): void { if (this.page < this.totalPages) { this.page += 1; this.loadProfiles(); } }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 400 || error.status === 422) return 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.';
      if (error.status === 401) return 'Phiên đăng nhập không hợp lệ hoặc đã hết hạn.';
      if (error.status === 403) return 'Bạn không có quyền thực hiện thao tác này.';
      if (error.status === 409) return 'Định danh nguồn đã tồn tại hoặc dữ liệu bị xung đột.';
      if (error.status === 404) return 'Không tìm thấy dữ liệu.';
    }
    return fallback;
  }
}
