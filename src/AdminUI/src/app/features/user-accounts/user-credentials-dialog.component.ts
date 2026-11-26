import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import {
  CreateIntegrationClientCredentialRequest,
  IntegrationClientCredential,
  UserAccount
} from '../../core/user-accounts/user-account.models';
import { UserAccountService } from '../../core/user-accounts/user-account.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { ToastService } from '../../shared/ui/toast/toast.service';

export interface UserCredentialsDialogData {
  account: UserAccount;
}

@Component({
  selector: 'app-user-credentials-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatTableModule, EmptyStateComponent, LoadingSkeletonComponent, StatusBadgeComponent],
  template: `
    <h2 mat-dialog-title>Client Credentials - {{ data.account.userName }}</h2>
    <mat-dialog-content class="dialog-content">
      <p class="subtitle">Quản lý client credentials cho IntegrationClient.</p>

      <form [formGroup]="credentialForm" (ngSubmit)="createCredential()" class="credential-form">
        <mat-form-field appearance="outline">
          <mat-label>Client ID (tùy chọn)</mat-label>
          <input matInput formControlName="clientId" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Hết hạn (UTC, tùy chọn)</mat-label>
          <input matInput type="datetime-local" formControlName="expiresAtUtc" />
        </mat-form-field>
        <div class="form-actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="credentialLoading">
            {{ credentialLoading ? 'Đang tạo...' : 'Tạo credential' }}
          </button>
        </div>
      </form>

      <div *ngIf="revealedSecret" class="secret-box" role="status">
        <p><strong>Client Secret (chỉ hiển thị một lần):</strong></p>
        <code>{{ revealedSecret }}</code>
      </div>

      <app-loading-skeleton *ngIf="credentialsLoading" [rows]="3" variant="line"></app-loading-skeleton>
      <app-empty-state *ngIf="!credentialsLoading && credentials.length === 0" title="Chưa có credential" description="Tài khoản này chưa có client credential."></app-empty-state>

      <div *ngIf="!credentialsLoading && credentials.length > 0" class="table-wrap">
        <table mat-table [dataSource]="credentials" class="data-table">
          <ng-container matColumnDef="clientId">
            <th mat-header-cell *matHeaderCellDef>Client ID</th>
            <td mat-cell *matCellDef="let c">{{ c.clientId }}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Trạng thái</th>
            <td mat-cell *matCellDef="let c"><app-status-badge [label]="c.status" [tone]="c.status === 'Active' ? 'success' : 'warning'"></app-status-badge></td>
          </ng-container>
          <ng-container matColumnDef="expiresAtUtc">
            <th mat-header-cell *matHeaderCellDef>Hết hạn</th>
            <td mat-cell *matCellDef="let c">{{ c.expiresAtUtc ? formatDate(c.expiresAtUtc) : '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Thao tác</th>
            <td mat-cell *matCellDef="let c">
              <div class="actions">
                <button mat-stroked-button type="button" (click)="rotateCredential(c)" [disabled]="credentialLoading || c.status !== 'Active'">Rotate</button>
                <button mat-stroked-button color="warn" type="button" (click)="revokeCredential(c)" [disabled]="credentialLoading || c.status !== 'Active'">Revoke</button>
              </div>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="credentialColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: credentialColumns;"></tr>
        </table>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" mat-dialog-close>Đóng</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-content { min-width: min(760px, 90vw); }
    .subtitle { margin: 0 0 16px; color: var(--app-muted); font-size: 14px; }
    .credential-form { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; align-items: start; margin-bottom: 8px; }
    .form-actions { display: flex; align-items: center; padding-top: 8px; }
    .secret-box { margin: 16px 0; padding: 14px; border-radius: 10px; background: #f0fdf4; border: 1px solid #bbf7d0; color: #166534; }
    .secret-box code { display:block; margin-top:8px; padding:10px; background:#fff; border:1px solid #bbf7d0; border-radius:8px; word-break:break-all; color:#0f172a; }
    .table-wrap { overflow-x: auto; }
    .data-table { width: 100%; }
    .actions { display: flex; gap: 8px; flex-wrap: wrap; }
    @media (max-width: 768px) { .dialog-content { min-width: auto; } }
  `]
})
export class UserCredentialsDialogComponent implements OnInit {
  private readonly userAccountService = inject(UserAccountService);
  private readonly fb = inject(FormBuilder);
  private readonly toastService = inject(ToastService);
  readonly dialogRef = inject(MatDialogRef<UserCredentialsDialogComponent>);
  readonly data = inject<UserCredentialsDialogData>(MAT_DIALOG_DATA);

  readonly credentialColumns = ['clientId', 'status', 'expiresAtUtc', 'actions'];
  credentials: IntegrationClientCredential[] = [];
  credentialsLoading = false;
  credentialLoading = false;
  revealedSecret = '';

  readonly credentialForm = this.fb.group({
    clientId: [''],
    expiresAtUtc: ['']
  });

  ngOnInit(): void {
    this.loadCredentials();
  }

  loadCredentials(): void {
    this.credentialsLoading = true;
    this.userAccountService.getCredentials(this.data.account.id)
      .pipe(finalize(() => (this.credentialsLoading = false)))
      .subscribe({
        next: (credentials) => (this.credentials = credentials),
        error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể tải credentials.'))
      });
  }

  createCredential(): void {
    this.credentialLoading = true;
    this.revealedSecret = '';
    const raw = this.credentialForm.getRawValue();
    const request: CreateIntegrationClientCredentialRequest = {
      clientId: raw.clientId?.trim() || null,
      expiresAtUtc: raw.expiresAtUtc ? new Date(raw.expiresAtUtc).toISOString() : null
    };

    this.userAccountService.createCredential(this.data.account.id, request)
      .pipe(finalize(() => (this.credentialLoading = false)))
      .subscribe({
        next: (response) => {
          this.revealedSecret = response.clientSecret;
          this.toastService.success('Tạo credential thành công.');
          this.credentialForm.reset({ clientId: '', expiresAtUtc: '' });
          this.loadCredentials();
        },
        error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể tạo credential.'))
      });
  }

  rotateCredential(credential: IntegrationClientCredential): void {
    this.credentialLoading = true;
    this.revealedSecret = '';
    this.userAccountService.rotateCredential(this.data.account.id, credential.id)
      .pipe(finalize(() => (this.credentialLoading = false)))
      .subscribe({
        next: (response) => {
          this.revealedSecret = response.clientSecret;
          this.toastService.success('Rotate credential thành công.');
          this.loadCredentials();
        },
        error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể rotate credential.'))
      });
  }

  revokeCredential(credential: IntegrationClientCredential): void {
    this.credentialLoading = true;
    this.userAccountService.revokeCredential(this.data.account.id, credential.id)
      .pipe(finalize(() => (this.credentialLoading = false)))
      .subscribe({
        next: () => {
          this.toastService.success('Revoke credential thành công.');
          this.loadCredentials();
        },
        error: (error: unknown) => this.toastService.error(this.getErrorMessage(error, 'Không thể revoke credential.'))
      });
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
