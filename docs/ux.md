# UX Spec — Admin UI (Notification System)

> Tài liệu này là **cơ sở duy nhất** để sinh UI/UX nhất quán khi implement `AdminUI` (Angular SPA, xem `docs/architecture.md`). Mọi label, tên màn hình, tên trạng thái trong UI **PHẢI** dùng đúng thuật ngữ hiển thị đã chốt trong `docs/domain-glossary.md`. Không tự bịa phong cách thiết kế; các mục chưa có brand guideline được đánh dấu **[ĐỀ XUẤT - CẦN DUYỆT]**.

## 1. Nguyên tắc thiết kế
Nguyên tắc gắn với đối tượng người dùng trong `docs/prd.md` (mục 2):

1. **Ưu tiên rõ ràng cho vận hành:** `Nhân viên vận hành` (thấp–trung bình kỹ thuật) cần hiểu nhanh tình trạng `Thông báo`, lý do lỗi và bước xử lý tiếp theo; `Quản trị viên hệ thống` cần thao tác cấu hình chính xác. UI phải hiển thị trạng thái, thời gian, và nguyên nhân lỗi bằng ngôn ngữ dễ hiểu, tránh bắt người dùng suy đoán.
2. **Giảm số bước cho tác vụ quan trọng:** Tra cứu `Thông báo`, xem `Nhật ký gửi thông báo`/`Lần thử gửi`, xem `Hàng đợi thư chết` và thực hiện `Gửi lại` phải thực hiện được trong ít thao tác, có bộ lọc và ngữ cảnh rõ ràng (trạng thái, kênh, thời gian).
3. **An toàn khi thao tác nhạy cảm:** Các hành động có tác động (ví dụ `Gửi lại`, thay đổi `Mẫu thông báo`, `Cấu hình kênh thông báo`, `Định tuyến sự kiện tích hợp`, bật/tắt cấu hình) phải có xác nhận, hiển thị hậu quả ngắn gọn, và phản hồi kết quả rõ (thành công/thất bại) để hạn chế sai sót.
4. **Nhất quán thuật ngữ và trạng thái:** Luôn dùng đúng tên hiển thị trong `docs/domain-glossary.md` cho màn hình, nhãn, trạng thái (`Pending` (Chờ xử lý), `Processing` (Đang xử lý), `Sent` (Đã gửi), `Failed` (Thất bại), `Unread` (chưa đọc), `Read` (đã đọc)) và khái niệm nghiệp vụ; không dùng từ đồng nghĩa thay thế trong UI.
5. **Trạng thái hệ thống luôn quan sát được:** Mọi màn hình danh sách/chi tiết phải thể hiện rõ loading/empty/error/success; lỗi hiển thị thông điệp hữu ích và gợi ý hành động (thử lại, kiểm tra bộ lọc, liên hệ `Quản trị viên hệ thống` khi thiếu quyền).

## 2. Information Architecture

### 2.1 Cây điều hướng (Admin UI)
```text
Admin UI
├─ Đăng nhập / Login
└─ Khu vực sau đăng nhập (RBAC)
   ├─ Tổng quan / Dashboard
   ├─ Thông báo / Notification
   │  ├─ Danh sách Thông báo
   │  └─ Chi tiết Thông báo
   │     ├─ Thông tin Thông báo & Trạng thái thông báo
   │     └─ Nhật ký gửi thông báo / NotificationDeliveryLog
   │        └─ Lần thử gửi / DeliveryAttempt
   ├─ Hàng đợi thư chết / DeadLetterQueue
   │  ├─ Danh sách Hàng đợi thư chết
   │  └─ Chi tiết & thao tác Gửi lại / Resend
   └─ Cấu hình (Quản trị viên hệ thống)
      ├─ Mẫu thông báo / NotificationTemplate
      ├─ Cấu hình kênh thông báo / NotificationChannelConfig
      └─ Định tuyến sự kiện tích hợp / IntegrationEventRoute
```

