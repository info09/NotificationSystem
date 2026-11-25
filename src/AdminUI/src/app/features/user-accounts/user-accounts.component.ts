import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import {
  CreateIntegrationClientCredentialRequest,
  IntegrationClientCredential,
  UserAccount
} from '../../core/user-accounts/user-account.models';
import { UserAccountService } from '../../core/user-accounts/user-account.service';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-user-accounts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page" aria-labelledby="user-accounts-title">
      <header class="page-header">
        <div>
          <h1 id="user-accounts-title">Tài khoản người dùng</h1>
          <p class="subtitle">Quản lý tài khoản, vai trò và thông tin xác thực IntegrationClient.</p>
        </div>
        <button type="button" class="primary" (click)="startCreate()">Tạo tài khoản</button>
      </header>

      <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()" aria-label="Bộ lọc tài khoản">
        <div class="field">
          <label for="statusFilter">Trạng thái</label>
          <select id="statusFilter" formControlName="status">
            <option value="">Tất cả</option>
            <option value="Active">Active</option>
            <option value="Locked">Locked</option>
            <option value="Deleted">Deleted</option>
          </select>
        </div>
        <div class="field">
          <label for="roleFilter">Vai trò</label>
          <select id="roleFilter" formControlName="role">
            <option value="">Tất cả</option>
            <option value="SystemAdministrator">SystemAdministrator</option>
            <option value="BusinessOperator">BusinessOperator</option>
            <option value="IntegrationClient">IntegrationClient</option>
          </select>
        </div>
        <div class="filter-actions">
          <button type="submit" class="primary">Áp dụng</button>
          <button type="button" class="secondary" (click)="clearFilters()">Xóa bộ lọc</button>
        </div>
      </form>

      <div *ngIf="feedbackMessage" class="status-box" [ngClass]="feedbackType === 'success' ? 'status-success' : 'status-error'" role="status">
        <p>{{ feedbackMessage }}</p>
      </div>

      <div *ngIf="showForm" class="panel">
        <h2>{{ editingId ? 'Cập nhật tài khoản' : 'Tạo tài khoản' }}</h2>
        <form [formGroup]="accountForm" (ngSubmit)="saveAccount()" class="form-grid">
          <div class="field">
            <label for="userName">Tên đăng nhập</label>
            <input id="userName" type="text" formControlName="userName" />
          </div>
          <div class="field">
            <label for="email">Email</label>
            <input id="email" type="email" formControlName="email" />
          </div>
          <div class="field">
            <label for="displayName">Tên hiển thị</label>
            <input id="displayName" type="text" formControlName="displayName" />
          </div>
          <div class="field">
            <label for="role">Vai trò</label>
            <select id="role" formControlName="role">
              <option value="SystemAdministrator">SystemAdministrator</option>
              <option value="BusinessOperator">BusinessOperator</option>
              <option value="IntegrationClient">IntegrationClient</option>
            </select>
          </div>
          <div class="field">
            <label for="password">Mật khẩu {{ editingId ? '(để trống nếu không đổi)' : '' }}</label>
            <input id="password" type="password" formControlName="password" />
          </div>
          <div class="form-actions">
            <button type="submit" class="primary" [disabled]="savingAccount">{{ savingAccount ? 'Đang lưu...' : 'Lưu' }}</button>
            <button type="button" class="secondary" (click)="cancelForm()" [disabled]="savingAccount">Hủy</button>
          </div>
        </form>
      </div>

      <div *ngIf="state === 'error'" class="status-box status-error" role="alert">
        <p>{{ errorMessage }}</p>
        <button type="button" class="primary" (click)="loadAccounts()">Thử lại</button>
      </div>

      <div *ngIf="state === 'loading'" class="table-wrap" aria-busy="true">
        <div class="skeleton" *ngFor="let row of skeletonRows"></div>
      </div>

      <div *ngIf="state === 'empty'" class="status-box status-empty">
        <h2>Không có tài khoản</h2>
        <p>Chưa có tài khoản phù hợp với bộ lọc hiện tại.</p>
      </div>

      <div *ngIf="state === 'success'" class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>Tên đăng nhập</th>
              <th>Email</th>
              <th>Tên hiển thị</th>
              <th>Vai trò</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let account of accounts; trackBy: trackById">
              <td>{{ account.userName }}</td>
              <td>{{ account.email }}</td>
              <td>{{ account.displayName }}</td>
              <td>{{ account.role }}</td>
              <td><span class="badge" [ngClass]="statusClass(account.status)">{{ account.status }}</span></td>
              <td class="actions">
                <button type="button" class="secondary" (click)="startEdit(account)">Sửa</button>
                <button type="button" class="secondary" (click)="toggleLock(account)" [disabled]="actionLoadingId === account.id">
                  {{ account.status === 'Locked' ? 'Mở khóa' : 'Khóa' }}
                </button>
                <button type="button" class="danger" (click)="deleteAccount(account)" [disabled]="actionLoadingId === account.id">Xóa</button>
                <button *ngIf="account.role === 'IntegrationClient'" type="button" class="secondary" (click)="openCredentials(account)">Credentials</button>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="pagination">
          <div>Tổng: {{ totalCount }} · Trang {{ page }} / {{ totalPages || 1 }}</div>
          <div class="pagination-actions">
            <button type="button" class="secondary" (click)="prevPage()" [disabled]="page <= 1">Trước</button>
            <button type="button" class="secondary" (click)="nextPage()" [disabled]="page >= totalPages">Sau</button>
          </div>
        </div>
      </div>

      <div *ngIf="selectedCredentialAccount" class="panel">
        <div class="panel-header">
          <h2>Client Credentials - {{ selectedCredentialAccount.userName }}</h2>
          <button type="button" class="secondary" (click)="closeCredentials()">Đóng</button>
        </div>

        <form [formGroup]="credentialForm" (ngSubmit)="createCredential()" class="form-grid">
          <div class="field">
            <label for="clientId">Client ID (tùy chọn)</label>
            <input id="clientId" type="text" formControlName="clientId" />
          </div>
          <div class="field">
            <label for="expiresAtUtc">Hết hạn (UTC, tùy chọn)</label>
            <input id="expiresAtUtc" type="datetime-local" formControlName="expiresAtUtc" />
          </div>
          <div class="form-actions">
            <button type="submit" class="primary" [disabled]="credentialLoading">{{ credentialLoading ? 'Đang tạo...' : 'Tạo credential' }}</button>
          </div>
        </form>

        <div *ngIf="revealedSecret" class="status-box status-success" role="status">
          <p><strong>Client Secret (chỉ hiển thị một lần):</strong></p>
          <code class="secret">{{ revealedSecret }}</code>
        </div>

        <div *ngIf="credentialsLoading" class="status-box">Đang tải credentials...</div>
        <div *ngIf="!credentialsLoading && credentials.length === 0" class="status-box status-empty">Chưa có credential.</div>

        <table *ngIf="!credentialsLoading && credentials.length > 0" class="data-table">
          <thead>
            <tr>
              <th>Client ID</th>
              <th>Trạng thái</th>
              <th>Hết hạn</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let credential of credentials; trackBy: trackByCredentialId">
              <td>{{ credential.clientId }}</td>
              <td>{{ credential.status }}</td>
              <td>{{ credential.expiresAtUtc ? formatDate(credential.expiresAtUtc) : '—' }}</td>
              <td class="actions">
                <button type="button" class="secondary" (click)="rotateCredential(credential)" [disabled]="credentialLoading || credential.status !== 'Active'">Rotate</button>
                <button type="button" class="danger" (click)="revokeCredential(credential)" [disabled]="credentialLoading || credential.status !== 'Active'">Revoke</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  `,
  styles: [`
    .page { padding: 24px; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; color: #0f172a; }
    .page-header { display:flex; justify-content:space-between; gap:16px; align-items:flex-start; margin-bottom:16px; }
    h1 { margin:0; font-size:24px; } h2 { margin:0 0 12px; font-size:18px; }
    .subtitle { margin:4px 0 0; color:#475569; }
    .filters, .panel { background:#fff; border:1px solid #cbd5e1; border-radius:8px; padding:16px; margin-bottom:16px; }
    .filters { display:flex; gap:12px; flex-wrap:wrap; align-items:end; }
    .form-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(220px,1fr)); gap:12px; }
    .field { display:flex; flex-direction:column; gap:6px; }
    label { font-size:13px; color:#334155; font-weight:600; }
    input, select { border:1px solid #cbd5e1; border-radius:6px; padding:8px 10px; font-size:14px; }
    .filter-actions, .form-actions, .pagination-actions, .actions { display:flex; gap:8px; flex-wrap:wrap; }
    .form-actions { align-items:end; }
    .primary, .secondary, .danger { border-radius:6px; padding:8px 12px; font-size:14px; cursor:pointer; border:1px solid #cbd5e1; }
    .primary { background:#2563eb; color:#fff; border-color:#2563eb; }
    .secondary { background:#fff; color:#0f172a; }
    .danger { background:#fff; color:#b91c1c; border-color:#fecaca; }
    button:disabled { opacity:.6; cursor:not-allowed; }
    .status-box { padding:16px; border-radius:8px; margin-bottom:16px; border:1px solid #cbd5e1; background:#fff; }
    .status-error { border-color:#fecaca; background:#fef2f2; color:#991b1b; }
    .status-empty { color:#475569; }
    .status-success { border-color:#bbf7d0; background:#f0fdf4; color:#166534; }
    .table-wrap { background:#fff; border:1px solid #cbd5e1; border-radius:8px; overflow:auto; }
    .data-table { width:100%; border-collapse:collapse; }
    .data-table th, .data-table td { padding:12px; border-bottom:1px solid #e2e8f0; text-align:left; font-size:14px; vertical-align:top; }
    .data-table th { background:#f8fafc; color:#334155; }
    .badge { display:inline-block; padding:2px 8px; border-radius:999px; font-size:12px; font-weight:600; }
    .badge-active { background:#dcfce7; color:#166534; } .badge-locked { background:#fef3c7; color:#92400e; } .badge-deleted { background:#fee2e2; color:#991b1b; }
    .pagination { display:flex; justify-content:space-between; gap:12px; padding:12px; align-items:center; }
    .skeleton { height:48px; border-bottom:1px solid #e2e8f0; background:linear-gradient(90deg,#f8fafc,#f1f5f9,#f8fafc); }
    .panel-header { display:flex; justify-content:space-between; gap:12px; align-items:center; margin-bottom:12px; }
    .secret { display:block; padding:8px; background:#fff; border:1px solid #bbf7d0; border-radius:4px; word-break:break-all; }
  `]
})
export class UserAccountsComponent implements OnInit {
  private readonly userAccountService = inject(UserAccountService);
  private readonly fb = inject(FormBuilder);

  state: ViewState = 'loading';
  accounts: UserAccount[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  errorMessage = 'Không thể tải danh sách tài khoản.';
  feedbackMessage = '';
  feedbackType: 'success' | 'error' = 'success';
  actionLoadingId: string | null = null;

  showForm = false;
  editingId: string | null = null;
  savingAccount = false;

  selectedCredentialAccount: UserAccount | null = null;
  credentials: IntegrationClientCredential[] = [];
  credentialsLoading = false;
  credentialLoading = false;
  revealedSecret = '';

  readonly skeletonRows = Array.from({ length: 5 });

  readonly filterForm = this.fb.nonNullable.group({
    status: [''],
    role: ['']
  });

  readonly accountForm = this.fb.nonNullable.group({
    userName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    displayName: ['', [Validators.required]],
    role: ['BusinessOperator', [Validators.required]],
    password: ['']
  });

  readonly credentialForm = this.fb.group({
    clientId: [''],
    expiresAtUtc: ['']
  });

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.state = 'loading';
    this.errorMessage = 'Không thể tải danh sách tài khoản.';
    const { status, role } = this.filterForm.getRawValue();

    this.userAccountService.getUserAccounts({
      status: status || undefined,
      role: role || undefined,
      page: this.page,
      pageSize: this.pageSize
    }).pipe(finalize(() => {})).subscribe({
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

  onApplyFilters(): void {
    this.page = 1;
    this.loadAccounts();
  }

  clearFilters(): void {
    this.filterForm.reset({ status: '', role: '' });
    this.page = 1;
    this.loadAccounts();
  }

  startCreate(): void {
    this.showForm = true;
    this.editingId = null;
    this.accountForm.reset({ userName: '', email: '', displayName: '', role: 'BusinessOperator', password: '' });
  }

  startEdit(account: UserAccount): void {
    this.showForm = true;
    this.editingId = account.id;
    this.accountForm.reset({
      userName: account.userName,
      email: account.email,
      displayName: account.displayName,
      role: account.role,
      password: ''
    });
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.accountForm.reset({ userName: '', email: '', displayName: '', role: 'BusinessOperator', password: '' });
  }

  saveAccount(): void {
    if (this.accountForm.invalid) {
      this.accountForm.markAllAsTouched();
      return;
    }

    const value = this.accountForm.getRawValue();
    const password = value.password?.trim() ? value.password : null;
    this.savingAccount = true;
    this.feedbackMessage = '';

    const request = {
      userName: value.userName.trim(),
      email: value.email.trim(),
      displayName: value.displayName.trim(),
      role: value.role,
      password
    };

    const operation = this.editingId
      ? this.userAccountService.updateUserAccount(this.editingId, request)
      : this.userAccountService.createUserAccount(request);

    operation.pipe(finalize(() => (this.savingAccount = false))).subscribe({
      next: () => {
        this.feedbackType = 'success';
        this.feedbackMessage = this.editingId ? 'Cập nhật tài khoản thành công.' : 'Tạo tài khoản thành công.';
        this.cancelForm();
        this.loadAccounts();
      },
      error: (error: unknown) => {
        this.feedbackType = 'error';
        this.feedbackMessage = this.getErrorMessage(error, 'Không thể lưu tài khoản.');
      }
    });
  }

  toggleLock(account: UserAccount): void {
    this.actionLoadingId = account.id;
    this.feedbackMessage = '';
    const operation = account.status === 'Locked'
      ? this.userAccountService.unlockUserAccount(account.id)
      : this.userAccountService.lockUserAccount(account.id);

    operation.pipe(finalize(() => (this.actionLoadingId = null))).subscribe({
      next: () => {
        this.feedbackType = 'success';
        this.feedbackMessage = account.status === 'Locked' ? 'Mở khóa tài khoản thành công.' : 'Khóa tài khoản thành công.';
        this.loadAccounts();
      },
      error: (error: unknown) => {
        this.feedbackType = 'error';
        this.feedbackMessage = this.getErrorMessage(error, 'Không thể cập nhật trạng thái tài khoản.');
      }
    });
  }

  deleteAccount(account: UserAccount): void {
    if (!confirm(`Xóa tài khoản "${account.userName}"?`)) {
      return;
    }

    this.actionLoadingId = account.id;
    this.feedbackMessage = '';
    this.userAccountService.deleteUserAccount(account.id)
      .pipe(finalize(() => (this.actionLoadingId = null)))
      .subscribe({
        next: () => {
          this.feedbackType = 'success';
          this.feedbackMessage = 'Xóa tài khoản thành công.';
          this.loadAccounts();
          if (this.selectedCredentialAccount?.id === account.id) {
            this.closeCredentials();
          }
        },
        error: (error: unknown) => {
          this.feedbackType = 'error';
          this.feedbackMessage = this.getErrorMessage(error, 'Không thể xóa tài khoản.');
        }
      });
  }

  openCredentials(account: UserAccount): void {
    this.selectedCredentialAccount = account;
    this.revealedSecret = '';
    this.credentialForm.reset({ clientId: '', expiresAtUtc: '' });
    this.loadCredentials(account.id);
  }

  closeCredentials(): void {
    this.selectedCredentialAccount = null;
    this.credentials = [];
    this.revealedSecret = '';
  }

  loadCredentials(userId: string): void {
    this.credentialsLoading = true;
    this.userAccountService.getCredentials(userId)
      .pipe(finalize(() => (this.credentialsLoading = false)))
      .subscribe({
        next: (credentials) => (this.credentials = credentials),
        error: (error: unknown) => {
          this.feedbackType = 'error';
          this.feedbackMessage = this.getErrorMessage(error, 'Không thể tải credentials.');
        }
      });
  }

  createCredential(): void {
    if (!this.selectedCredentialAccount) {
      return;
    }

    this.credentialLoading = true;
    this.revealedSecret = '';
    const raw = this.credentialForm.getRawValue();
    const request: CreateIntegrationClientCredentialRequest = {
      clientId: raw.clientId?.trim() || null,
      expiresAtUtc: raw.expiresAtUtc ? new Date(raw.expiresAtUtc).toISOString() : null
    };

    this.userAccountService.createCredential(this.selectedCredentialAccount.id, request)
      .pipe(finalize(() => (this.credentialLoading = false)))
      .subscribe({
        next: (response) => {
          this.revealedSecret = response.clientSecret;
          this.feedbackType = 'success';
          this.feedbackMessage = 'Tạo credential thành công.';
          this.credentialForm.reset({ clientId: '', expiresAtUtc: '' });
          this.loadCredentials(this.selectedCredentialAccount!.id);
        },
        error: (error: unknown) => {
          this.feedbackType = 'error';
          this.feedbackMessage = this.getErrorMessage(error, 'Không thể tạo credential.');
        }
      });
  }

  rotateCredential(credential: IntegrationClientCredential): void {
    if (!this.selectedCredentialAccount) {
      return;
    }
    this.credentialLoading = true;
    this.revealedSecret = '';
    this.userAccountService.rotateCredential(this.selectedCredentialAccount.id, credential.id)
      .pipe(finalize(() => (this.credentialLoading = false)))
      .subscribe({
        next: (response) => {
          this.revealedSecret = response.clientSecret;
          this.feedbackType = 'success';
          this.feedbackMessage = 'Rotate credential thành công.';
          this.loadCredentials(this.selectedCredentialAccount!.id);
        },
        error: (error: unknown) => {
          this.feedbackType = 'error';
          this.feedbackMessage = this.getErrorMessage(error, 'Không thể rotate credential.');
        }
      });
  }

  revokeCredential(credential: IntegrationClientCredential): void {
    if (!this.selectedCredentialAccount) {
      return;
    }
    this.credentialLoading = true;
    this.userAccountService.revokeCredential(this.selectedCredentialAccount.id, credential.id)
      .pipe(finalize(() => (this.credentialLoading = false)))
      .subscribe({
        next: () => {
          this.feedbackType = 'success';
          this.feedbackMessage = 'Revoke credential thành công.';
          this.loadCredentials(this.selectedCredentialAccount!.id);
        },
        error: (error: unknown) => {
          this.feedbackType = 'error';
          this.feedbackMessage = this.getErrorMessage(error, 'Không thể revoke credential.');
        }
      });
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadAccounts();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page += 1;
      this.loadAccounts();
    }
  }

  trackById(_: number, account: UserAccount): string {
    return account.id;
  }

  trackByCredentialId(_: number, credential: IntegrationClientCredential): string {
    return credential.id;
  }

  statusClass(status: string): string {
    const normalized = status.toLowerCase();
    if (normalized === 'active') return 'badge-active';
    if (normalized === 'locked') return 'badge-locked';
    return 'badge-deleted';
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleString();
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
