# Decision Log — Nhật ký quyết định

## 1. Mục đích file
File này dùng để **tra cứu lý do ra quyết định** ("tại sao lúc đó lại chọn thế") khi quay lại dự án sau này.

- **KHÔNG** dùng để mô tả trạng thái hiện tại của dự án (xem `docs/status.md`).
- **KHÔNG** dùng để mô tả kiến trúc hệ thống (xem `docs/architecture.md`).

## 2. Quy tắc khi thêm 1 quyết định mới
Mỗi quyết định là một entry riêng trong mục **Log** (heading con để dễ tra cứu/link). Entry **bắt buộc** theo format sau:

- **ID & Ngày (hoặc STT):** Ví dụ `DEC-001` và ngày `YYYY-MM-DD` (hoặc STT tăng dần nếu không dùng ngày).
- **Bối cảnh (Context):** Vấn đề gì cần quyết định, tại sao phải quyết định vào thời điểm đó.
- **Các lựa chọn đã xem xét (Options considered):** Liệt kê các phương án đã cân nhắc, **kể cả phương án không chọn**, và lý do loại từng phương án.
- **Quyết định (Decision):** Chọn phương án nào — nêu rõ ràng trong 1 câu.
- **Lý do (Rationale):** Vì sao chọn phương án này; gắn với NFR/ràng buộc cụ thể trong `docs/prd.md` hoặc `docs/architecture.md` (nếu có), hoặc với mục tiêu/ràng buộc dự án.
- **Đánh đổi/Hệ quả (Trade-off):** Những gì bị mất, rủi ro phát sinh, hoặc chi phí phải chấp nhận khi chọn hướng này.
- **Trạng thái (Status):** `Active` hoặc `Superseded`.
  - Nếu có quyết định mới thay thế, **KHÔNG xóa** entry cũ: đổi `Status` thành `Superseded` và ghi rõ trỏ tới entry mới (ví dụ: `Superseded by DEC-00X`).
  - Entry thay thế nên tham chiếu ngược lại entry cũ trong phần Bối cảnh/Lý do khi phù hợp.

### Mẫu entry
```md
### DEC-XXX — <Tiêu đề ngắn gọn>
- **Ngày:** YYYY-MM-DD
- **Bối cảnh (Context):** ...
- **Các lựa chọn đã xem xét (Options considered):**
  - **Phương án A:** ... — *Loại vì:* ...
  - **Phương án B:** ... — *Chọn/Loại vì:* ...
- **Quyết định (Decision):** ...
- **Lý do (Rationale):** ... (tham chiếu `docs/prd.md`/`docs/architecture.md`: NFR/FR/ràng buộc liên quan nếu có)
- **Đánh đổi/Hệ quả (Trade-off):** ...
- **Trạng thái (Status):** Active
```

## 3. Ngưỡng khi nào cần ghi vào đây

### PHẢI ghi (bắt buộc)
- Chọn công nghệ/thư viện/framework/hạ tầng chính (ví dụ message broker, ORM, auth, observability stack, provider tích hợp).
- Thay đổi kiến trúc hoặc ranh giới module/lớp quan trọng (ví dụ thay đổi pattern, luồng xử lý, chiến lược lưu trữ/tích hợp).
- Thay đổi convention/quy ước đã thống nhất có ảnh hưởng đến cách viết code hoặc cấu trúc dự án.
- Chọn giải pháp cho một vấn đề có **nhiều cách làm hợp lý** (trade-off không hiển nhiên).
- Cố ý **không** áp dụng một best practice/common approach vì ràng buộc thực tế (thời gian, chi phí, phạm vi MVP, độ phức tạp, môi trường).

### KHÔNG cần ghi
- Tên biến/hàm/class cục bộ, cách đặt tên chi tiết không ảnh hưởng người khác.
- Format/style code đã được công cụ hoặc convention chung xử lý tự động.
- Chi tiết triển khai nhỏ, thuần cục bộ, không tạo precedent và không ảnh hưởng quyết định/thiết kế của người khác.

> Nguyên tắc: nếu người khác (hoặc chính bạn trong tương lai) có thể hỏi "vì sao lại làm như vậy?" và câu trả lời không hiển nhiên từ code/PRD/architecture hiện tại, hãy ghi vào đây.

## 4. Log

