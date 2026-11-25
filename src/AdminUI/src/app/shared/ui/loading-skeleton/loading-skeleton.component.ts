import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

/**
 * Skeleton loading chuẩn cho khu vực dữ liệu.
 */
@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skeleton-wrap" aria-busy="true" aria-live="polite">
      <div
        *ngFor="let item of items"
        class="skeleton"
        [class.line]="variant === 'line'"
        [class.card]="variant === 'card'"
        [style.height.px]="height"
      ></div>
    </div>
  `,
  styles: [`
    .skeleton-wrap { display: grid; gap: 12px; }
    .skeleton { border-radius: 6px; background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%); background-size: 200% 100%; animation: shimmer 1.2s infinite; }
    .skeleton.line { height: 44px; }
    .skeleton.card { height: 84px; }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }
  `]
})
export class LoadingSkeletonComponent {
  @Input() rows = 3;
  @Input() variant: 'line' | 'card' = 'line';
  @Input() height: number | null = null;

  get items(): number[] {
    const count = Number.isFinite(this.rows) && this.rows > 0 ? Math.floor(this.rows) : 0;
    return Array.from({ length: count }, (_, index) => index);
  }
}