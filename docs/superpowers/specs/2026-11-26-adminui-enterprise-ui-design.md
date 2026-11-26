# AdminUI Enterprise UI Redesign - Design

## Intent & Why
- **Vấn đề:** AdminUI hiện hoạt động đúng nghiệp vụ nhưng giao diện quá đơn giản, thiếu cảm giác sản phẩm quản trị chuyên nghiệp/enterprise.
- **Mục tiêu:** Nâng cấp toàn bộ AdminUI theo hướng professional/enterprise, nhất quán, dễ vận hành, vẫn giữ nguyên luồng nghiệp vụ/API/RBAC hiện có.
- **Đối tượng:** SystemAdministrator, BusinessOperator (và luồng auth liên quan).

## Goals
- Tạo hệ thống visual nhất quán cho toàn bộ AdminUI: layout, typography, màu, spacing, trạng thái, component.
- Nâng cấp trải nghiệm các màn hình: Dashboard, Thông báo (list/detail), Hàng đợi thư chết, Tài khoản người dùng, Hồ sơ người nhận EndUser, Login/Forgot/Reset Password.
- Dùng Angular Material + theme tùy chỉnh để tăng tính nhất quán, accessibility, tốc độ phát triển.
- Giữ nguyên hành vi nghiệp vụ, route, RBAC, API contract.

## Non-goals
- Không thay đổi backend domain/API/business logic, trừ khi cần chỉnh nhỏ để hỗ trợ hiển thị (không dự kiến).
- Không thêm tính năng nghiệp vụ mới ngoài UI/UX presentation.
- Không làm redesign brand/logo lớn ngoài phạm vi AdminUI hiện có.

## Anti-goals
- Không biến UI thành dashboard "màu mè", nhiều animation gây rối mắt vận hành.
- Không thay đổi flow nghiệp vụ hoặc quyền truy cập.
- Không tạo hệ component song song mâu thuẫn với Angular Material sau khi đã chọn Material.

## Constraints
- Stack hiện tại: Angular 17 standalone components, cấu trúc `src/AdminUI`.
- Cần xin phép Đại ca trước khi chạy lệnh cài package/build/test.
- Môi trường agent không chạy được shell/test; Đại ca xác nhận kết quả test/build.
- Không tự ý commit.
- Phải giữ thuật ngữ theo `docs/domain-glossary.md`.
- Đại ca chấp nhận chuyển styling global sang SCSS để cấu hình theme Angular Material.

## Approaches Considered

### 1. Angular Material + custom enterprise theme (Đã chọn)
- **Mô tả:** Thêm Angular Material/CDK/Material Icons, định nghĩa theme token và refactor layout + toàn bộ màn hình sang Material components.
- **Ưu điểm:** Nhất quán, accessibility tốt, component chín, phù hợp enterprise.
- **Tradeoff:** Thêm dependency, refactor template/style rộng.

### 2. PrimeNG + theme enterprise
- **Ưu điểm:** Nhiều component nghiệp vụ sẵn.
- **Tradeoff:** Cảm giác dễ phụ thuộc lib, tùy biến sâu tốn công, style hệ riêng.

### 3. Chỉ thêm icon + tự xây design system CSS
- **Ưu điểm:** Nhẹ, kiểm soát hoàn toàn.
- **Tradeoff:** Tốn tay, khó nhất quán khi làm toàn bộ AdminUI.

**Quyết định:** Chọn hướng 1 theo xác nhận của Đại ca.

## Design Decisions (Đã chốt với Đại ca)
- **Phạm vi:** Toàn bộ AdminUI.
- **Phong cách:** Professional, enterprise.
- **Thư viện:** Chấp nhận thêm bộ UI/icon phù hợp; dùng Angular Material.
- **Bố cục:** Sidebar trái cố định + topbar mỏng (hướng 1).
- **Tone:** Dark sidebar + light content (hướng 1).
- **Theme/styling:** Chấp nhận chuyển sang SCSS cho Angular Material theme; dùng theme API chính thức/stable của phiên bản Material được cài (ưu tiên ổn định với Angular 17).
- **DLQ detail:** Dùng **dialog**.
- **User Accounts / End User Profiles:** Dùng **dialog** cho tạo/sửa.
- **Refactor shared UI:** Cập nhật toàn bộ call sites trong cùng đợt, không giữ lớp tương thích cũ song song.
- **Pattern dùng chung:** Chuẩn hóa page header/filter/table shell khi lặp từ 2 màn trở lên; không trừu tượng hóa sớm.

## Visual System

