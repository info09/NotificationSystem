import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <main class="page">
      <h1>Tổng quan</h1>
      <p>Trang tổng quan sẽ được triển khai trong các story tiếp theo.</p>
    </main>
  `,
  styles: [`
    .page { padding: 24px; font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif; }
    h1 { margin: 0 0 8px; font-size: 24px; color: #0f172a; }
    p { margin: 0; color: #475569; }
  `]
})
export class DashboardComponent {}
