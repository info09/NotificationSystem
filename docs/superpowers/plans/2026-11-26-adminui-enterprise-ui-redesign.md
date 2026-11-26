# AdminUI Enterprise UI Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Nâng cấp toàn bộ AdminUI sang giao diện professional/enterprise với Angular Material, dark sidebar + light content, giữ nguyên nghiệp vụ/API/RBAC.

**Architecture:** Thêm Angular Material/CDK/Material Icons và theme SCSS tùy chỉnh, refactor app shell sang sidenav+topbar, sau đó migrate shared UI và từng màn hình sang Material components theo pattern nhất quán. Giữ route, service, RBAC, hành vi nghiệp vụ hiện có; cập nhật test Angular theo DOM mới.

**Tech Stack:** Angular 17 standalone, Angular Material/CDK, Material Icons/Symbols, SCSS, RxJS, Karma/Jasmine.

**Spec:** `docs/superpowers/specs/2026-11-26-adminui-enterprise-ui-design.md`

## Global Constraints

- Phạm vi: toàn bộ AdminUI (`src/AdminUI`) - Dashboard, Notifications list/detail, DLQ, User Accounts, End User Profiles, Login/Forgot/Reset Password, layout.
- Phong cách: professional/enterprise; bố cục sidebar trái cố định + topbar mỏng; tone dark sidebar + light content.
- Dùng Angular Material + CDK + Material Icons; chấp nhận chuyển global styling sang SCSS cho theme.
- Theme API: dùng API chính thức/stable của phiên bản Angular Material được cài, ưu tiên cấu hình ổn định với Angular 17.
- DLQ detail dùng `MatDialog`; User Accounts/End User Profiles dùng `MatDialog` cho tạo/sửa.
- Refactor shared UI: cập nhật toàn bộ call sites trong cùng đợt, không giữ lớp tương thích cũ song song.
- Chuẩn hóa pattern dùng chung (page header/filter/table shell) chỉ khi lặp từ 2 màn trở lên; không trừu tượng hóa sớm.
- Giữ nguyên route, API contract, RBAC, hành vi nghiệp vụ; giữ thuật ngữ theo `docs/domain-glossary.md`.
- Xin phép Đại ca trước khi chạy lệnh cài package/build/test. Không tự ý commit. Môi trường agent không chạy được test; Đại ca xác nhận pass.

## Review Focus

- **Mobile drawer/focus:** Khi sidebar ở mobile drawer, bàn phím/Tab và focus không bị lọt ra sau overlay; đóng drawer trả focus hợp lý.
- **RBAC menu vs route:** Menu ẩn với người không có quyền, nhưng route guard vẫn chặn truy cập trực tiếp `/user-accounts`, `/end-user-profiles`.
- **Dialog accessibility:** DLQ detail dialog và CRUD dialog có tiêu đề rõ, focus vào dialog, Escape/đóng đúng, không mất dữ liệu ngầm khi đang processing.
- **Query/filter state:** Refactor Notifications/DLQ không làm mất đọc `queryParamMap` (`status`, `channel`, `queueName`) và hành vi lọc/tải lại hiện có.
- **Status contrast/meaning:** Badge trạng thái Pending/Processing/Sent/Failed/Healthy/Degraded/Unhealthy giữ đúng nghĩa và đủ tương phản trên nền light.

---

### Task 1: Cài Angular Material và thiết lập theme SCSS

**Files:**
- Modify: `src/AdminUI/package.json`
- Modify: `src/AdminUI/angular.json`
- Modify: `src/AdminUI/src/index.html`
- Create: `src/AdminUI/src/styles.scss`
- Delete: `src/AdminUI/src/styles.css`
- Create/Modify: `src/AdminUI/src/app/app.config.ts` (nếu cần animation provider)

**Interfaces:**
- Consumes: Angular 17 app hiện tại, `angular.json` styles `src/styles.css`.
- Produces: Angular Material/CDK/icons dependencies; `styles.scss` với theme enterprise tokens; app build config trỏ `styles.scss`.

- [ ] **Step 1: Xin phép Đại ca chạy lệnh cài package**

  Run (sau khi Đại ca đồng ý): `cd src/AdminUI && npx ng add @angular/material --skip-confirmation` hoặc cài thủ công `@angular/material @angular/cdk` + cấu hình. Ghi rõ lệnh sẽ chạy và tác động `package.json/package-lock.json/angular.json`.

