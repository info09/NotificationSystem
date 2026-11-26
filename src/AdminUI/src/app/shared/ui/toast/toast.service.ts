import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

export type ToastTone = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: number;
  tone: ToastTone;
  message: string;
}

/**
 * Dịch vụ hiển thị thông báo UI bằng MatSnackBar.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly snackBar = inject(MatSnackBar);
  private nextId = 1;
  readonly toasts: ToastMessage[] = [];

  show(message: string, tone: ToastTone = 'info', durationMs = 4000): void {
    const toast: ToastMessage = { id: this.nextId++, tone, message };
    this.toasts.push(toast);

    const config: MatSnackBarConfig = {
      duration: tone === 'error' || tone === 'warning' ? 6000 : durationMs,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: [`app-snackbar`, `app-snackbar-${tone}`]
    };

    const ref = this.snackBar.open(message, tone === 'error' || tone === 'warning' ? 'Đóng' : undefined, config);
    ref.afterDismissed().subscribe(() => this.dismiss(toast.id));
  }

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error', 0);
  }

  warning(message: string): void {
    this.show(message, 'warning', 0);
  }

  info(message: string): void {
    this.show(message, 'info');
  }

  dismiss(id: number): void {
    const index = this.toasts.findIndex((item) => item.id === id);
    if (index >= 0) {
      this.toasts.splice(index, 1);
    }
  }
}
