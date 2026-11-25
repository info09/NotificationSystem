import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { EndUserProfile } from '../../core/end-user-profiles/end-user-profile.models';
import { EndUserProfileService } from '../../core/end-user-profiles/end-user-profile.service';

type ViewState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-end-user-profiles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page" aria-labelledby="end-user-profiles-title">
      <header class="page-header">
        <div>
          <h1 id="end-user-profiles-title">Hồ sơ người nhận EndUser</h1>
          <p class="subtitle">Quản lý hồ sơ EndUser theo định danh hệ thống nguồn.</p>
        </div>
        <button type="button" class="primary" (click)="startCreate()">Tạo hồ sơ</button>
      </header>

      <form class="filters" [formGroup]="filterForm" (ngSubmit)="onApplyFilters()" aria-label="Bộ lọc hồ sơ EndUser">
        <div class="field">
          <label for="statusFilter">Trạng thái</label>
          <select id="statusFilter" formControlName="status">
            <option value="">Tất cả</option>
            <option value="Active">Active</option>
            <option value="Locked">Locked</option>
          </select>
        </div>
        <div class="field">
          <label for="externalUserIdFilter">Định danh nguồn</label>
          <input id="externalUserIdFilter" type="text" formControlName="externalUserId" />
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
        <h2>{{ editingId ? 'Cập nhật hồ sơ EndUser' : 'Tạo hồ sơ EndUser' }}</h2>
        <form [formGroup]="profileForm" (ngSubmit)="saveProfile()" class="form-grid">
          <div class="field">
            <label for="externalUserId">Định danh nguồn</label>
            <input id="externalUserId" type="text" formControlName="externalUserId" [readonly]="!!editingId" />
          </div>
          <div class="field">
            <label for="displayName">Tên hiển thị</label>
            <input id="displayName" type="text" formControlName="displayName" />
          </div>
          <div class="field">
            <label for="email">Email</label>
            <input id="email" type="email" formControlName="email" />
          </div>
          <div class="field">
            <label for="phoneNumber">Số điện thoại</label>
            <input id="phoneNumber" type="text" formControlName="phoneNumber" />
          </div>
          <div class="form-actions">
            <button type="submit" class="primary" [disabled]="saving">{{ saving ? 'Đang lưu...' : 'Lưu' }}</button>
            <button type="button" class="secondary" (click)="cancelForm()" [disabled]="saving">Hủy</button>
          </div>
        </form>
      </div>

      <div *ngIf="state === 'error'" class="status-box status-error" role="alert">
        <p>{{ errorMessage }}</p>
        <button type="button" class="primary" (click)="loadProfiles()">Thử lại</button>
      </div>

      <div *ngIf="state === 'loading'" class="table-wrap" aria-busy="true">
        <div class="skeleton" *ngFor="let row of skeletonRows"></div>
      </div>

      <div *ngIf="state === 'empty'" class="status-box status-empty">
        <h2>Không có hồ sơ</h2>
        <p>Chưa có hồ sơ EndUser phù hợp với bộ lọc hiện tại.</p>
      </div>

      <div *ngIf="state === 'success'" class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>Định danh nguồn</th>
              <th>Tên hiển thị</th>
              <th>Email</th>
              <th>Số điện thoại</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let profile of profiles; trackBy: trackById">
              <td class="mono">{{ profile.externalUserId }}</td>
              <td>{{ profile.displayName }}</td>
              <td>{{ profile.email || '—' }}</td>
              <td>{{ profile.phoneNumber || '—' }}</td>
              <td><span class="badge" [ngClass]="statusClass(profile.status)">{{ profile.status }}</span></td>
              <td class="actions">
                <button type="button" class="secondary" (click)="startEdit(profile)">Sửa</button>
                <button type="button" class="secondary" (click)="toggleLock(profile)" [disabled]="actionLoadingId === profile.id">
                  {{ profile.status === 'Locked' ? 'Mở khóa' : 'Khóa' }}
                </button>
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
    input[readonly] { background:#f8fafc; color:#475569; }
    .filter-actions, .form-actions, .pagination-actions, .actions { display:flex; gap:8px; flex-wrap:wrap; }
    .form-actions { align-items:end; }
    .primary, .secondary { border-radius:6px; padding:8px 12px; font-size:14px; cursor:pointer; border:1px solid #cbd5e1; }
    .primary { background:#2563eb; color:#fff; border-color:#2563eb; }
    .secondary { background:#fff; color:#0f172a; }
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
    .badge-active { background:#dcfce7; color:#166534; } .badge-locked { background:#fef3c7; color:#92400e; }
    .pagination { display:flex; justify-content:space-between; gap:12px; padding:12px; align-items:center; }
    .skeleton { height:48px; border-bottom:1px solid #e2e8f0; background:linear-gradient(90deg,#f8fafc,#f1f5f9,#f8fafc); }
    .mono { font-family: Consolas, "Courier New", monospace; font-size: 13px; }
  `]
})
export class EndUserProfilesComponent implements OnInit {
  private readonly service = inject(EndUserProfileService);
  private readonly fb = inject(FormBuilder);

  state: ViewState = 'loading';
  profiles: EndUserProfile[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  errorMessage = 'Không thể tải danh sách hồ sơ EndUser.';
  feedbackMessage = '';
  feedbackType: 'success' | 'error' = 'success';
  actionLoadingId: string | null = null;
  showForm = false;
  editingId: string | null = null;
  saving = false;
  readonly skeletonRows = Array.from({ length: 5 });

  readonly filterForm = this.fb.nonNullable.group({
    status: [''],
    externalUserId: ['']
  });

  readonly profileForm = this.fb.nonNullable.group({
    externalUserId: ['', [Validators.required]],
    displayName: ['', [Validators.required]],
    email: [''],
    phoneNumber: ['']
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

  startCreate(): void {
    this.showForm = true;
    this.editingId = null;
    this.profileForm.reset({ externalUserId: '', displayName: '', email: '', phoneNumber: '' });
    this.profileForm.controls.externalUserId.enable();
  }

  startEdit(profile: EndUserProfile): void {
    this.showForm = true;
    this.editingId = profile.id;
    this.profileForm.reset({
      externalUserId: profile.externalUserId,
      displayName: profile.displayName,
      email: profile.email ?? '',
      phoneNumber: profile.phoneNumber ?? ''
    });
    this.profileForm.controls.externalUserId.disable();
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.profileForm.reset({ externalUserId: '', displayName: '', email: '', phoneNumber: '' });
    this.profileForm.controls.externalUserId.enable();
  }

  saveProfile(): void {
    if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
    const value = this.profileForm.getRawValue();
    this.saving = true;
    this.feedbackMessage = '';

    const operation = this.editingId
      ? this.service.update(this.editingId, {
          displayName: value.displayName.trim(),
          email: value.email?.trim() || null,
          phoneNumber: value.phoneNumber?.trim() || null
        })
      : this.service.create({
          externalUserId: value.externalUserId.trim(),
          displayName: value.displayName.trim(),
          email: value.email?.trim() || null,
          phoneNumber: value.phoneNumber?.trim() || null
        });

    operation.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.feedbackType = 'success';
        this.feedbackMessage = this.editingId ? 'Cập nhật hồ sơ EndUser thành công.' : 'Tạo hồ sơ EndUser thành công.';
        this.cancelForm();
        this.loadProfiles();
      },
      error: (error: unknown) => {
        this.feedbackType = 'error';
        this.feedbackMessage = this.getErrorMessage(error, 'Không thể lưu hồ sơ EndUser.');
      }
    });
  }

  toggleLock(profile: EndUserProfile): void {
    this.actionLoadingId = profile.id;
    this.feedbackMessage = '';
    const operation = profile.status === 'Locked' ? this.service.unlock(profile.id) : this.service.lock(profile.id);
    operation.pipe(finalize(() => (this.actionLoadingId = null))).subscribe({
      next: () => {
        this.feedbackType = 'success';
        this.feedbackMessage = profile.status === 'Locked' ? 'Mở khóa hồ sơ EndUser thành công.' : 'Khóa hồ sơ EndUser thành công.';
        this.loadProfiles();
      },
      error: (error: unknown) => {
        this.feedbackType = 'error';
        this.feedbackMessage = this.getErrorMessage(error, 'Không thể cập nhật trạng thái hồ sơ EndUser.');
      }
    });
  }

  prevPage(): void { if (this.page > 1) { this.page -= 1; this.loadProfiles(); } }
  nextPage(): void { if (this.page < this.totalPages) { this.page += 1; this.loadProfiles(); } }
  trackById(_: number, profile: EndUserProfile): string { return profile.id; }
  statusClass(status: string): string { return status.toLowerCase() === 'active' ? 'badge-active' : 'badge-locked'; }

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
