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
import { UserAccount } from '../../core/user-accounts/user-account.models';
import { UserAccountService } from '../../core/user-accounts/user-account.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../shared/ui/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { ToastService } from '../../shared/ui/toast/toast.service';
import { UserAccountDialogComponent } from './user-account-dialog.component';
import { UserCredentialsDialogComponent } from './user-credentials-dialog.component';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-user-accounts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, MatFormFieldModule, MatSelectModule, MatTableModule, MatDialogModule, EmptyStateComponent, ErrorStateComponent, LoadingSkeletonComponent, StatusBadgeComponent],
  template: `
    <section class="page-container" aria-labelledby="user-accounts-title">
      <header class="page-header">
        <div>
          <p class="eyebrow">Quản trị</p>
          <h1 id="user-accounts-title" class="page-title">Tài khoản người dùng</h1>
          <p class="page-subtitle">Quản lý tài khoản, vai trò và thông tin xác thực IntegrationClient.</p>
        </div>
        <button mat-flat-button color="primary" type="button" (click)="openCreateDialog()">Tạo tài khoản</button>
      </header>

      <mat-card class="panel-card filter-card">
        <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()">
          <mat-form-field appearance="outline">
            <mat-label>Trạng thái</mat-label>
            <mat-select formControlName="status">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="Active">Active</mat-option>
              <mat-option value="Locked">Locked</mat-option>
              <mat-option value="Deleted">Deleted</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Vai trò</mat-label>
            <mat-select formControlName="role">
              <mat-option value="">Tất cả</mat-option>
              <mat-option value="SystemAdministrator">SystemAdministrator</mat-option>
              <mat-option value="BusinessOperator">BusinessOperator</mat-option>
              <mat-option value="IntegrationClient">IntegrationClient</mat-option>
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
          <app-error-state *ngIf="state === 'error'" [message]="errorMessage" (retry)="loadAccounts()"></app-error-state>
          <app-loading-skeleton *ngIf="state === 'loading'" [rows]="5" variant="line"></app-loading-skeleton>
          <app-empty-state *ngIf="state === 'empty'" title="Không có tài khoản" description="Chưa có tài khoản phù hợp với bộ lọc hiện tại."></app-empty-state>

          <div *ngIf="state === 'success'" class="table-wrap">
            <table mat-table [dataSource]="accounts" class="data-table">
              <ng-container matColumnDef="userName"><th mat-header-cell *matHeaderCellDef>Tên đăng nhập</th><td mat-cell *matCellDef="let a">{{ a.userName }}</td></ng-container>
              <ng-container matColumnDef="email"><th mat-header-cell *matHeaderCellDef>Email</th><td mat-cell *matCellDef="let a">{{ a.email }}</td></ng-container>
              <ng-container matColumnDef="displayName"><th mat-header-cell *matHeaderCellDef>Tên hiển thị</th><td mat-cell *matCellDef="let a">{{ a.displayName }}</td></ng-container>
              <ng-container matColumnDef="role"><th mat-header-cell *matHeaderCellDef>Vai trò</th><td mat-cell *matCellDef="let a">{{ a.role }}</td></ng-container>
              <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Trạng thái</th><td mat-cell *matCellDef="let a"><app-status-badge [label]="a.status" [tone]="statusTone(a.status)"></app-status-badge></td></ng-container>
              <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef>Thao tác</th><td mat-cell *matCellDef="let a">
                <div class="actions">
                  <button mat-stroked-button type="button" (click)="openEditDialog(a)">Sửa</button>
                  <button mat-stroked-button type="button" (click)="toggleLock(a)" [disabled]="actionLoadingId === a.id">{{ a.status === 'Locked' ? 'Mở khóa' : 'Khóa' }}</button>
                  <button mat-stroked-button color="warn" type="button" (click)="deleteAccount(a)" [disabled]="actionLoadingId === a.id">Xóa</button>
                  <button *ngIf="a.role === 'IntegrationClient'" mat-stroked-button type="button" (click)="openCredentials(a)">Credentials</button>
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
    .pagination { display:flex; justify-content:space-between; gap:12px; padding-top:16px; align-items:center; flex-wrap:wrap; color:var(--app-muted); font-size:14px; }
  `]
})
export class UserAccountsComponent implements OnInit {
  private readonly userAccountService = inject(UserAccountService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly toastService = inject(ToastService);

  readonly displayedColumns = ['userName', 'email', 'displayName', 'role', 'status', 'actions'];

  state: ViewState = 'loading';
  accounts: UserAccount[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  errorMessage = 'Không thể tải danh sách tài khoản.';
  actionLoadingId: string | null = null;

  readonly filterForm = this.fb.nonNullable.group({
    status: [''],
    role: ['']
  });

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  ngOnInit(): void { this.loadAccounts(); }

  loadAccounts(): void {
    this.state = 'loading';
    this.errorMessage = 'Không thể tải danh sách tài khoản.';
    const { status, role } = this.filterForm.getRawValue();

    this.userAccountService.getUserAccounts({
      status: status || undefined,
      role: role || undefined,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.accounts = response.items;
        this.totalCount = response.totalCount;
        this.page = response.page;
        this.pageSize = response.pageSize;
        this.state = this.accounts.length === 0 ? 'empty' : 'success';
      },
      error: (error: unknown) => {
        this.state = 'error';
        this.errorMessage = this.getErrorMessage(error, 'Không thể tải danh sách tài khoản.');
      }
    });
  }

  onApplyFilters(): void { this.page = 1; this.loadAccounts(); }
  clearFilters(): void { this.filterForm.reset({ status: '', role: '' }); this.page = 1; this.loadAccounts(); }

  openCreateDialog(): void {
    const ref = this.dialog.open(UserAccountDialogComponent, { data: { account: null }, width: '640px', maxWidth: '95vw' });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.userAccountService.createUserAccount(result).subscribe({
        next: () => { this.toastService.success('Tạo tài khoản thành công.'); this.loadAccounts(); },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu tài khoản.'))
      });
    });
  }

  openEditDialog(account: UserAccount): void {
    const ref = this.dialog.open(UserAccountDialogComponent, { data: { account }, width: '640px', maxWidth: '95vw' });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.userAccountService.updateUserAccount(account.id, result).subscribe({
        next: () => { this.toastService.success('Cập nhật tài khoản thành công.'); this.loadAccounts(); },
        error: (error) => this.toastService.error(this.getErrorMessage(error, 'Không thể lưu tài khoản.'))
      });
    });
  }

  toggleLock(account: UserAccount): void {
    this.actionLoadingId = account.id;
    const operation = account.status === 'Locked'
      ? this.userAccountService.unlockUserAccount(account.id)
      : this.userAccountService.lockUserAccount(account.id);

    operation.pipe(finalize(() => (this.actionLoadingId = null))).subscribe({
      next: () => {
        this.toastService.success(account.status === 'Locked' ? 'Mở khóa tài khoản thành công.' : 'Khóa tài khoản thành công.');
        this.loadAccounts();
      },
      error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể cập nhật trạng thái tài khoản.'))
    });
  }

  deleteAccount(account: UserAccount): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Xóa tài khoản', message: `Xóa tài khoản "${account.userName}"?`, confirmLabel: 'Xóa', tone: 'warn' }
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.actionLoadingId = account.id;
      this.userAccountService.deleteUserAccount(account.id)
        .pipe(finalize(() => (this.actionLoadingId = null)))
        .subscribe({
          next: () => {
            this.toastService.success('Xóa tài khoản thành công.');
            this.loadAccounts();
          },
          error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể xóa tài khoản.'))
        });
    });
  }

  openCredentials(account: UserAccount): void {
    this.dialog.open(UserCredentialsDialogComponent, {
      data: { account },
      width: '820px',
      maxWidth: '95vw'
    });
  }

  prevPage(): void { if (this.page > 1) { this.page -= 1; this.loadAccounts(); } }
  nextPage(): void { if (this.page < this.totalPages) { this.page += 1; this.loadAccounts(); } }

  statusTone(status: string): 'success' | 'warning' | 'danger' | 'default' {
    const normalized = status.toLowerCase();
    if (normalized === 'active') return 'success';
    if (normalized === 'locked') return 'warning';
    if (normalized === 'deleted') return 'danger';
    return 'default';
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
