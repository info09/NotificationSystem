import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { NotificationTemplate } from '../../core/notification-templates/notification-template.models';

export interface NotificationTemplateDialogData {
  template: NotificationTemplate | null;
}

@Component({
  selector: 'app-notification-template-dialog',
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
    <h2 mat-dialog-title>{{ data.template ? 'Cập nhật Mẫu thông báo' : 'Tạo Mẫu thông báo' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form" id="notification-template-form" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Khóa mẫu</mat-label>
          <input matInput formControlName="templateKey" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Tên hiển thị</mat-label>
          <input matInput formControlName="name" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Kênh</mat-label>
          <mat-select formControlName="channel">
            <mat-option value="Email">Email</mat-option>
            <mat-option value="InApp">InApp</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Tiêu đề</mat-label>
          <input matInput formControlName="subject" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Mô tả</mat-label>
          <textarea matInput rows="2" formControlName="description"></textarea>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Nội dung mẫu</mat-label>
          <textarea matInput rows="6" formControlName="body"></textarea>
        </mat-form-field>

        <mat-slide-toggle formControlName="isActive">Kích hoạt</mat-slide-toggle>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close()">Hủy</button>
      <button mat-flat-button color="primary" type="submit" form="notification-template-form">Lưu</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-form { display: grid; gap: 4px; padding-top: 8px; min-width: min(680px, 80vw); }
    @media (max-width: 768px) { .dialog-form { min-width: auto; } }
  `]
})
export class NotificationTemplateDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly dialogRef = inject(MatDialogRef<NotificationTemplateDialogComponent>);
  readonly data = inject<NotificationTemplateDialogData>(MAT_DIALOG_DATA);

  readonly form = this.fb.nonNullable.group({
    templateKey: [{ value: this.data.template?.templateKey ?? '', disabled: !!this.data.template }, [Validators.required]],
    name: [this.data.template?.name ?? '', [Validators.required]],
    channel: [this.data.template?.channel ?? 'Email', [Validators.required]],
    subject: [this.data.template?.subject ?? ''],
    description: [this.data.template?.description ?? ''],
    body: [this.data.template?.body ?? '', [Validators.required]],
    isActive: [this.data.template?.isActive ?? true]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    if (this.data.template) {
      this.dialogRef.close({
        name: value.name.trim(),
        description: value.description?.trim() || null,
        channel: value.channel,
        subject: value.subject?.trim() || null,
        body: value.body.trim(),
        isActive: value.isActive
      });
      return;
    }

    this.dialogRef.close({
      templateKey: value.templateKey.trim(),
      name: value.name.trim(),
      description: value.description?.trim() || null,
      channel: value.channel,
      subject: value.subject?.trim() || null,
      body: value.body.trim(),
      isActive: value.isActive
    });
  }
}