> Ghi chú: `Đăng xuất / Logout` là hành động toàn cục trong khu vực sau đăng nhập (header/menu người dùng), không phải màn hình nội dung riêng. `Nhân viên vận hành` có quyền truy cập `Tổng quan`, `Thông báo`, `Hàng đợi thư chết` và thao tác `Gửi lại` theo quyền; không thấy/quyền hạn chế với nhóm Cấu hình (theo RBAC). `Quản trị viên hệ thống` có toàn quyền vận hành/cấu hình.

### 2.2 Mapping màn hình ↔ FR
| Màn hình / Khu vực (tên hiển thị / code) | Mục đích chính | FR liên quan |
|---|---|---|
| `Đăng nhập` / `Login` | Xác thực truy cập `AdminUI` bằng `JwtBearer`, định hướng theo vai trò `RBAC`. | FR-16, NFR-05 |
| `Tổng quan` / `Dashboard` | Theo dõi hoạt động tổng thể, tóm tắt số liệu/ tình trạng (`Kiểm tra sức khỏe`, thông báo gần đây, DLQ cần chú ý). | FR-15, FR-16 (phần giám sát hoạt động) |
| `Thông báo` (Danh sách & Chi tiết) / `Notification` | Tra cứu `Thông báo`, xem `Trạng thái thông báo`, thông tin người nhận/kênh, truy vết theo `Mã tương quan` (`MessageId`/`notificationId`). | FR-06, FR-14, FR-16 |
| `Nhật ký gửi thông báo` & `Lần thử gửi` (trong Chi tiết `Thông báo`) / `NotificationDeliveryLog`, `DeliveryAttempt` | Xem lịch sử gửi/thử gửi, lỗi, thời gian, số lần `Thử lại`. | FR-14, FR-08 (hiển thị kết quả retry) |
| `Hàng đợi thư chết` / `DeadLetterQueue` | Xem thông điệp/thông báo lỗi trong DLQ, nguyên nhân, và thực hiện `Gửi lại`. | FR-09, FR-16 |
| `Mẫu thông báo` / `NotificationTemplate` | CRUD và quản lý nội dung/mẫu render cho `Thông báo`. | FR-16, FR-11 |
| `Cấu hình kênh thông báo` / `NotificationChannelConfig` | Quản lý cấu hình/bật tắt kênh (`Email`, `Thông báo trong ứng dụng`) ở mức không nhạy cảm. | FR-16, FR-11, FR-12 |
| `Định tuyến sự kiện tích hợp` / `IntegrationEventRoute` | Quản lý ánh xạ `Sự kiện tích hợp` → `Mẫu thông báo`/`Kênh thông báo`. | FR-02 (mapping), FR-16 |

## 3. Danh sách màn hình/luồng chính

> Quy ước chung cho mọi luồng: phải có trạng thái **Loading / Empty / Error / Success** (hoặc trạng thái dữ liệu hợp lệ tương đương). Không bỏ qua trạng thái lỗi hoặc rỗng khi implement.

### 3.1 Đăng nhập / `Login`
- **Mục tiêu:** Cho `Quản trị viên hệ thống` / `Nhân viên vận hành` truy cập `AdminUI`.
- **Bước chính:**
  1. Người dùng mở màn hình `Đăng nhập`.
  2. Nhập thông tin xác thực và gửi.
  3. Hệ thống xác thực (`JwtBearer`), kiểm tra `RBAC`.
  4. Chuyển đến `Tổng quan` khi thành công.
- **Trạng thái bắt buộc:**
  - **Loading:** Nút gửi và form ở trạng thái đang xử lý, tránh submit lặp.
  - **Empty:** Trạng thái ban đầu form trống/hợp lệ theo validation.
  - **Error:** Sai thông tin, tài khoản không có quyền, phiên hết hạn, lỗi mạng/hệ thống; hiển thị thông báo rõ và cho phép thử lại.
  - **Success:** Đăng nhập thành công, điều hướng tới `Tổng quan`.
- **Quyền:** Chỉ vai trò được phép truy cập `AdminUI`.