### DEC-001 — Quản lý user & credential tập trung tại `IdentityAccess` (Cách 1), sẵn sàng tích hợp IdP
- **Ngày:** 2026-04-03
- **Bối cảnh (Context):** Cần bổ sung chức năng quản lý user (tài khoản, vai trò, credential, hồ sơ nhận in-app) ngoài phạm vi PRD/status hiện tại; cần chọn ranh giới module và hướng phát triển lâu dài (IdP).
- **Các lựa chọn đã xem xét (Options considered):**
  - **Cách 1 — Tập trung trong `IdentityAccess`:** quản lý tài khoản/credential/phát `JWT`/reset mật khẩu và hồ sơ `EndUser` trong cùng module, `AdminUI` quản trị tập trung — *Chọn vì:* triển khai trọn bộ nhanh, nhất quán, thuận lợi tích hợp IdP sau.
  - **Cách 2 — Tách theo trách nhiệm:** `IdentityAccess` cho tài khoản/credential/`JWT`, module `Notification` cho hồ sơ `EndUser` — *Loại vì:* tách luồng dữ liệu/trải nghiệm quản trị, phối hợp nhiều hơn trong giai đoạn hiện tại.
- **Quyết định (Decision):** Tập trung quản lý user/credential và hồ sơ `EndUser` trong module `IdentityAccess` (Cách 1), với contract rõ để `Notification`/`InAppNotification` tham chiếu khi cần.
- **Lý do (Rationale):** Phù hợp kiến trúc Modular Monolith 1 developer, giảm trùng lặp và chuẩn bị điểm mở rộng IdP; bám mục tiêu bảo mật/phân quyền (NFR-05, FR-16, FR-01) trong `docs/prd.md` và ranh giới module trong `docs/architecture.md`.
- **Đánh đổi/Hệ quả (Trade-off):** `IdentityAccess` mở rộng trách nhiệm (bao gồm hồ sơ `EndUser` gắn nghiệp vụ nhận in-app), cần giữ contract/port rõ với module `Notification` để tránh coupling không kiểm soát.
- **Trạng thái (Status):** Active

### DEC-002 — Quản lý credential trong app (hiện tại), hỗ trợ tích hợp IdP sau
- **Ngày:** 2026-04-03
- **Bối cảnh (Context):** Hệ thống cần cơ chế xác thực/quản trị tài khoản ngay trong MVP/mở rộng hiện tại, nhưng có nhu cầu tích hợp IdP/SSO trong tương lai.
- **Các lựa chọn đã xem xét (Options considered):**
  - **Tự quản lý credential trong app:** lưu/hash credential, cấp/phát token trong hệ thống — *Chọn vì:* đáp ứng nhu cầu hiện tại, chủ động vận hành.
  - **Tích hợp IdP/SSO ngay:** ủy thác danh tính/xác thực cho IdP — *Loại (hiện tại) vì:* chưa phải ưu tiên triển khai ngay; vẫn giữ làm hướng mở rộng.
- **Quyết định (Decision):** Hiện tại quản lý credential trong ứng dụng (DB/hash/luồng nội bộ), thiết kế abstraction để có thể tích hợp IdP/SSO về sau.
- **Lý do (Rationale):** Đáp ứng phạm vi và tiến độ mong muốn; tương thích định hướng bảo mật `JwtBearer`/`RBAC` (NFR-05) trong `docs/prd.md`.
- **Đánh đổi/Hệ quả (Trade-off):** Tăng phạm vi phát triển/vận hành bảo mật (lưu trữ, reset, rotate secret); cần thiết kế điểm mở rộng để giảm chi phí chuyển đổi sang IdP.
- **Trạng thái (Status):** Active

### DEC-003 — `IntegrationClient` dùng client credentials do app quản lý để phát `JWT`
- **Ngày:** 2026-04-03
- **Bối cảnh (Context):** `IntegrationClient` cần cơ chế truy cập API an toàn; hệ thống đã có xác thực/validation `JwtBearer`.
- **Các lựa chọn đã xem xét (Options considered):**
  - **Client credentials (client id/secret) → phát `JWT`:** hệ thống quản lý credential và cấp token — *Chọn vì:* thống nhất với `JwtBearer`/`RBAC` hiện có, thuận lợi quản lý và mở rộng.
  - **API key/secret dài hạn gửi trực tiếp:** xác thực bằng khóa tĩnh — *Loại vì:* khác mô hình token hiện tại, khó chuẩn hóa claim/role và vòng đời như `JWT`.