- [ ] **Step 2: Cấu hình theme SCSS**

  Tạo `src/AdminUI/src/styles.scss` với Material theme enterprise (dark sidebar/light content tokens qua CSS variables/Material theme). Chuyển nội dung cần giữ từ `styles.css`. Cập nhật `angular.json` styles -> `src/styles.scss`.

- [ ] **Step 3: Cấu hình font/icon/animation**

  Đảm bảo `index.html` hoặc styles có font Inter và Material Icons/Symbols phù hợp. Đảm bảo `provideAnimations()`/`provideAnimationsAsync()` có trong `app.config.ts` nếu Material cần.

- [ ] **Step 4: Smoke build**

  Run (Đại ca): `cd src/AdminUI && npm run build`
  Expected: build PASS.

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI/package.json src/AdminUI/package-lock.json src/AdminUI/angular.json src/AdminUI/src/index.html src/AdminUI/src/styles.scss src/AdminUI/src/app/app.config.ts
  git commit -m "feat(admin-ui): add angular material enterprise theme"
  ```

### Task 2: Refactor app shell sang sidebar + topbar

**Files:**
- Modify: `src/AdminUI/src/app/core/layout/main-layout.component.ts`
- Modify/Create: `src/AdminUI/src/app/core/layout/main-layout.component.spec.ts` (nếu có/ cần thêm)
- Modify: `src/AdminUI/src/app/app.routes.ts` (chỉ nếu cần; dự kiến không)

**Interfaces:**
- Consumes: `AuthService.isSystemAdministrator()`, routes hiện có `/dashboard`, `/notifications`, `/dead-letter-queues`, `/user-accounts`, `/end-user-profiles`.
- Produces: shell Material với `mat-sidenav` dark sidebar, `mat-toolbar` topbar, nav items icon+label, responsive drawer mobile, logout action.

- [ ] **Step 1: Viết/cập nhật test layout**

  Test: render nav items cơ bản; ẩn User Accounts/End User Profiles khi không phải SystemAdministrator; có action đăng xuất gọi `logout()` và điều hướng `/login`.

- [ ] **Step 2: Chạy test để xác nhận FAIL/needs update**

  Run (Đại ca): `cd src/AdminUI && npm test -- --watch=false --browsers=ChromeHeadless --include='**/main-layout*.spec.ts'`
  Expected: FAIL do DOM/layout mới.

- [ ] **Step 3: Implement shell Material**

  Dùng `MatSidenavContainer/MatSidenav/MatSidenavContent`, `MatToolbar`, `MatNavList`, `MatIcon`, `MatButton`. Sidebar dark, content light. Mobile breakpoint chuyển sidenav mode `over`.

- [ ] **Step 4: Chạy test layout**

  Run (Đại ca): lệnh ở Step 2.
  Expected: PASS.

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI/src/app/core/layout/main-layout.component.ts src/AdminUI/src/app/core/layout/main-layout.component.spec.ts
  git commit -m "feat(admin-ui): redesign app shell with material sidenav"
  ```

### Task 3: Chuẩn hóa shared UI components

**Files:**
- Modify: `src/AdminUI/src/app/shared/ui/status-badge/status-badge.component.ts`
- Modify: `src/AdminUI/src/app/shared/ui/empty-state/empty-state.component.ts`
- Modify: `src/AdminUI/src/app/shared/ui/error-state/error-state.component.ts`
- Modify: `src/AdminUI/src/app/shared/ui/loading-skeleton/loading-skeleton.component.ts`
- Modify: `src/AdminUI/src/app/shared/ui/modal/ui-modal.component.ts` hoặc thay bằng dialog helper (cập nhật tất cả call sites)
- Modify: `src/AdminUI/src/app/shared/ui/toast/toast.service.ts`
- Modify: `src/AdminUI/src/app/shared/ui/toast/toast-container.component.ts`
- Modify: `src/AdminUI/src/app/app.component.ts`
- Test: các spec shared tương ứng (tạo nếu thiếu)

**Interfaces:**
- Consumes: API shared component hiện tại được dùng bởi Dashboard/Notifications/DLQ/User Accounts/End User Profiles.
- Produces: shared components theo Material/enterprise style; giữ hoặc cập nhật có kiểm soát API nhưng cập nhật toàn bộ call sites trong cùng đợt.

- [ ] **Step 1: Viết/cập nhật test shared components**

  Bao gồm: status badge render tone/label; empty/error có action; toast service/snackbar wrapper hiển thị success/error; dialog/modal confirm/cancel/processing.