### 3.2 Tổng quan / `Dashboard`
- **Mục tiêu:** Cung cấp cái nhìn nhanh về hoạt động và tình trạng hệ thống.
- **Nội dung tối thiểu:** Tóm tắt `Thông báo` theo `Trạng thái thông báo` (gần đây), số lượng mục trong `Hàng đợi thư chết` cần xử lý, trạng thái `Kiểm tra sức khỏe` (API/DB/RabbitMQ) và lối tắt tới màn hình liên quan.
- **Bước chính:** Tải dữ liệu tổng hợp → hiển thị thẻ/tổng quan → người dùng điều hướng tới chi tiết.
- **Trạng thái bắt buộc:**
  - **Loading:** Skeleton/thẻ chờ cho từng khối dữ liệu.
  - **Empty:** Không có dữ liệu trong khoảng quan sát (hiển thị "Chưa có dữ liệu" và hướng dẫn).
  - **Error:** Không tải được một phần/toàn bộ dữ liệu; hiển thị lỗi từng khối (không chặn toàn trang nếu có thể) và nút thử lại.
  - **Success:** Dữ liệu hiển thị đầy đủ, có thể làm mới.

### 3.3 Tra cứu Thông báo / `Notification` (Danh sách)
- **Mục tiêu:** Tìm và theo dõi `Thông báo` theo trạng thái/kênh/thời gian.
- **Bước chính:**
  1. Mở danh sách `Thông báo`.
  2. Áp dụng bộ lọc tối thiểu: `Trạng thái thông báo` (`Pending`, `Processing`, `Sent`, `Failed`), `Kênh thông báo` (`Email`, `Thông báo trong ứng dụng`), khoảng thời gian.
  3. Xem kết quả dạng bảng (phân trang), chọn một `Thông báo` để xem chi tiết.
- **Trạng thái bắt buộc:**
  - **Loading:** Skeleton bảng khi tải/đổi filter/phân trang.
  - **Empty:** Không có kết quả phù hợp; hiển thị trạng thái rỗng với gợi ý điều chỉnh bộ lọc/xóa lọc.
  - **Error:** Lỗi tải danh sách; hiển thị thông báo và nút thử lại, giữ nguyên giá trị bộ lọc.
  - **Success:** Hiển thị danh sách, phân trang, và truy cập chi tiết.

### 3.4 Chi tiết Thông báo, Nhật ký gửi thông báo & Lần thử gửi
- **Mục tiêu:** Điều tra trạng thái và lịch sử xử lý/gửi của một `Thông báo`.
- **Bước chính:**
  1. Mở chi tiết từ danh sách `Thông báo`.
  2. Xem thông tin chính: `notificationId`, `Người nhận`, `Kênh thông báo`, `Trạng thái thông báo`, thời gian, `Mã tương quan` liên quan.
  3. Xem `Nhật ký gửi thông báo` và các `Lần thử gửi` (thời gian, kết quả, lỗi, lần `Thử lại`).
- **Trạng thái bắt buộc:**
  - **Loading:** Skeleton cho vùng chi tiết và vùng lịch sử/lần thử gửi.
  - **Empty:** `Thông báo` chưa có `Lần thử gửi`/nhật ký (hiển thị giải thích ngắn, ví dụ đang `Pending`).
  - **Error:** Không tải được chi tiết hoặc lịch sử; cho phép thử lại từng vùng nếu có thể.
  - **Success:** Hiển thị đầy đủ thông tin và lịch sử theo dữ liệu.

### 3.5 Hàng đợi thư chết / `DeadLetterQueue` và Gửi lại / `Resend`
- **Mục tiêu:** Rà soát thông điệp/thông báo lỗi và thực hiện `Gửi lại` an toàn.
- **Bước chính:**
  1. Mở danh sách `Hàng đợi thư chết` (lọc theo kênh/thời gian/lý do nếu có).
  2. Xem chi tiết mục lỗi (nguyên nhân, số lần `Thử lại`, `Mã tương quan`).
  3. Chọn `Gửi lại` (một hoặc thao tác được hỗ trợ) → xác nhận trong modal → hệ thống thực hiện theo cơ chế idempotent → hiển thị kết quả.