- **Quyết định (Decision):** `IntegrationClient` sử dụng client id/secret do ứng dụng quản lý (cấp/rotate/vô hiệu hóa) để lấy `JWT` truy cập API.
- **Lý do (Rationale):** Tái sử dụng hạ tầng `JwtBearer` (ST-P1-01) và mô hình `RBAC` (NFR-05, FR-01) trong `docs/prd.md`/`docs/architecture.md`.
- **Đánh đổi/Hệ quả (Trade-off):** Cần cơ chế lưu hash/rotate secret và endpoint cấp token an toàn; phải kiểm soát rò rỉ secret và thu hồi hiệu lực.
- **Trạng thái (Status):** Active

### DEC-004 — Đặt lại mật khẩu tự phục vụ qua link/token gửi email
- **Ngày:** 2026-04-03
- **Bối cảnh (Context):** Tài khoản `SystemAdministrator`/`BusinessOperator` cần cơ chế khôi phục truy cập khi quên mật khẩu.
- **Các lựa chọn đã xem xét (Options considered):**
  - **Tự đặt lại qua link/token gửi email:** người dùng yêu cầu và hoàn tất đặt lại bằng token một lần — *Chọn vì:* tự phục vụ, giảm phụ thuộc thao tác thủ công.
  - **Quản trị viên đặt mật khẩu mới/trả mật khẩu tạm trực tiếp:** xử lý thủ công trong app — *Loại vì:* không đáp ứng nhu cầu tự phục vụ đã chọn.
- **Quyết định (Decision):** Triển khai luồng đặt lại mật khẩu tự phục vụ bằng link/token (một lần, có hạn, gửi email) cho tài khoản quản trị/vận hành đăng nhập `AdminUI`.
- **Lý do (Rationale):** Tăng khả năng vận hành và trải nghiệm người dùng `AdminUI`; tận dụng hạ tầng email trong kiến trúc (FR-11/NFR-10, `docs/architecture.md`, `docs/deploy.md`).
- **Đánh đổi/Hệ quả (Trade-off):** Phụ thuộc cấu hình gửi email và cần kiểm soát bảo mật (token ngẫu nhiên, TTL, chống lạm dụng, không lộ thông tin tài khoản).
- **Trạng thái (Status):** Active

### DEC-005 — Hồ sơ `EndUser` là thực thể lưu trong hệ thống, khóa theo định danh hệ thống nguồn
- **Ngày:** 2026-04-03
- **Bối cảnh (Context):** Cần khả năng quản lý người nhận `EndUser` cho `InAppNotification`; đồng thời phải xác định nguồn định danh và mô hình lưu trữ.
- **Các lựa chọn đã xem xét (Options considered):**
  - **Chỉ tham chiếu định danh từ hệ thống nguồn (không lưu hồ sơ quản trị):** nhẹ, tránh danh bạ nội bộ — *Loại (theo lựa chọn cuối) vì:* cần CRUD tạo/sửa/khóa hồ sơ `EndUser` trong hệ thống.
  - **Lưu hồ sơ `EndUser` trong hệ thống, khóa theo định danh nguồn:** có thực thể quản trị nhưng giữ liên kết nguồn — *Chọn vì:* đáp ứng yêu cầu quản lý và vẫn nhất quán với định danh từ hệ thống nguồn.
  - **Tự sinh id nội bộ làm khóa chính:** độc lập định danh — *Loại vì:* yêu cầu dùng định danh hệ thống nguồn làm khóa/liên kết chính.
- **Quyết định (Decision):** Quản lý hồ sơ `EndUser` như thực thể lưu trong hệ thống (tạo/sửa/khóa/mở khóa), sử dụng định danh từ hệ thống nguồn làm khóa chính/liên kết nghiệp vụ.
- **Lý do (Rationale):** Đáp ứng phạm vi quản lý được yêu cầu và hỗ trợ nghiệp vụ nhận `InAppNotification` (FR-12) trong `docs/prd.md`/`docs/architecture.md`, giữ truy vết với hệ thống nguồn.
- **Đánh đổi/Hệ quả (Trade-off):** Phát sinh dữ liệu hồ sơ nội bộ cần quản lý vòng đời/đồng bộ kỳ vọng với nguồn; cần quy ước rõ khi `EndUser` bị khóa ảnh hưởng luồng nhận/hiển thị in-app.
- **Trạng thái (Status):** Active
