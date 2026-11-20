# Thiết kế: Quản lý User (IdentityAccess, in-app credential)

> Trạng thái: **Nháp - chờ Đại ca duyệt**. Phạm vi bổ sung ngoài `docs/prd.md`/`docs/status.md` hiện tại; sau khi duyệt sẽ cập nhật PRD/glossary/architecture/ux/conventions/status/decisions tương ứng.

## 1. Mục tiêu & phạm vi

- **Mục tiêu:** Bổ sung quản lý user/credential trong ứng dụng (sẵn sàng tích hợp IdP sau), gồm tài khoản quản trị/tích hợp và hồ sơ người nhận in-app.
- **Trong phạm vi (Cách 1 - tập trung `IdentityAccess`):**
  - Quản lý tài khoản `SystemAdministrator`, `BusinessOperator`, `IntegrationClient`: tạo, sửa, khóa/mở khóa, xóa (theo chính sách), gán vai trò.
  - `IntegrationClient`: quản lý client id/secret (cấp mới/rotate/vô hiệu hóa) và dùng client credentials để hệ thống phát `JWT` truy cập API.
  - `SystemAdministrator`/`BusinessOperator`: đăng nhập `AdminUI` bằng credential nội bộ; tự đặt lại mật khẩu qua luồng link/token (gửi email).
  - Hồ sơ `EndUser` (người nhận in-app): tạo, sửa, khóa/mở khóa như thực thể lưu trong hệ thống, **khóa chính nghiệp vụ = định danh từ hệ thống nguồn** (external id).
- **Ngoài phạm vi hiện tại:** Tích hợp IdP/SSO (chuẩn bị điểm mở rộng), tự phục vụ/profile nâng cao cho `EndUser`, đồng bộ hai chiều với hệ thống nguồn.

## 2. Nguyên tắc & ranh giới module

- **Module sở hữu:** `IdentityAccess` (Domain/Application/Infrastructure/Presentation) sở hữu danh tính, credential, phát hành `JWT`, token đặt lại mật khẩu, và hồ sơ `EndUser` theo Cách 1.
- **Tương tác với module khác:** `Notification`/`InAppNotification` tra cứu/tham chiếu `EndUser` qua định danh nguồn (contract/read model hoặc port do `IdentityAccess` cung cấp) để phục vụ nhận/hiển thị in-app; không nhân bản logic credential.
- **Thuật ngữ:** Tuân thủ `docs/domain-glossary.md` (`SystemAdministrator`, `BusinessOperator`, `IntegrationClient`, `EndUser`, `JwtBearer`, `RBAC`, `AdminUI`, `InAppNotification`, `Notification`). Bổ sung thuật ngữ mới (nếu cần) vào glossary khi cập nhật docs.
- **IdP sau này:** Tách lớp phát hành/xác thực credential nội bộ qua abstraction (application service/port) để có thể thay bằng IdP mà không đổi contract nghiệp vụ cốt lõi.

## 3. Mô hình khái niệm (đề xuất)

- **UserAccount** (tài khoản có đăng nhập/quản trị hoặc tích hợp), gợi ý thuộc tính: `Id`, `UserName`/`Email` (unique theo quy ước), `DisplayName`, `Role` (`SystemAdministrator`|`BusinessOperator`|`IntegrationClient`), `Status` (Active/Locked/Deleted-soft nếu áp dụng), `PasswordHash` (cho tài khoản human), audit (`CreatedAt`, `UpdatedAt`...), metadata cần thiết.
- **IntegrationClientCredential** (gắn `UserAccount` role `IntegrationClient`): `ClientId` (unique), `ClientSecretHash` (lưu hash, không lưu plaintext), trạng thái/`ExpiresAt` (nếu có), thời điểm rotate/vô hiệu hóa; hỗ trợ cấp mới/rotate.
- **PasswordResetToken**: token ngẫu nhiên an toàn (lưu hash), gắn tài khoản, `ExpiresAt`, `UsedAt`/trạng thái, mục đích một lần (one-time).
- **EndUserProfile** (hồ sơ nhận in-app): `ExternalUserId` (**khóa chính/định danh nghiệp vụ** từ hệ thống nguồn), thông tin hiển thị/liên hệ tối thiểu cần cho in-app/quản trị (ví dụ tên, email/phone nếu cần), `Status` (Active/Locked), audit. Không lưu credential đăng nhập trừ khi mở rộng sau.

> Tên bảng/C# type cụ thể sẽ chuẩn hóa theo `docs/conventions.md` và `docs/domain-glossary.md` khi lập plan/implement (truy vết thuật ngữ chuẩn).

## 4. Luồng chính

### 4.1 Quản lý tài khoản & vai trò (RBAC)
- Chỉ `SystemAdministrator` (hoặc policy tương đương) được tạo/sửa/khóa/xóa/gán vai trò tài khoản quản trị & `IntegrationClient` qua `AdminUI`/API bảo vệ `RBAC`.
- Khóa tài khoản: từ chối đăng nhập/phát token mới; token đã phát xử lý theo TTL/chiến lược thu hồi (tối thiểu không cấp mới khi locked).
- Xóa: ưu tiên soft-delete/khóa vĩnh viễn nếu cần giữ audit; quyết định chi tiết khi implement (ghi `docs/decisions.md` nếu phát sinh).