- **Trạng thái bắt buộc:**
  - **Loading:** Tải danh sách/chi tiết; trạng thái đang xử lý khi thực hiện `Gửi lại`.
  - **Empty:** Không có mục trong `Hàng đợi thư chết` (trạng thái tích cực, rõ ràng).
  - **Error:** Lỗi tải DLQ hoặc lỗi khi `Gửi lại`; hiển thị nguyên nhân (nếu có) và hướng dẫn thử lại/kiểm tra quyền.
  - **Success:** `Gửi lại` được tiếp nhận/thực thi thành công (hiển thị thông báo thành công và cập nhật danh sách/trạng thái hiển thị).

### 3.6 Quản lý Mẫu thông báo / `NotificationTemplate` (CRUD)
- **Mục tiêu:** `Quản trị viên hệ thống` tạo/cập nhật/quản lý `Mẫu thông báo`.
- **Bước chính:** Danh sách → tạo mới hoặc chọn sửa → nhập/sửa thông tin mẫu (theo schema API) → lưu → phản hồi.
- **Trạng thái bắt buộc:**
  - **Loading:** Tải danh sách/chi tiết; đang lưu.
  - **Empty:** Chưa có `Mẫu thông báo`; hiển thị trạng thái rỗng và hành động tạo mới (nếu có quyền).
  - **Error:** Lỗi tải/lưu/validation; hiển thị lỗi tại trường liên quan và/hoặc thông báo tổng quát.
  - **Success:** Lưu/tạo/cập nhật thành công, danh sách/chi tiết được cập nhật.

### 3.7 Quản lý Cấu hình kênh thông báo / `NotificationChannelConfig`
- **Mục tiêu:** Quản lý cấu hình/bật tắt `Kênh thông báo` (`Email`, `Thông báo trong ứng dụng`) ở mức tham số không nhạy cảm.
- **Bước chính:** Xem danh sách cấu hình → chỉnh sửa/bật tắt → xác nhận nếu cần → lưu.
- **Trạng thái bắt buộc:** Loading / Empty / Error / Success (tương tự 3.6). Không hiển thị hoặc cho chỉnh sửa secret/credential (theo kiến trúc bảo mật).

### 3.8 Quản lý Định tuyến sự kiện tích hợp / `IntegrationEventRoute`
- **Mục tiêu:** Cấu hình ánh xạ `Sự kiện tích hợp` → `Mẫu thông báo` và/hoặc `Kênh thông báo`.
- **Bước chính:** Danh sách route → tạo/sửa/xóa hoặc bật tắt (theo phạm vi API) → lưu → phản hồi.
- **Trạng thái bắt buộc:** Loading / Empty / Error / Success. Validation rõ khi thiếu mapping bắt buộc hoặc event type không hợp lệ.

## 4. Design tokens cơ bản

> Chưa có brand guideline. Các token dưới đây là baseline cho `AdminUI` vận hành (rõ ràng, trung tính, ưu tiên đọc dữ liệu/bảng/trạng thái). **[ĐỀ XUẤT - CẦN DUYỆT]**

### 4.1 Màu sắc (Color tokens)
| Token | Giá trị đề xuất | Mục đích sử dụng |
|---|---|---|
| `--color-primary` | `#2563EB` | Hành động chính, liên kết, trạng thái focus/active chính |
| `--color-primary-hover` | `#1D4ED8` | Hover cho hành động chính |
| `--color-secondary` | `#475569` | Hành động phụ, thành phần trung tính nhấn nhẹ |
| `--color-bg` | `#F8FAFC` | Nền ứng dụng |
| `--color-surface` | `#FFFFFF` | Nền thẻ, bảng, modal, form |
| `--color-border` | `#CBD5E1` | Viền, phân tách |
| `--color-text` | `#0F172A` | Văn bản chính |
| `--color-text-muted` | `#475569` | Văn bản phụ, mô tả, metadata |
| `--color-success` | `#16A34A` | Thành công, `Sent` (Đã gửi), `Read` (đã đọc) khi dùng badge tương ứng |
| `--color-warning` | `#D97706` | Cảnh báo, `Pending` (Chờ xử lý)/cần chú ý |
| `--color-error` | `#DC2626` | Lỗi, `Failed` (Thất bại), validation lỗi |
| `--color-info` | `#0284C7` | Thông tin trung tính, `Processing` (Đang xử lý) |
| `--color-focus-ring` | `#2563EB` | Vòng focus rõ cho keyboard navigation |