- [ ] **Step 2: Chạy test shared để xác nhận FAIL**

  Run (Đại ca): `cd src/AdminUI && npm test -- --watch=false --browsers=ChromeHeadless --include='**/shared/ui/**/*.spec.ts'`

- [ ] **Step 3: Implement shared components**

  Dùng `MatCard`, `MatIcon`, `MatButton`, `MatProgressBar/Spinner`, `MatSnackBar`, `MatDialog`/wrapper. Giữ ngữ nghĩa trạng thái và thuật ngữ glossary.

- [ ] **Step 4: Cập nhật call sites**

  Cập nhật Dashboard, Notifications, DLQ, User Accounts, End User Profiles, app component theo API mới.

- [ ] **Step 5: Chạy test shared + feature liên quan**

  Run (Đại ca): test shared và spec các màn bị ảnh hưởng.
  Expected: PASS.

- [ ] **Step 6: Commit**

  ```bash
  git add src/AdminUI/src/app/shared/ui src/AdminUI/src/app/app.component.ts src/AdminUI/src/app/features
  git commit -m "refactor(admin-ui): standardize material shared ui"
  ```

### Task 4: Refactor màn hình Auth

**Files:**
- Modify: `src/AdminUI/src/app/features/login/login.component.ts`
- Modify: `src/AdminUI/src/app/features/forgot-password/forgot-password.component.ts`
- Modify: `src/AdminUI/src/app/features/reset-password/reset-password.component.ts`
- Test: `*.spec.ts` tương ứng

**Interfaces:**
- Consumes: `AuthService`/auth flows hiện có, route guest guard.
- Produces: auth layout enterprise card trung tâm, Material form fields/buttons/errors, giữ hành vi submit/validation/redirect.

- [ ] **Step 1: Viết/cập nhật test auth**

  Test render form, validation/error hiển thị, submit gọi service đúng.

- [ ] **Step 2: Chạy test auth FAIL**

  Run (Đại ca): include auth specs.

- [ ] **Step 3: Implement auth UI Material**

  Dùng `MatCard`, `MatFormField`, `MatInput`, `MatButton`, `MatProgressSpinner/Bar`.

- [ ] **Step 4: Chạy test auth PASS**

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI/src/app/features/login src/AdminUI/src/app/features/forgot-password src/AdminUI/src/app/features/reset-password
  git commit -m "feat(admin-ui): redesign auth screens"
  ```

### Task 5: Refactor Dashboard

**Files:**
- Modify: `src/AdminUI/src/app/features/dashboard/dashboard.component.ts`
- Modify: `src/AdminUI/src/app/features/dashboard/dashboard.component.spec.ts`

**Interfaces:**
- Consumes: `/api/admin/dashboard/summary` response.
- Produces: dashboard enterprise với metric cards Material, icon, status tones, per-block loading/empty/error/success.

- [ ] **Step 1: Cập nhật test dashboard theo DOM mới**

  Giữ assertions trạng thái success/empty/per-block error/request fail/refresh.

- [ ] **Step 2: Chạy test dashboard FAIL**

  Run (Đại ca): include `dashboard.component.spec.ts`.

- [ ] **Step 3: Implement dashboard Material**

  Dùng `MatCard`, `MatIcon`, `MatButton`, `MatProgressBar/Skeleton`, link điều hướng giữ query params.

- [ ] **Step 4: Chạy test dashboard PASS**

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI/src/app/features/dashboard/dashboard.component.ts src/AdminUI/src/app/features/dashboard/dashboard.component.spec.ts
  git commit -m "feat(admin-ui): redesign dashboard overview"
  ```

### Task 6: Refactor Notifications list/detail

**Files:**
- Modify: `src/AdminUI/src/app/features/notifications/notifications-list.component.ts`
- Modify: `src/AdminUI/src/app/features/notifications/notification-detail.component.ts`
- Modify: `src/AdminUI/src/app/features/notifications/*.spec.ts`

**Interfaces:**
- Consumes: `NotificationService`, query params `status/channel/page/pageSize/createdFrom/createdTo`.
- Produces: Material filter bar + table + paginator/detail layout; giữ hành vi lọc, đọc query param, điều hướng chi tiết.

- [ ] **Step 1: Cập nhật test notifications**

  Bao gồm đọc query param, render trạng thái, navigation/detail.

- [ ] **Step 2: Chạy test notifications FAIL**

