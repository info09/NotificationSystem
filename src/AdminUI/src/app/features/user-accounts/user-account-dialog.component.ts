import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UserAccount } from '../../core/user-accounts/user-account.models';

export interface UserAccountDialogData {
  account: UserAccount | null;
}

@Component({
  selector: 'app-user-account-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.account ? 'Cập nhật tài khoản' : 'Tạo tài khoản' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form" id="user-account-form" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Tên đăng nhập</mat-label>
          <input matInput formControlName="userName" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Tên hiển thị</mat-label>
          <input matInput formControlName="displayName" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Vai trò</mat-label>
          <mat-select formControlName="role">
            <mat-option value="SystemAdministrator">SystemAdministrator</mat-option>
            <mat-option value="BusinessOperator">BusinessOperator</mat-option>
            <mat-option value="IntegrationClient">IntegrationClient</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>{{ data.account ? 'Mật khẩu (để trống nếu không đổi)' : 'Mật khẩu' }}</mat-label>
          <input matInput type="password" formControlName="password" />
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close()">Hủy</button>
      <button mat-flat-button color="primary" type="submit" form="user-account-form">Lưu</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-form { display: grid; gap: 4px; padding-top: 8px; min-width: min(560px, 80vw); }
    @media (max-width: 768px) { .dialog-form { min-width: auto; } }
  `]
})
export class UserAccountDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly dialogRef = inject(MatDialogRef<UserAccountDialogComponent>);
  readonly data = inject<UserAccountDialogData>(MAT_DIALOG_DATA);

  readonly form = this.fb.nonNullable.group({
    userName: [this.data.account?.userName ?? '', [Validators.required]],
    email: [this.data.account?.email ?? '', [Validators.required, Validators.email]],
    displayName: [this.data.account?.displayName ?? '', [Validators.required]],
    role: [this.data.account?.role ?? 'BusinessOperator', [Validators.required]],
    password: ['']
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      userName: value.userName.trim(),
      email: value.email.trim(),
      displayName: value.displayName.trim(),
      role: value.role,
      password: value.password?.trim() ? value.password : null
    });
  }
}