> Lưu ý: Badge trạng thái phải kết hợp **màu + nhãn văn bản** (không chỉ màu) để đảm bảo khả năng nhận biết.

### 4.2 Typography scale
| Token | Kích thước / trọng lượng đề xuất | Sử dụng |
|---|---|---|
| `--font-family-base` | Hệ thống sans-serif (`Inter`, `Segoe UI`, `Roboto`, `Arial`, sans-serif) | Toàn bộ `AdminUI` |
| `--text-h1` | 24px / 600 / line-height 1.3 | Tiêu đề trang chính |
| `--text-h2` | 20px / 600 / line-height 1.35 | Tiêu đề khu vực/thẻ lớn |
| `--text-h3` | 16px / 600 / line-height 1.4 | Tiêu đề bảng/section/form |
| `--text-body` | 14px / 400 / line-height 1.5 | Nội dung mặc định, dữ liệu bảng, form |
| `--text-body-strong` | 14px / 600 / line-height 1.5 | Nhãn quan trọng, giá trị nhấn mạnh |
| `--text-caption` | 12px / 400 / line-height 1.4 | Metadata, trợ giúp, ghi chú thời gian/ID |

### 4.3 Spacing scale
| Token | Giá trị | Sử dụng |
|---|---|---|
| `--space-1` | 4px | Khoảng cách rất nhỏ (icon/text) |
| `--space-2` | 8px | Khoảng cách trong control/nhóm nhỏ |
| `--space-3` | 12px | Khoảng cách giữa phần tử form/badge |
| `--space-4` | 16px | Padding chuẩn card/section, gap cột |
| `--space-5` | 24px | Khoảng cách giữa section |
| `--space-6` | 32px | Khoảng cách lớn giữa khối trang |

### 4.4 Breakpoints (Responsive)
| Token | Giá trị đề xuất | Mục tiêu |
|---|---|---|
| `--bp-mobile` | `< 768px` | Màn hình nhỏ (hỗ trợ tối thiểu) |
| `--bp-tablet` | `768px – 1023px` | Máy tính bảng/màn hình trung bình |
| `--bp-desktop` | `>= 1024px` | Màn hình làm việc chính của `AdminUI` |

## 5. Component pattern chung

