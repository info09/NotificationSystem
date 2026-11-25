import { CommonModule } from '@angular/common';
import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';

/**
 * Modal xác nhận/hành động chuẩn.
 */
@Component({
  selector: 'app-ui-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="open" class="modal-backdrop" role="presentation" (click)="onBackdropClick($event)">
      <div class="modal" role="dialog" aria-modal="true" [attr.aria-labelledby]="titleId">
        <h2 [id]="titleId" class="modal-title">{{ title }}</h2>
        <p *ngIf="description" class="modal-description">{{ description }}</p>
        <div class="modal-body">
          <ng-content></ng-content>
        </div>
        <div class="modal-actions">
          <button type="button" class="secondary" (click)="cancel.emit()" [disabled]="processing">{{ cancelLabel }}</button>
          <button type="button" [ngClass]="danger ? 'danger' : 'primary'" (click)="confirm.emit()" [disabled]="processing">{{ confirmLabel }}</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-backdrop { position: fixed; inset: 0; background: rgba(15, 23, 42, 0.45); display: flex; align-items: center; justify-content: center; padding: 16px; z-index: 1000; }
    .modal { width: min(560px, 100%); background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; padding: 20px; box-shadow: 0 20px 40px rgba(15, 23, 42, 0.2); }
    .modal-title { margin: 0 0 8px; font-size: 18px; color: #0f172a; }
    .modal-description { margin: 0 0 12px; color: #475569; font-size: 14px; }
    .modal-body:empty { display: none; }
    .modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
    button { border-radius: 6px; padding: 8px 12px; font-size: 14px; cursor: pointer; font-family: inherit; border: 1px solid transparent; }
    button:disabled { opacity: 0.6; cursor: not-allowed; }
    .primary { background: #2563eb; color: #fff; border-color: #2563eb; }
    .danger { background: #dc2626; color: #fff; border-color: #dc2626; }
    .secondary { background: #fff; color: #0f172a; border-color: #cbd5e1; }
    button:focus { outline: 2px solid #2563eb; outline-offset: 1px; }
    @media (max-width: 767px) { .modal { width: 100%; } .modal-actions { flex-direction: column-reverse; } .modal-actions button { width: 100%; } }
  `]
})
export class UiModalComponent {
  @Input() open = false;
  @Input() title = '';
  @Input() description = '';
  @Input() confirmLabel = 'Xác nhận';
  @Input() cancelLabel = 'Hủy';
  @Input() processing = false;
  @Input() danger = false;
  @Input() titleId = 'ui-modal-title';
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.open && !this.processing) {
      this.cancel.emit();
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget && !this.processing) {
      this.cancel.emit();
    }
  }
}