### Layout
- **Shell:** `MatSidenavContainer` với sidenav trái cố định trên desktop, chuyển `over`/drawer trên mobile.
- **Sidebar:** nền xanh than đậm (slate/navy), logo/tên hệ thống, nav item có icon + label, active state rõ (accent bar/background), nhóm menu theo vận hành/quản trị.
- **Topbar:** nền trắng, tiêu đề trang/ngữ cảnh, khu vực user/action (Đăng xuất). Không dùng top nav chính.
- **Content:** nền xám rất nhạt, max-width hợp lý hoặc full fluid có padding chuẩn; page header gồm title, subtitle, primary action.

### Color & Tokens
- **Primary:** xanh dương enterprise (kiểm soát độ bão hòa).
- **Sidebar:** dark slate (`#0F172A`/`#111827` range), text/icon muted light, active accent primary.
- **Surface:** trắng cho card/table, nền app `#F8FAFC`/gần đó.
- **Status:** dùng palette có kiểm soát cho Pending/Processing/Sent/Failed/Healthy/Degraded/Unhealthy; không dùng màu neon.
- **Focus:** visible outline/ring rõ, đạt accessibility.

### Typography & Spacing
- Giữ Inter/System sans fallback.
- Hierarchy: H1 page title, section title, body/meta.
- Spacing theo 8px grid; card padding 16/24; table row density medium.

### Motion
- Tối thiểu: transition hover/focus, drawer mở đóng, dialog/snackbar. Không animation phức tạp.

## Component Mapping

| Hiện tại | Đích |
|---|---|
| Custom buttons | `MatButton`, `MatIconButton` |
| Form input/select | `MatFormField`, `MatInput`, `MatSelect` |
| Tables | `MatTable` (+ paginator/sort khi phù hợp) |
| Modal xác nhận | `MatDialog` |
| Toast service | `MatSnackBar` hoặc wrapper service trên snackbar |
| Status badge | Component badge/chip dùng Material token |
| Loading skeleton | giữ component skeleton nhẹ hoặc dùng progress bar/spinner theo ngữ cảnh |
| Empty/Error state | card/block chuẩn hóa với icon, message, action |

## Screen Notes

### Auth (Login/Forgot/Reset)
- Card auth trung tâm, nền nhẹ enterprise, logo/tên hệ thống, form Material, error rõ ràng.
- Giữ nguyên hành vi/validation/auth flow.

### Dashboard
- Page header + nút làm mới.
- 3 khối summary dùng card Material; giữ loading/empty/error/success theo từng khối.
- Metric cards có icon, giá trị nổi bật, tone trạng thái.

### Notifications List/Detail
- List: filter bar Material, table Material, badge trạng thái/kênh, pagination.
- Detail: layout đọc rõ, metadata dạng definition list/card, trạng thái/lịch sử hiển thị nhất quán.

### Dead-letter Queue
- Filter + table; chi tiết Thông điệp dùng **MatDialog**; action Gửi lại dùng MatDialog xác nhận và MatSnackBar kết quả.
- Giữ query param `queueName`, resend idempotency, RBAC.

### User Accounts / End User Profiles
- Table + dialog tạo/sửa theo pattern CRUD admin; action rõ, confirm khi cần.
- Chỉ SystemAdministrator thấy menu/route như hiện tại.

## Accessibility & Responsive
- Contrast đạt mức tốt cho dark sidebar và status badge.
- Focus visible cho keyboard.
- Mobile: sidebar thành drawer, table cho phép horizontal scroll hoặc responsive stack hợp lý.
- Touch target đủ lớn.

## Implementation Plan (High Level)
1. Cài Angular Material/CDK/icons (cần phép Đại ca chạy lệnh), cấu hình theme/tokens/global styles.
2. Refactor app shell/layout (sidenav, topbar, nav, responsive).
3. Refactor shared UI components để dùng Material/wrapper nhất quán.
4. Refactor màn hình auth.
5. Refactor Dashboard, Notifications, DLQ.
6. Refactor User Accounts, End User Profiles.
7. Kiểm tra thuật ngữ glossary, responsive, accessibility cơ bản.
8. Chạy lint/build/test AdminUI (Đại ca xác nhận), sửa lỗi.

## Testing / Verification
- Unit test AdminUI hiện có cập nhật theo DOM mới.
- Smoke/manual: login, dashboard load, filter notifications, open detail, DLQ resend modal, admin CRUD screens, logout.
- Kiểm tra responsive desktop/tablet/mobile.
- Đại ca xác nhận build/test pass trước khi tick Done story liên quan.

## Open Items (Builder may decide)
- Chọn `MatSnackBar` trực tiếp hay wrapper service hiện có bọc snackbar.
- Dùng Material table đầy đủ hay table HTML + Material styling cho màn đơn giản (ưu tiên nhất quán).
- Chi tiết icon cụ thể cho từng menu/status (phải rõ ràng, enterprise).
- Có cần dark mode toggle không: mặc định **không** trong scope này.