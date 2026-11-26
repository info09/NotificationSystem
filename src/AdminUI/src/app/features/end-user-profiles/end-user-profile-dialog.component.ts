import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { EndUserProfile } from '../../core/end-user-profiles/end-user-profile.models';

export interface EndUserProfileDialogData {
  profile: EndUserProfile | null;
}

@Component({
  selector: 'app-end-user-profile-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.profile ? 'Cập nhật hồ sơ EndUser' : 'Tạo hồ sơ EndUser' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form" id="end-user-profile-form" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Định danh nguồn</mat-label>
          <input matInput formControlName="externalUserId" [readonly]="!!data.profile" />
          <mat-error *ngIf="form.controls.externalUserId.hasError('required')">Vui lòng nhập định danh nguồn.</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Tên hiển thị</mat-label>
          <input matInput formControlName="displayName" />
          <mat-error *ngIf="form.controls.displayName.hasError('required')">Vui lòng nhập tên hiển thị.</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Số điện thoại</mat-label>
          <input matInput formControlName="phoneNumber" />
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close()">Hủy</button>
      <button mat-flat-button color="primary" type="submit" form="end-user-profile-form">Lưu</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-form { display: grid; gap: 4px; padding-top: 8px; min-width: min(560px, 80vw); }
    @media (max-width: 768px) { .dialog-form { min-width: auto; } }
  `]
})
export class EndUserProfileDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly dialogRef = inject(MatDialogRef<EndUserProfileDialogComponent>);
  readonly data = inject<EndUserProfileDialogData>(MAT_DIALOG_DATA);

  readonly form = this.fb.nonNullable.group({
    externalUserId: [this.data.profile?.externalUserId ?? '', [Validators.required]],
    displayName: [this.data.profile?.displayName ?? '', [Validators.required]],
    email: [this.data.profile?.email ?? ''],
    phoneNumber: [this.data.profile?.phoneNumber ?? '']
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      externalUserId: value.externalUserId.trim(),
      displayName: value.displayName.trim(),
      email: value.email?.trim() || null,
      phoneNumber: value.phoneNumber?.trim() || null
    });
  }
}