| Component | Hành vi mặc định | Khi nào dùng | Ghi chú bắt buộc |
|---|---|---|---|
| **Form** | Label rõ ràng phía trên/đồng nhất; trường bắt buộc có dấu hiệu; validate inline khi blur/submit; nút chính ở cuối (phải), nút phụ/hủy bên trái nút chính; disable khi đang submit. | `Đăng nhập`, tạo/sửa `Mẫu thông báo`, `Cấu hình kênh thông báo`, `Định tuyến sự kiện tích hợp`. | Hiển thị lỗi gắn với trường và thông báo tổng quát khi cần; không xóa dữ liệu người dùng đã nhập khi lỗi server. |
| **Table** | Header cố định ngữ nghĩa, cột căn chỉnh theo loại dữ liệu, hỗ trợ phân trang, sắp xếp/lọc khi phù hợp; trạng thái hàng hiển thị bằng badge (nhãn + màu). | Danh sách `Thông báo`, `Hàng đợi thư chết`, `Mẫu thông báo`, `Cấu hình kênh thông báo`, `Định tuyến sự kiện tích hợp`, `Nhật ký gửi thông báo`/`Lần thử gửi`. | Phải có loading skeleton, empty state, error state; hiển thị `Mã tương quan`/ID dạng dễ copy khi cần truy vết. |
| **Modal (xác nhận/hành động)** | Chặn tương tác nền, tiêu đề rõ, mô tả ngắn hậu quả, nút xác nhận và hủy; đóng bằng nút/ESC (khi an toàn), focus trap. | Xác nhận `Gửi lại`, xóa/vô hiệu hóa cấu hình (nếu có), thay đổi nhạy cảm. | Hành động phá hoại/nhạy cảm dùng màu `--color-error` hoặc nhấn mạnh cảnh báo; mặc định focus vào nút an toàn (Hủy) với thao tác rủi ro. |
| **Toast/Notification (UI feedback)** | Hiển thị ngắn gọn kết quả hành động (success/error/warning/info), tự ẩn với success/info, error quan trọng có thể yêu cầu đóng thủ công; không che khu vực thao tác chính. | Phản hồi lưu form, `Gửi lại`, làm mới dữ liệu, lỗi không gắn trường cụ thể. | Nội dung toast dùng thuật ngữ glossary; không dùng toast thay cho lỗi validation trong form. (Phân biệt với khái niệm nghiệp vụ `Thông báo`.) |
| **Empty state** | Icon/hình minh họa trung tính (tùy chọn), tiêu đề ngắn, mô tả 1–2 dòng, hành động gợi ý (xóa bộ lọc, tạo mới, thử lại). | Danh sách rỗng, chưa có cấu hình/mẫu, DLQ trống, chưa có `Lần thử gửi`. | Không để vùng nội dung trống hoàn toàn; thông điệp phải hữu ích theo ngữ cảnh màn hình. |
| **Loading skeleton** | Khung xương phản ánh bố cục thực (bảng, thẻ, form chi tiết), tránh layout shift lớn khi dữ liệu về. | Tải trang, đổi filter, phân trang, mở chi tiết, submit chờ phản hồi. | Ưu tiên skeleton cho dữ liệu; spinner chỉ dùng cho hành động cục bộ ngắn (nút/inline). |
| **Badge trạng thái** | Hiển thị nhãn trạng thái chuẩn; màu theo semantic token. | `Trạng thái thông báo` (`Pending`/`Processing`/`Sent`/`Failed`), `Trạng thái đọc` (`Unread`/`Read`) khi hiển thị. | Luôn kèm nhãn văn bản đúng glossary; không chỉ dùng màu. |

## 6. Accessibility tối thiểu
Mức tối thiểu bắt buộc cho `AdminUI` MVP:

- **Tương phản:** Văn bản và thành phần UI đạt tương phản tối thiểu dễ đọc (mục tiêu tương đương WCAG AA cho text thông thường; không dùng màu làm tín hiệu duy nhất cho trạng thái/lỗi).
- **Keyboard navigation:** Tất cả chức năng chính thao tác được bằng bàn phím (Tab/Shift+Tab/Enter/Space/ESC), thứ tự focus hợp lý, có trạng thái focus rõ (`--color-focus-ring`), modal có focus trap.
- **Label & form:** Mọi input/select/textarea có label liên kết rõ; lỗi validation được mô tả bằng văn bản và gắn với trường; nhóm trường liên quan có ngữ cảnh (fieldset/legend khi phù hợp).
- **Ảnh & icon:** Ảnh mang ý nghĩa có alt text phù hợp; icon trang trí ẩn khỏi trợ năng, icon chức năng có nhãn truy cập (aria-label/text tương đương).
- **Cấu trúc & phản hồi:** Heading theo thứ bậc logic, bảng có header đúng ngữ nghĩa, thông báo lỗi/thành công quan trọng được công bố theo cách trợ năng có thể nhận biết (live region khi phù hợp).
- **Kích thước tương tác:** Vùng nhấn/chạm và khoảng cách phần tử đủ lớn để thao tác chính xác trên desktop/tablet.