- [ ] **Step 3: Implement list/detail Material**

  Dùng `MatFormField`, `MatSelect`, `MatInput`, `MatTable`, `MatPaginator`, `MatCard`, status badge.

- [ ] **Step 4: Chạy test notifications PASS**

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI/src/app/features/notifications
  git commit -m "feat(admin-ui): redesign notifications screens"
  ```

### Task 7: Refactor Dead-letter Queue

**Files:**
- Modify: `src/AdminUI/src/app/features/dead-letter-queue/dead-letter-queue-list.component.ts`
- Modify: `src/AdminUI/src/app/features/dead-letter-queue/*.spec.ts`
- Create/Modify: dialog component cho detail/resend nếu tách file

**Interfaces:**
- Consumes: `DeadLetterQueueService`, query param `queueName`, resend idempotency.
- Produces: Material table/filter, detail dialog, resend confirm dialog/flow, snackbar kết quả; giữ hành vi resend/audit-facing UX.

- [ ] **Step 1: Cập nhật test DLQ**

  Bao gồm đọc `queueName` query param, mở detail dialog, resend success/error.

- [ ] **Step 2: Chạy test DLQ FAIL**

- [ ] **Step 3: Implement DLQ Material + dialog**

  Dùng `MatDialog` cho detail; giữ confirm resend; hiển thị lỗi/quyền rõ ràng.

- [ ] **Step 4: Chạy test DLQ PASS**

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI/src/app/features/dead-letter-queue
  git commit -m "feat(admin-ui): redesign dead-letter queue screen"
  ```

### Task 8: Refactor User Accounts & End User Profiles

**Files:**
- Modify: `src/AdminUI/src/app/features/user-accounts/user-accounts.component.ts`
- Modify: `src/AdminUI/src/app/features/end-user-profiles/end-user-profiles.component.ts`
- Modify: `src/AdminUI/src/app/features/user-accounts/*.spec.ts`
- Modify: `src/AdminUI/src/app/features/end-user-profiles/*.spec.ts`

**Interfaces:**
- Consumes: services/use cases hiện có, RBAC SystemAdministrator.
- Produces: Material table + dialog create/edit, validation/error/success feedback nhất quán.

- [ ] **Step 1: Cập nhật test CRUD admin screens**

  Bao gồm mở dialog create/edit, submit hợp lệ/không hợp lệ, hiển thị lỗi API.

- [ ] **Step 2: Chạy test CRUD FAIL**

- [ ] **Step 3: Implement CRUD dialog Material**

  Dùng `MatDialog`, `MatFormField`, `MatSelect`, `MatTable`, `MatSnackBar`.

- [ ] **Step 4: Chạy test CRUD PASS**

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI/src/app/features/user-accounts src/AdminUI/src/app/features/end-user-profiles
  git commit -m "feat(admin-ui): redesign admin crud screens"
  ```

### Task 9: Verification, cleanup, và handoff

**Files:**
- Modify: `docs/status.md` (khi có story theo dõi, nếu được Đại ca yêu cầu)
- Modify: các file AdminUI còn sót style cũ nếu có

**Interfaces:**
- Consumes: toàn bộ thay đổi Task 1-8.
- Produces: AdminUI nhất quán enterprise, test/build được Đại ca xác nhận.

- [ ] **Step 1: Rà soát thuật ngữ glossary và responsive**

  Kiểm tra nhãn hiển thị theo `docs/domain-glossary.md`; kiểm tra desktop/tablet/mobile cho sidebar, table, dialog.

- [ ] **Step 2: Rà soát accessibility cơ bản**

  Kiểm tra focus visible, dialog focus trap, contrast status badge, keyboard navigation sidebar.

- [ ] **Step 3: Chạy lint/build/test (Đại ca)**

  Run (Đại ca):
  - `cd src/AdminUI && npm run build`
  - `cd src/AdminUI && npm test -- --watch=false --browsers=ChromeHeadless`
  - `cd src/AdminUI && npm run lint` (nếu script hoạt động)

  Expected: PASS.

- [ ] **Step 4: Sửa lỗi phát sinh và cập nhật docs/status nếu cần**

  Chỉ tick Done khi Đại ca xác nhận test/build liên quan pass.

- [ ] **Step 5: Commit**

  ```bash
  git add src/AdminUI docs/status.md docs/superpowers/specs/2026-11-26-adminui-enterprise-ui-design.md docs/superpowers/plans/2026-11-26-adminui-enterprise-ui-redesign.md
  git commit -m "feat(admin-ui): complete enterprise ui redesign"
  ```