### 4.2 Xác thực & phát `JWT`
- **`SystemAdministrator`/`BusinessOperator`:** đăng nhập bằng credential nội bộ → hệ thống xác thực và phát `JWT` theo cấu hình `JWT_*` hiện có (issuer/audience/signing key/lifetime) cho `AdminUI`.
- **`IntegrationClient`:** gửi client credentials (client id/secret) tới endpoint cấp token → hệ thống xác thực secret (so hash) và phát `JWT` chứa claims/role phù hợp `RBAC`. Hỗ trợ rotate secret (giữ thời gian ân hạn ngắn nếu cần, quyết định khi implement).
- Tiếp tục dùng `JwtBearer` validation như ST-P1-01; mở rộng claim/role mapping theo ST-P1-02/P1-03.

### 4.3 Đặt lại mật khẩu tự phục vụ (link/token)
- Người dùng yêu cầu đặt lại (cung cấp email/định danh theo thiết kế) → hệ thống tạo `PasswordResetToken` một lần, có hạn, gửi link/token qua email.
- Người dùng mở link/gửi token + mật khẩu mới → hệ thống xác thực token (còn hạn, chưa dùng), cập nhật `PasswordHash`, đánh dấu token đã dùng (vô hiệu token cũ liên quan nếu có).
- **Phụ thuộc:** hạ tầng gửi email (`EmailProvider`/SMTP, Mailpit dev) và cấu hình URL `AdminUI` cho link; cần xử lý an toàn (không lộ tồn tại tài khoản theo ngữ cảnh phù hợp, rate limit cơ bản khi implement).

### 4.4 Quản lý hồ sơ `EndUser` (in-app, khóa = external id)
- CRUD quản trị (tạo/sửa/khóa/mở khóa) hồ sơ `EndUser` qua `AdminUI`/API bảo vệ `RBAC`.
- `ExternalUserId` từ hệ thống nguồn là định danh duy nhất; tạo trùng phải được xử lý rõ ràng (idempotent/conflict).
- Trạng thái khóa ảnh hưởng việc nhận/hiển thị in-app theo nghiệp vụ `Notification` (cần quy ước cụ thể khi thiết kế chi tiết/implement, phối hợp với module `Notification`).

## 5. API & `AdminUI` (dự kiến)

- **API (IdentityAccess, bảo vệ RBAC phù hợp):** quản lý tài khoản/role; quản lý `IntegrationClient` credential (create/rotate/revoke); endpoint client-credentials token; login/logout cho `AdminUI` (bổ sung bên cạnh JWT validation); yêu cầu/hoàn tất đặt lại mật khẩu; quản lý hồ sơ `EndUser`.
- **`AdminUI`:** màn hình đăng nhập (ST-P1-04) mở rộng cho credential nội bộ; khu quản trị user/tài khoản & `IntegrationClient`; khu quản trị hồ sơ `EndUser`; luồng quên/đặt lại mật khẩu.
- Chi tiết route, contract, validation, phân quyền từng endpoint/màn hình sẽ cụ thể hóa trong story/plan (bám `docs/ux.md`, `docs/conventions.md`).

## 6. Bảo mật & vận hành

- Không lưu plaintext mật khẩu/secret/token reset; dùng hash phù hợp (ví dụ password hashing mạnh, secret/token hash an toàn).
- Token reset: ngẫu nhiên, TTL ngắn, một lần, chống dò đoán/bruteforce cơ bản.
- Audit log cho thao tác quản trị nhạy cảm (tạo/khóa/xóa/đổi vai trò/rotate secret/reset) theo khả năng `Observability`/quy ước dự án.
- Không hardcode secret; cấu hình qua env/appsettings theo `docs/deploy.md`.

## 7. Ảnh hưởng tài liệu & kế hoạch

- Cập nhật: `docs/prd.md` (yêu cầu quản lý user/credential/reset), `docs/domain-glossary.md` (thuật ngữ mới nếu có), `docs/architecture.md` (IdentityAccess mở rộng, phát JWT, luồng reset), `docs/ux.md` (màn hình/luồng), `docs/conventions.md` (quy ước nếu cần), `docs/deploy.md` (biến cấu hình email/reset URL nếu thêm), `docs/status.md` (stories/phase/dependency/blocker), `docs/decisions.md` (quyết định Cách 1, in-app credential, client credentials, reset flow, EndUser keyed external id).
- Đề xuất bổ sung stories (tách nhỏ, có thể điều chỉnh khi lập plan): quản lý tài khoản/role + lưu trữ credential; phát JWT cho login & client credentials; luồng quên/đặt lại mật khẩu + email; quản lý hồ sơ `EndUser` + liên kết `Notification`; API/UI/AdminUI tương ứng; migration/schema; test unit/integration/e2e theo DoD.

## 8. Tiêu chí nghiệm thu cấp thiết kế

- Phạm vi và luồng khớp xác nhận của Đại ca (Cách 1, in-app credential, client credentials→JWT, self-service reset, `EndUser` entity keyed external id, triển khai tất cả).
- Khi duyệt: chuyển sang cập nhật docs + `writing-plans` để chia story/task khả thi và tuân thủ DoD (unit+integration+e2e pass mới tick).