## 7. Content/tone of voice trong UI copy
- **Ngôn ngữ:** Tiếng Việt (theo `docs/prd.md` và đối tượng `Quản trị viên hệ thống`, `Nhân viên vận hành`).
- **Giọng điệu:** Chuyên nghiệp, rõ ràng, trung tính; ưu tiên câu ngắn, mô tả đúng trạng thái và hướng dẫn hành động tiếp theo.
- **Xưng hô:** Dùng cách diễn đạt trung tính trong nhãn/nút/thông báo (ví dụ "Đăng nhập", "Gửi lại", "Không tải được dữ liệu"); tránh xưng hô cá nhân ("bạn", "anh/chị") trừ khi thật sự cần trong thông điệp hướng dẫn dài (mặc định: không dùng).
- **Label & thuật ngữ:** Mọi danh từ nghiệp vụ trong UI copy phải khớp nguyên văn thuật ngữ hiển thị trong `docs/domain-glossary.md` (ví dụ `Thông báo`, `Mẫu thông báo`, `Hàng đợi thư chết`, `Gửi lại`, `Nhật ký gửi thông báo`, `Lần thử gửi`, `Trạng thái thông báo`). Không tự dịch/đổi tên (không dùng "tin nhắn", "template", "retry tay" thay cho thuật ngữ đã chốt).
- **Thông báo lỗi:** Nêu vấn đề + nguyên nhân khả dĩ (nếu biết) + hành động gợi ý (ví dụ "Không tải được danh sách `Thông báo`. Vui lòng thử lại."). Không lộ chi tiết kỹ thuật nhạy cảm, token, hay PII.
- **Thông báo thành công:** Xác nhận hành động đã hoàn tất và đối tượng tác động (ví dụ "Đã gửi yêu cầu `Gửi lại` cho mục đã chọn.").
- **Trạng thái rỗng:** Giải thích vì sao rỗng và gợi ý bước tiếp theo (đổi bộ lọc, tạo `Mẫu thông báo`, kiểm tra `Hàng đợi thư chết`).
- **Phạm vi áp dụng:** Quy tắc này áp dụng cho `AdminUI` và mọi UI copy do dự án kiểm soát; client hiển thị `Thông báo trong ứng dụng` cho `Người dùng cuối` (ngoài `AdminUI`) cũng phải dùng cùng thuật ngữ nghiệp vụ khi hiển thị trạng thái/nội dung liên quan (`Unread` (chưa đọc), `Read` (đã đọc)).

## 8. Responsive rule
`AdminUI` là Angular SPA dùng chủ yếu trên desktop cho vận hành/quản trị, nhưng phải hỗ trợ tối thiểu đa kích thước theo breakpoints mục 4.4 **[ĐỀ XUẤT - CẦN DUYỆT]**:

| Khu vực | Desktop (`>= 1024px`) | Tablet (`768px – 1023px`) | Mobile (`< 768px`) |
|---|---|---|---|
| Bố cục chung | Điều hướng bên (sidebar) + vùng nội dung; bảng và bộ lọc hiển thị đầy đủ. | Sidebar thu gọn/có thể ẩn (drawer); nội dung ưu tiên theo chiều dọc. | Điều hướng dạng menu/drawer; nội dung xếp chồng một cột, thao tác chính vẫn truy cập được. |
| Bảng dữ liệu | Hiển thị nhiều cột, phân trang và bộ lọc trên cùng. | Giảm/ưu tiên cột quan trọng, cho phép cuộn ngang trong vùng bảng nếu cần. | Ưu tiên cột chính (ID/trạng thái/thời gian), ẩn cột phụ sau "Xem chi tiết"; hỗ trợ cuộn ngang hoặc chế độ thẻ tóm tắt cho danh sách dài. |
| Form & cấu hình | Form nhiều cột khi phù hợp, khu vực hành động cố định/rõ ràng. | Chuyển dần về một cột, nhóm trường rõ. | Một cột, nút hành động full-width hoặc dễ chạm, giữ thứ tự label-input hợp lý. |
| Modal & thao tác nhạy cảm | Modal giữa màn hình, nội dung gọn. | Modal co giãn theo chiều rộng, vẫn giữ nút xác nhận/hủy rõ. | Modal gần full-screen khi cần; nút hành động dễ chạm, không gây bấm nhầm. |
| Trạng thái loading/empty/error | Hiển thị trong đúng ngữ cảnh khối/bảng. | Giữ nguyên nguyên tắc, điều chỉnh bố cục. | Giữ nguyên nguyên tắc, nội dung thông điệp không bị cắt mất. |

> Yêu cầu tối thiểu: không mất khả năng truy cập các luồng MVP (`Đăng nhập`, tra cứu `Thông báo`, xem `Hàng đợi thư chết`, `Gửi lại`, quản lý cấu hình) trên tablet; mobile hỗ trợ xem/tra cứu và thao tác thiết yếu ở mức chấp nhận được.
