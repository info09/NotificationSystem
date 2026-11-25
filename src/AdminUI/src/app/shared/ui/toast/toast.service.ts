import { Injectable } from '@angular/core';

export type ToastTone = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: number;
  tone: ToastTone;
  message: string;
}

/**
 * Dịch vụ hiển thị toast UI feedback.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly toasts: ToastMessage[] = [];

  show(message: string, tone: ToastTone = 'info', durationMs = 4000): void {
    const toast: ToastMessage = { id: this.nextId++, tone, message };
    this.toasts.push(toast);

    const shouldAutoDismiss = tone === 'success' || tone === 'info';
    if (shouldAutoDismiss) {
      setTimeout(() => this.dismiss(toast.id), durationMs);
    }
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