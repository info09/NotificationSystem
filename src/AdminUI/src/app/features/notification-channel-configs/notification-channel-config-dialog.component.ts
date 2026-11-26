import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { NotificationChannelConfig } from '../../core/notification-channel-configs/notification-channel-config.models';

export interface NotificationChannelConfigDialogData {
  config: NotificationChannelConfig | null;
}

@Component({
  selector: 'app-notification-channel-config-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data.config ? 'Cập nhật Cấu hình kênh thông báo' : 'Tạo Cấu hình kênh thông báo' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form" id="notification-channel-config-form" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Kênh</mat-label>
          <mat-select formControlName="channel">
            <mat-option value="Email">Email</mat-option>
            <mat-option value="InApp">InApp</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Tên hiển thị</mat-label>
          <input matInput formControlName="displayName" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Mô tả</mat-label>
          <textarea matInput rows="3" formControlName="description"></textarea>
        </mat-form-field>

        <mat-slide-toggle formControlName="isEnabled">Bật kênh</mat-slide-toggle>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close()">Hủy</button>
      <button mat-flat-button color="primary" type="submit" form="notification-channel-config-form">Lưu</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-form { display: grid; gap: 4px; padding-top: 8px; min-width: min(560px, 80vw); }
    @media (max-width: 768px) { .dialog-form { min-width: auto; } }
  `]
})
export class NotificationChannelConfigDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly dialogRef = inject(MatDialogRef<NotificationChannelConfigDialogComponent>);
  readonly data = inject<NotificationChannelConfigDialogData>(MAT_DIALOG_DATA);

  readonly form = this.fb.nonNullable.group({
    channel: [{ value: this.data.config?.channel ?? 'Email', disabled: !!this.data.config }, [Validators.required]],
    displayName: [this.data.config?.displayName ?? '', [Validators.required]],
    description: [this.data.config?.description ?? ''],
    isEnabled: [this.data.config?.isEnabled ?? true]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    if (this.data.config) {
      this.dialogRef.close({
        displayName: value.displayName.trim(),
        description: value.description?.trim() || null,
        isEnabled: value.isEnabled
      });
      return;
    }

    this.dialogRef.close({
      channel: value.channel,
      displayName: value.displayName.trim(),
      description: value.description?.trim() || null,
      isEnabled: value.isEnabled
    });
  }
}
