import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { NotificationTemplate } from '../../core/notification-templates/notification-template.models';
import { NotificationTemplateService } from '../../core/notification-templates/notification-template.service';
import { IntegrationEventRoute } from '../../core/integration-event-routes/integration-event-route.models';

export interface IntegrationEventRouteDialogData {
  route: IntegrationEventRoute | null;
}

@Component({
  selector: 'app-integration-event-route-dialog',
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
    <h2 mat-dialog-title>{{ data.route ? 'Cập nhật Định tuyến sự kiện tích hợp' : 'Tạo Định tuyến sự kiện tích hợp' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form" id="integration-event-route-form" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Loại sự kiện tích hợp</mat-label>
          <input matInput formControlName="eventType" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Mẫu thông báo</mat-label>
          <mat-select formControlName="notificationTemplateId">
            <mat-option [value]="null">Không chọn</mat-option>
            <mat-option *ngFor="let template of templates" [value]="template.id">
              {{ template.name }} ({{ template.templateKey }})
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Kênh</mat-label>
          <mat-select formControlName="channel">
            <mat-option [value]="null">Không chọn</mat-option>
            <mat-option value="Email">Email</mat-option>
            <mat-option value="InApp">InApp</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Mô tả</mat-label>
          <textarea matInput rows="3" formControlName="description"></textarea>
        </mat-form-field>

        <mat-slide-toggle formControlName="isActive">Kích hoạt</mat-slide-toggle>

        <p class="mapping-hint" *ngIf="mappingError">Cần chọn ít nhất Mẫu thông báo hoặc Kênh.</p>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close()">Hủy</button>
      <button mat-flat-button color="primary" type="submit" form="integration-event-route-form">Lưu</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-form { display: grid; gap: 4px; padding-top: 8px; min-width: min(680px, 80vw); }
    .mapping-hint { margin: 4px 0 0; color: #dc2626; font-size: 13px; }
    @media (max-width: 768px) { .dialog-form { min-width: auto; } }
  `]
})
export class IntegrationEventRouteDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly templateService = inject(NotificationTemplateService);
  readonly dialogRef = inject(MatDialogRef<IntegrationEventRouteDialogComponent>);
  readonly data = inject<IntegrationEventRouteDialogData>(MAT_DIALOG_DATA);

  templates: NotificationTemplate[] = [];
  mappingError = false;

  readonly form = this.fb.group({
    eventType: [this.data.route?.eventType ?? '', [Validators.required]],
    notificationTemplateId: [this.data.route?.notificationTemplateId ?? null as string | null],
    channel: [this.data.route?.channel ?? null as string | null],
    description: [this.data.route?.description ?? ''],
    isActive: [this.data.route?.isActive ?? true]
  });

  ngOnInit(): void {
    this.templateService.getNotificationTemplates({ page: 1, pageSize: 100 }).subscribe({
      next: (response) => (this.templates = response.items),
      error: () => (this.templates = [])
    });
  }

  submit(): void {
    this.mappingError = false;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const notificationTemplateId = value.notificationTemplateId || null;
    const channel = value.channel?.trim() || null;
    if (!notificationTemplateId && !channel) {
      this.mappingError = true;
      return;
    }

    this.dialogRef.close({
      eventType: value.eventType?.trim() ?? '',
      notificationTemplateId,
      channel,
      description: value.description?.trim() || null,
      isActive: !!value.isActive
    });
  }
}
