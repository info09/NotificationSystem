# Status — Kế hoạch & Tiến độ dự án (Notification System)

> **Nguồn sự thật duy nhất về tiến độ dự án.** File này tổng hợp từ `docs/prd.md`, `docs/domain-glossary.md`, `docs/architecture.md`, `docs/ux.md`, `docs/deploy.md`, `docs/conventions.md`, `docs/decisions.md`. Tên phase/hạng mục/story và thuật ngữ nghiệp vụ/UI phải dùng đúng `docs/domain-glossary.md`.

## 1. Tổng quan phase

| Phase | Tên phase | Mục tiêu | Phụ thuộc chính |
|---|---|---|---|
| 0 | Hạ tầng, CI/CD & khung mã nguồn | Dựng nền tảng triển khai (Docker Compose, GitHub Actions, cấu trúc Clean Architecture theo module, scaffold `AdminUI`) trước mọi phase triển khai tính năng/deploy thật | `docs/deploy.md`, `docs/conventions.md`, `docs/architecture.md` |
| 1 | `IdentityAccess`: `JwtBearer`, `RBAC` & `AdminUI` `Đăng nhập`/`Đăng xuất` | Thiết lập xác thực/phân quyền nền tảng trước các tính năng cần bảo vệ | Phase 0 |
| 2 | Nền tảng `Cơ sở dữ liệu`, `HealthCheck` & `Observability` | Thiết lập lưu trữ, migration, health/readiness, logging/tracing nền tảng | Phase 0, 1 |
| 3 | Module `Notification`: `CreateNotificationRequest`, `IdempotencyKey` & tra cứu `Thông báo` | Tạo/tiếp nhận và quản lý vòng đời `Notification` ở mức API + truy vấn vận hành cơ bản | Phase 1, 2 |
| 4 | `IntegrationMessaging`: `TransactionalOutbox`, `OutboxMessage` & `RabbitMQ` publish | Đảm bảo publish `Message` nhất quán DB–broker | Phase 2, 3 |
| 5 | `NotificationWorker`, `InboxPattern`/`InboxMessage` & consume | Xử lý nền idempotent, chuyển `NotificationStatus`, concurrency | Phase 4 |
| 6 | `Retry`, `ExponentialBackoff`, `DeadLetterQueue` & `Resend` | Chịu lỗi tạm thời, DLQ và luồng gửi lại an toàn | Phase 5 |
| 7 | Kênh `Email` & `Thông báo trong ứng dụng` (`InAppNotification`) | Dispatch đa kênh MVP, in-app pull/polling và `ReadStatus` | Phase 5, 6 |
| 8 | `NotificationDeliveryLog`, `DeliveryAttempt` & truy vết | Ghi nhận lịch sử/lần thử gửi và phục vụ tra cứu điều tra | Phase 5, 6, 7 |
| 9 | `AdminUI` `Tổng quan` (`Dashboard`) & hoàn thiện vận hành | Hiển thị tổng quan/health/tình trạng và hoàn thiện trải nghiệm tra cứu/vận hành theo `docs/ux.md` | Phase 1, 2, 3, 6, 8 |
| 10 | Cấu hình quản trị: `Mẫu thông báo`, `Cấu hình kênh thông báo`, `Định tuyến sự kiện tích hợp` | Cung cấp cấu hình quản trị CRUD và UI tương ứng | Phase 1, 2, 3 |
| 11 | `IntegrationEvent` end-to-end & `AuditLog` | Tiếp nhận `IntegrationEvent`, định tuyến cấu hình, ghi `AuditLog` thao tác nhạy cảm | Phase 3, 4, 5, 10 |
| 12 | Hardening, kiểm thử hiệu năng/tải & quy trình deploy staging/production | Đạt mục tiêu NFR/metric MVP, xác minh pipeline và quy trình deploy/rollback | Phase 0–11 |
| 13 | Quản lý user & credential (`IdentityAccess`, `EndUser`) | Quản lý tài khoản/vai trò/credential trong app, phát `JwtBearer` (login & client credentials), tự đặt lại mật khẩu, quản lý hồ sơ `EndUser` nhận in-app | Phase 1, 2 (hạ tầng gửi email cho reset mật khẩu; liên quan Phase 7) |

## 2. Chi tiết phase & stories

> **Quy ước mã story:** `ST-P{phase}-{số}` (ví dụ `ST-P0-01`). Mỗi story là đơn vị việc đủ nhỏ để hoàn thành trong 1 prompt.
>
> **Quy ước tick:** Chỉ tick `- [x]` khi story đã có **unit test + integration test + e2e test liên quan đều pass** theo `docs/conventions.md` (theo phạm vi liên quan của story). Không tick nếu mới chỉ "chạy được" hoặc thiếu tầng test liên quan.

### Phase 0 — Hạ tầng, CI/CD & khung mã nguồn
- **Mục tiêu:** Thiết lập khung dự án Modular Monolith (mỗi module backend là Clean Architecture), môi trường local Docker Compose, template cấu hình/secret an toàn, và CI/CD GitHub Actions có gate test trước deploy (theo `docs/deploy.md`).
- **FR/NFR liên quan:** NFR-08, NFR-10 (nền tảng); hỗ trợ toàn bộ FR/NFR về khả năng triển khai/kiểm thử.
- **Stories:**
  - [x] **ST-P0-01** — Khởi tạo cấu trúc thư mục/solution `src`, `tests`, `docker`, `docs`, `.github/workflows` theo `docs/conventions.md` (skeleton build được).
  - [x] **ST-P0-02** — Khởi tạo module backend theo Clean Architecture (`Domain/Application/Infrastructure/Presentation`) cho `Notification`, `TemplateConfiguration`, `IntegrationMessaging`, `Delivery`, `Administration`, `IdentityAccess` và `BuildingBlocks`.
  - [x] **ST-P0-03** — Khởi tạo Hosts `Api`, `Worker` và scaffold `AdminUI` (Angular SPA) theo cấu trúc chuẩn; cấu hình tham chiếu/DI cơ bản.
  - [x] **ST-P0-04** — Thiết lập `docker-compose` local/dev cho `postgres`, `rabbitmq`, `mailpit`, `api`, `worker`, `admin-ui` (skeleton chạy được, network/volumes cơ bản).
  - [x] **ST-P0-05** — Tạo `.env.example` và cơ chế nạp cấu hình local cho các biến bắt buộc theo `docs/deploy.md` (không commit secret).
  - [x] **ST-P0-06** — Thiết lập khung `tests/UnitTests`, `tests/IntegrationTests`, `tests/E2ETests` (bao gồm cấu hình Testcontainers/Docker cho integration khi áp dụng) và lệnh chạy test chuẩn.
  - [x] **ST-P0-07** — Thiết lập GitHub Actions CI: build + lint/format cơ bản + gate `unit → integration → e2e`, fail thì dừng pipeline.
  - [x] **ST-P0-08** — Xác minh pipeline và môi trường local chạy pass (build, compose smoke, test scaffold các tầng liên quan) theo quy ước tick.

### Phase 1 — `IdentityAccess`: `JwtBearer`, `RBAC` & `AdminUI` `Đăng nhập`/`Đăng xuất`
- **Mục tiêu:** Bảo vệ API/AdminUI bằng `JwtBearer` + `RBAC` (`SystemAdministrator`, `BusinessOperator`, `IntegrationClient`) và cung cấp luồng `Đăng nhập`/`Đăng xuất` trên `AdminUI`.
- **FR/NFR liên quan:** FR-01, FR-06, FR-09, FR-16, NFR-05.
- **UI (theo `docs/ux.md`):** Màn hình `Đăng nhập` / `Login`, hành động `Đăng xuất` / `Logout`, guard/điều hướng theo vai trò.
- **Stories:**
  - [x] **ST-P1-01** — Triển khai xác thực `JwtBearer` trong module `IdentityAccess` (cấu hình issuer/audience/signing key, validate token).
  - [x] **ST-P1-02** — Triển khai `RBAC` cho vai trò `SystemAdministrator`, `BusinessOperator`, `IntegrationClient` và policy/authorization handlers.
  - [x] **ST-P1-03** — Áp dụng bảo vệ endpoint API theo `RBAC` (bao gồm endpoint quản trị nhạy cảm) và chuẩn hóa lỗi 401/403 (ProblemDetails).
  - [x] **ST-P1-04** — Dựng `AdminUI` màn hình `Đăng nhập` / `Login` (form, validation, loading/empty/error/success) và tích hợp luồng xác thực.
  - [x] **ST-P1-05** — Triển khai `Đăng xuất` / `Logout`, quản lý phiên/token phía client an toàn, route guard và điều hướng tới `Tổng quan` / `Dashboard` sau đăng nhập.
  - [x] **ST-P1-06** — Hoàn thiện kiểm thử unit/integration/e2e liên quan luồng xác thực/phân quyền và UI `Đăng nhập`/`Đăng xuất`.

### Phase 2 — Nền tảng `Cơ sở dữ liệu`, `HealthCheck` & `Observability`
- **Mục tiêu:** Thiết lập PostgreSQL/EF Core, migration, `HealthCheck` và `StructuredLog`/`CorrelationId`/OpenTelemetry cơ bản.
- **FR/NFR liên quan:** FR-15, NFR-02, NFR-06, NFR-07, NFR-08.
- **Stories:**
  - [x] **ST-P2-01** — Thiết lập kết nối PostgreSQL, DbContext/EF Core và cấu trúc persistence theo module (Clean Architecture).
  - [x] **ST-P2-02** — Thiết lập chiến lược migration EF Core, transaction cơ bản và quy trình áp dụng trong môi trường local/dev.
  - [x] **ST-P2-03** — Triển khai `HealthCheck` `GET /health` (liveness) và `GET /health/ready` (readiness: API, Database, RabbitMQ).
  - [x] **ST-P2-04** — Triển khai `StructuredLog` (Serilog) với enrichment `CorrelationId` (`messageId`, `notificationId`, trace id) và middleware liên quan.
  - [x] **ST-P2-05** — Tích hợp OpenTelemetry cơ bản cho HTTP, EF Core, worker/RabbitMQ (mức khả thi MVP) và cấu hình theo môi trường.
  - [x] **ST-P2-06** — Hoàn thiện kiểm thử unit/integration/e2e liên quan DB, health, logging/tracing.

### Phase 3 — Module `Notification`: `CreateNotificationRequest`, `IdempotencyKey` & tra cứu `Thông báo`
- **Mục tiêu:** Cho phép `IntegrationClient` tạo `Notification` qua API an toàn/idempotent và cho `Quản trị viên hệ thống`/`Nhân viên vận hành` tra cứu `Thông báo` trong `AdminUI`.
- **FR/NFR liên quan:** FR-01, FR-06, FR-10 (phạm vi tạo yêu cầu), NFR-01, NFR-02, NFR-05, NFR-07.
- **UI (theo `docs/ux.md`):** `Thông báo` / `Notification` — danh sách và chi tiết cơ bản, bộ lọc `Trạng thái thông báo`/`Kênh thông báo`/thời gian.
- **Stories:**
  - [x] **ST-P3-01** — Triển khai domain `Notification` (`NotificationStatus`: `Pending`/`Processing`/`Sent`/`Failed`, `Recipient`, kênh/nội dung hoặc template ref) và quy tắc nghiệp vụ cơ bản.
  - [x] **ST-P3-02** — Triển khai application/use case và persistence cho tạo/truy vấn `Notification` theo Clean Architecture.
  - [x] **ST-P3-03** — Triển khai API tạo `Notification` từ `CreateNotificationRequest` (JWT + RBAC, validation, trả `notificationId`) với xử lý `IdempotencyKey` chống tạo trùng.
  - [x] **ST-P3-04** — Triển khai API truy vấn danh sách/chi tiết `Notification` (lọc trạng thái/kênh/thời gian, phân trang) theo quyền.
  - [x] **ST-P3-05** — Dựng `AdminUI` danh sách `Thông báo` (bảng, bộ lọc, phân trang, loading/empty/error/success).
  - [x] **ST-P3-06** — Dựng `AdminUI` chi tiết `Thông báo` cơ bản (thông tin, `Trạng thái thông báo`, `Mã tương quan`, loading/empty/error/success).
  - [x] **ST-P3-07** — Hoàn thiện kiểm thử unit/integration/e2e liên quan tạo, idempotency, truy vấn và UI `Thông báo`.

### Phase 4 — `IntegrationMessaging`: `TransactionalOutbox`, `OutboxMessage` & `RabbitMQ` publish
- **Mục tiêu:** Bảo đảm tính nhất quán khi phát hành `Message` qua `RabbitMQ` bằng `TransactionalOutbox`/`OutboxMessage`.
- **FR/NFR liên quan:** FR-03, FR-04, NFR-02, NFR-04, NFR-07.
- **Stories:**
  - [x] **ST-P4-01** — Thiết kế/triển khai entity & persistence `OutboxMessage` và tích hợp ghi outbox trong cùng transaction DB với nghiệp vụ.
  - [x] **ST-P4-02** — Cấu hình `RabbitMQ` topology MVP (`notifications.topic`, `notifications.email.q`, `notifications.inapp.q`, DLQ tương ứng, routing keys).
  - [x] **ST-P4-03** — Triển khai Outbox Publisher (BackgroundService) đọc `OutboxMessage` chưa publish, publish `Message` và cập nhật trạng thái an toàn.
  - [x] **ST-P4-04** — Bổ sung cơ chế retry/xử lý lỗi publish outbox theo chính sách an toàn (không mất bản ghi đã commit).
  - [x] **ST-P4-05** — Hoàn thiện kiểm thử unit/integration/e2e liên quan `TransactionalOutbox`, publish và topology `RabbitMQ`.

### Phase 5 — `NotificationWorker`, `InboxPattern`/`InboxMessage` & consume
- **Mục tiêu:** Xử lý `Message` nền với at-least-once + idempotent (`InboxPattern`/`InboxMessage`, `MessageId`), chuyển trạng thái và kiểm soát concurrency.
- **FR/NFR liên quan:** FR-05, FR-06, FR-07, FR-10, NFR-02, NFR-03, NFR-07.
- **Stories:**
  - [x] **ST-P5-01** — Triển khai entity & persistence `InboxMessage`/`InboxPattern` với ràng buộc unique theo `MessageId` và ngữ cảnh xử lý cần thiết.
  - [x] **ST-P5-02** — Triển khai `NotificationWorker` consume `Queue`, kiểm tra inbox/idempotency và `ack` sau xử lý an toàn.
  - [x] **ST-P5-03** — Triển khai luồng cập nhật `NotificationStatus` (`Pending`→`Processing`→`Sent`/`Failed`) trong xử lý nền.
  - [x] **ST-P5-04** — Triển khai giới hạn concurrency (mặc định 5/worker, cấu hình được) và hỗ trợ chạy nhiều worker instance (competing consumers).
  - [x] **ST-P5-05** — Hoàn thiện kiểm thử unit/integration/e2e liên quan consume, idempotency và concurrency.

### Phase 6 — `Retry`, `ExponentialBackoff`, `DeadLetterQueue` & `Resend`
- **Mục tiêu:** Tự động `Retry` lỗi tạm thời, cô lập lỗi vào `DeadLetterQueue` và hỗ trợ `Resend` idempotent từ `AdminUI`.
- **FR/NFR liên quan:** FR-08, FR-09, NFR-02, NFR-04, NFR-05.
- **UI (theo `docs/ux.md`):** `Hàng đợi thư chết` / `DeadLetterQueue` (danh sách, chi tiết) và thao tác `Gửi lại` / `Resend`.
- **Stories:**
  - [x] **ST-P6-01** — Triển khai phân loại lỗi retryable/non-retryable và chính sách `Retry` tối đa 3 lần với `ExponentialBackoff` + jitter.
  - [x] **ST-P6-02** — Triển khai chuyển `Message` vào `DeadLetterQueue` khi vượt ngưỡng/không xử lý được theo chính sách.
  - [x] **ST-P6-03** — Triển khai API truy vấn `DeadLetterQueue` và luồng `Resend` idempotent theo `RBAC` (kèm `AuditLog` thao tác nhạy cảm).
  - [x] **ST-P6-04** — Dựng `AdminUI` danh sách `Hàng đợi thư chết` (lọc/chi tiết tóm tắt, loading/empty/error/success).
  - [x] **ST-P6-05** — Dựng `AdminUI` chi tiết DLQ và thao tác `Gửi lại` (modal xác nhận, trạng thái xử lý, kết quả success/error).
  - [x] **ST-P6-06** — Hoàn thiện kiểm thử unit/integration/e2e liên quan retry, DLQ và `Resend`.

### Phase 7 — Kênh `Email` & `Thông báo trong ứng dụng` (`InAppNotification`)
- **Mục tiêu:** Gửi `Notification` qua `Email` và `InAppNotification` theo abstraction kênh, hỗ trợ pull/polling và `ReadStatus`.
- **FR/NFR liên quan:** FR-11, FR-12, FR-13, NFR-01, NFR-02.
- **Stories:**
  - [x] **ST-P7-01** — Triển khai domain `Email`/abstraction `EmailProvider` và cấu hình `SmtpClient` + Mailpit cho môi trường dev.
  - [x] **ST-P7-02** — Triển khai gửi `Email` qua SMTP (`EmailProvider`, Mailpit dev), hỗ trợ HTML/plain-text fallback và render nội dung/template.
  - [x] **ST-P7-03** — Triển khai lưu `InAppNotification` và API pull/polling (danh sách, chi tiết, số chưa đọc) cho `EndUser`.
  - [x] **ST-P7-04** — Triển khai cập nhật `ReadStatus` (`Unread`/`Read`) và API đánh dấu đã đọc.
  - [x] **ST-P7-05** — Tích hợp cập nhật kết quả dispatch kênh vào luồng worker/trạng thái xử lý.
  - [x] **ST-P7-06** — Hoàn thiện kiểm thử unit/integration/e2e liên quan dispatch kênh, `Email`, `InAppNotification` và `ReadStatus`.

### Phase 8 — `NotificationDeliveryLog`, `DeliveryAttempt` & truy vết
- **Mục tiêu:** Ghi nhận và truy vấn lịch sử gửi/thử gửi phục vụ điều tra vận hành.
- **FR/NFR liên quan:** FR-14, FR-08, NFR-06, NFR-08.
- **UI (theo `docs/ux.md`):** `Nhật ký gửi thông báo` / `NotificationDeliveryLog` và `Lần thử gửi` / `DeliveryAttempt` trong chi tiết `Thông báo`.
- **Stories:**
  - [x] **ST-P8-01** — Triển khai entity & persistence `NotificationDeliveryLog` và `DeliveryAttempt` (thời gian, kênh, kết quả, lỗi, retry, `Mã tương quan`).
  - [x] **ST-P8-02** — Ghi nhận `DeliveryAttempt`/`NotificationDeliveryLog` trong luồng gửi, `Retry` và kết quả xử lý kênh.
  - [x] **ST-P8-03** — Triển khai API truy vấn lịch sử/truy vết theo `Notification` (lọc/phân trang theo quyền, retention cấu hình).
  - [x] **ST-P8-04** — Dựng UI hiển thị `Nhật ký gửi thông báo` & `Lần thử gửi` trong chi tiết `Thông báo` (loading/empty/error/success).
  - [x] **ST-P8-05** — Hoàn thiện kiểm thử unit/integration/e2e liên quan ghi nhận và truy vết lịch sử gửi.

### Phase 9 — `AdminUI` `Tổng quan` (`Dashboard`) & hoàn thiện vận hành
- **Mục tiêu:** Cung cấp màn hình `Tổng quan` và hoàn thiện trải nghiệm tra cứu/vận hành nhất quán theo `docs/ux.md`.
- **FR/NFR liên quan:** FR-15, FR-16, NFR-05, NFR-06.
- **UI (theo `docs/ux.md`):** `Tổng quan` / `Dashboard`, pattern UI vận hành nhất quán.
- **Stories:**
  - [x] **ST-P9-01** — Triển khai API/tổng hợp dữ liệu `Tổng quan` (tóm tắt `Trạng thái thông báo`, DLQ cần chú ý, trạng thái `Kiểm tra sức khỏe`).
  - [x] **ST-P9-02** — Dựng `AdminUI` màn hình `Tổng quan` / `Dashboard` (thẻ tổng hợp, lối tắt, loading/empty/error/success).
  - [x] **ST-P9-03** — Chuẩn hóa component/pattern UI (table, filter, modal, toast, empty state, skeleton, badge trạng thái) theo `docs/ux.md`.
  - [x] **ST-P9-04** — Chuẩn hóa thuật ngữ hiển thị theo `docs/domain-glossary.md` và responsive tối thiểu trên các màn hình `AdminUI` MVP.
  - [x] **ST-P9-05** — Hoàn thiện kiểm thử unit/integration/e2e liên quan `Tổng quan` và trải nghiệm vận hành.

### Phase 10 — Cấu hình quản trị: `Mẫu thông báo`, `Cấu hình kênh thông báo`, `Định tuyến sự kiện tích hợp`
- **Mục tiêu:** Quản trị viên cấu hình template/kênh/định tuyến sự kiện qua API và `AdminUI`.
- **FR/NFR liên quan:** FR-16, FR-11, FR-12, FR-02, NFR-05.
- **UI (theo `docs/ux.md`):** `Mẫu thông báo` / `NotificationTemplate`, `Cấu hình kênh thông báo` / `NotificationChannelConfig`, `Định tuyến sự kiện tích hợp` / `IntegrationEventRoute`.
- **Stories:**
  - [x] **ST-P10-01** — Triển khai CRUD API & persistence cho `Mẫu thông báo` / `NotificationTemplate` theo `RBAC`.
  - [x] **ST-P10-02** — Triển khai CRUD/quản lý `Cấu hình kênh thông báo` / `NotificationChannelConfig` (không lộ secret) theo `RBAC`.
  - [x] **ST-P10-03** — Triển khai CRUD/quản lý `Định tuyến sự kiện tích hợp` / `IntegrationEventRoute` (mapping event → template/kênh) theo `RBAC`.
  - [x] **ST-P10-04** — Dựng `AdminUI` màn hình quản lý `Mẫu thông báo` (danh sách/form CRUD, loading/empty/error/success).
  - [x] **ST-P10-05** — Dựng `AdminUI` màn hình quản lý `Cấu hình kênh thông báo` (loading/empty/error/success).
  - [x] **ST-P10-06** — Dựng `AdminUI` màn hình quản lý `Định tuyến sự kiện tích hợp` (loading/empty/error/success).
  - [x] **ST-P10-07** — Hoàn thiện kiểm thử unit/integration/e2e liên quan cấu hình quản trị và UI tương ứng.

### Phase 11 — `IntegrationEvent` end-to-end & `AuditLog`
- **Mục tiêu:** Tiếp nhận `IntegrationEvent`, ánh xạ qua `IntegrationEventRoute` tạo `Notification`, và ghi `AuditLog` cho thao tác nhạy cảm.
- **FR/NFR liên quan:** FR-02, FR-03, FR-10, FR-16, NFR-05, NFR-07.
- **Stories:**
  - [ ] **ST-P11-01** — Triển khai tiếp nhận/validate `IntegrationEvent` theo envelope chuẩn (`messageId`, `eventId`, `eventType`, `occurredAt`, `source`, `idempotencyKey`, `version`, `payload`) và ghi nhận truy vết.
  - [ ] **ST-P11-02** — Triển khai luồng map `IntegrationEvent` → `Notification` theo `IntegrationEventRoute` (event mẫu MVP).
  - [ ] **ST-P11-03** — Kết nối luồng `IntegrationEvent` end-to-end qua `TransactionalOutbox`/`RabbitMQ`/`NotificationWorker` với `Idempotency` đảm bảo.
  - [ ] **ST-P11-04** — Triển khai `AuditLog` cho thao tác quản trị/vận hành nhạy cảm (cấu hình, `Gửi lại`) và API truy vấn theo quyền.
  - [ ] **ST-P11-05** — Hoàn thiện kiểm thử unit/integration/e2e liên quan `IntegrationEvent` end-to-end và `AuditLog`.

### Phase 12 — Hardening, kiểm thử hiệu năng/tải & quy trình deploy staging/production
- **Mục tiêu:** Xác minh độ tin cậy/hiệu năng/khả năng vận hành đạt mục tiêu MVP và sẵn sàng deploy theo `docs/deploy.md`.
- **FR/NFR liên quan:** NFR-01, NFR-02, NFR-03, NFR-04, NFR-06, NFR-08, NFR-09, NFR-10; xác nhận FR-01→FR-16.
- **Stories:**
  - [ ] **ST-P12-01** — Hoàn thiện và chuẩn hóa bộ integration tests luồng chính (outbox/inbox/retry/DLQ/idempotency, `Email`/`InAppNotification`) theo `docs/conventions.md`.
  - [ ] **ST-P12-02** — Đo và cải thiện coverage unit test core modules theo mục tiêu (~70% core, định hướng) và xử lý khoảng trống quan trọng.
  - [ ] **ST-P12-03** — Thực hiện load/performance test (k6/NBomber) cho tiếp nhận và xử lý; tinh chỉnh để đánh giá/đạt mục tiêu NFR-01, NFR-03.
  - [ ] **ST-P12-04** — Rà soát và hardening bảo mật tối thiểu (JWT/RBAC, rate limit, CORS, che giấu PII/secret, audit, validation) cùng kiểm thử liên quan.
  - [ ] **ST-P12-05** — Chuẩn hóa tài liệu/artefact vận hành deploy (compose staging/production, env, backup/migration/rollback runbook) theo `docs/deploy.md`.
  - [ ] **ST-P12-06** — Xác minh deploy staging theo checklist (pre-check → build → migrate → deploy → smoke test/e2e) và ghi nhận kết quả.
  - [ ] **ST-P12-07** — Tổng hợp kết quả xác nhận FR/NFR/metric MVP, cập nhật hạn chế/rủi ro còn lại; **deploy production chỉ thực hiện khi có xác nhận rõ ràng từ người dùng trong cùng phiên**.
  - [ ] **ST-P12-08** — Chuẩn hóa `ProblemDetails`, mã lỗi nghiệp vụ và hợp đồng lỗi cho các endpoint chính (đã implement: AddProblemDetails/exception/status code pages, `ApiProblemCodes`, ToHttpResult module, RBAC/JWT 401/403 có `code`; chờ Đại ca xác nhận test pass).
  - [ ] **ST-P12-09** — Rà soát và hoàn thiện `Idempotency`/unique constraint xuyên suốt luồng (tạo, consume, resend, in-app) theo NFR-02 (đã implement: xác nhận unique index, bổ sung race handling CreateInAppNotification + Detach repo, thêm unit test race in-app; chờ Đại ca xác nhận test pass).

### Phase 13 — Quản lý user & credential (`IdentityAccess`, `EndUser`)
- **Mục tiêu:** Cung cấp quản lý tài khoản/vai trò/credential trong app (sẵn sàng tích hợp IdP sau), phát `JwtBearer` cho đăng nhập và `IntegrationClient` (client credentials), luồng tự đặt lại mật khẩu, và quản lý hồ sơ `EndUser` nhận in-app theo thiết kế `docs/design-user-management.md`.
- **FR/NFR liên quan:** Bổ sung phạm vi PRD (quản lý user/credential, phát `JWT`, đặt lại mật khẩu, hồ sơ `EndUser`); FR-01, FR-15, FR-16, NFR-05, NFR-06, NFR-07.
- **UI (theo `docs/ux.md`, bổ sung):** `Đăng nhập` / `Login` (credential nội bộ), quên/đặt lại mật khẩu, quản trị tài khoản & vai trò, quản trị `IntegrationClient` (client credentials), quản trị hồ sơ `EndUser`.
- **Stories:**
  - [x] **ST-P13-01** — Cập nhật tài liệu liên quan (`docs/prd.md`, `docs/domain-glossary.md`, `docs/architecture.md`, `docs/ux.md`, `docs/deploy.md`, `docs/decisions.md`) và thiết kế/lập schema & migration trong module `IdentityAccess` cho tài khoản/vai trò, credential, client credentials `IntegrationClient`, `PasswordResetToken`, và hồ sơ `EndUser` (khóa theo định danh hệ thống nguồn).
  - [x] **ST-P13-02** — Triển khai quản trị tài khoản `SystemAdministrator`/`BusinessOperator`/`IntegrationClient`: tạo/sửa/khóa/mở khóa/xóa (theo chính sách), gán vai trò, lưu mật khẩu/secret an toàn (hash) và API bảo vệ `RBAC`.
  - [x] **ST-P13-03** — Triển khai quản lý client credentials cho `IntegrationClient` (cấp/rotate/vô hiệu hóa, không lộ secret sau khi tạo) và endpoint client credentials để phát `JwtBearer` theo cấu hình hiện có.
  - [x] **ST-P13-04** — Triển khai đăng nhập `AdminUI` bằng credential nội bộ (xác thực, phát `JWT`, claims/role) và tích hợp với `Đăng xuất`/guard/điều hướng hiện có.
  - [x] **ST-P13-05** — Triển khai luồng tự đặt lại mật khẩu: yêu cầu reset, tạo token/link một lần có hạn, gửi email, xác nhận và cập nhật mật khẩu; xử lý bảo mật cơ bản (không lộ thông tin nhạy cảm, chống lạm dụng ở mức khả thi).
  - [x] **ST-P13-06** — Triển khai quản lý hồ sơ `EndUser` (tạo/sửa/khóa/mở khóa, tra cứu theo định danh nguồn) và contract/port để module `Notification`/`InAppNotification` tham chiếu theo nghiệp vụ.
  - [x] **ST-P13-07** — Dựng `AdminUI` quản trị tài khoản & `IntegrationClient` (danh sách, tạo/sửa, khóa/xóa, gán vai trò, quản lý client credentials) theo `RBAC`, gồm trạng thái loading/empty/error/success.
  - [x] **ST-P13-08** — Dựng `AdminUI` luồng quên/đặt lại mật khẩu và màn hình quản lý hồ sơ `EndUser` (loading/empty/error/success).
  - [x] **ST-P13-09** — Hoàn thiện kiểm thử unit/integration/e2e liên quan quản lý user, phát `JWT` (login & client credentials), reset mật khẩu, hồ sơ `EndUser` và UI tương ứng.

## 3. Trạng thái hiện tại
- **Phase đang làm:** Phase 11 — `IntegrationEvent` end-to-end & `AuditLog`.
- **Story đang làm:** (chưa bắt đầu).
- **Story tiếp theo đề xuất:** **ST-P11-01** — Triển khai tiếp nhận/validate `IntegrationEvent` theo envelope chuẩn (`messageId`, `eventId`, `eventType`, `occurredAt`, `source`, `idempotencyKey`, `version`, `payload`) và ghi nhận truy vết.
- **Chờ xác nhận test pass:** **ST-P12-08** (ProblemDetails/mã lỗi nghiệp vụ), **ST-P12-09** (Idempotency/unique constraint).
- **Tạm hoãn song song:** Không có.
- **Cập nhật lần cuối:** 2026-11-26 — Đại ca xác nhận pass unit/integration/e2e và AdminUI test liên quan ST-P10-01..07. Đã tick Done Phase 10.

## 4. Deferred Work

| Việc bị hoãn | Hoãn từ (phase/story) | Lý do hoãn | Mốc quay lại |
|---|---|---|---|
| *(Chưa có)* | — | — | — |

> Nếu có việc hoãn phát sinh, **bắt buộc** thêm dòng mới ngay (việc gì, từ đâu, lý do, mốc quay lại). Nếu chưa xác định mốc: ghi **"chưa xác định, cần review lại trước khi release"**.

## 5. Quy tắc cập nhật file này

Sau khi hoàn thành bất kỳ story/hạng mục nào, phải tick checkbox tương ứng trong file này **TRƯỚC KHI** báo hoàn thành task cho người dùng. Nếu story/hạng mục chưa đủ test 3 tầng (unit/integration/e2e) liên quan đều pass (theo `docs/conventions.md`) thì **không được tick**, dù code đã chạy đúng.

Nếu trong lúc làm phát sinh việc cần hoãn lại, phải thêm ngay vào **Deferred Work**, không được bỏ qua âm thầm.

Nếu trong lúc làm phát sinh quyết định đáng ghi, phải thêm vào `docs/decisions.md` trước, rồi mới tick checklist/story ở đây.

Khi thay đổi phase/story đang làm, phải cập nhật mục **Trạng thái hiện tại** và ghi thời điểm cập nhật.

## 6. Blocker/Risk log

| Ngày | Phase/Story | Blocker/Risk | Tác động | Cần người dùng quyết định | Trạng thái |
|---|---|---|---|---|---|
| 2026-04-03 | Phase 0 / ST-P0-01 → ST-P0-08 | Blocker môi trường shell/CLI/Docker/Node (agent) ngăn tự xác minh. | Đại ca đã xác nhận xác minh và pass các hạng mục liên quan (unit + integration + e2e theo phạm vi áp dụng). | — | Resolved |
| 2026-04-03 | Phase 1 / ST-P1-01 → ST-P1-06 | Blocker môi trường shell/CLI/Docker/Node (agent) ngăn tự xác minh. | Đại ca đã xác nhận xác minh và pass các hạng mục liên quan (unit + integration + e2e theo phạm vi áp dụng). | — | Resolved |
| 2026-04-03 | Phase 2 / ST-P2-01 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 2 / ST-P2-02 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 2 / ST-P2-03 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 2 / ST-P2-04 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 2 / ST-P2-05 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 2 / ST-P2-06 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 3 / ST-P3-01 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 3 / ST-P3-02 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test; chưa có EF Core migration cho `notifications`. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 3 / ST-P3-03 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test; migration `AddNotifications` đã được cập nhật để bao gồm cột/index idempotency và `CreatedAt` (khi implement ST-P3-04). | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |
| 2026-04-03 | Phase 3 / ST-P3-04 | Môi trường agent không chạy được `shell`/`.NET CLI`/Docker/Node để tự build/test; migration `AddNotifications` đã được cập nhật thêm `CreatedAt`/index phục vụ lọc/truy vấn. | Đại ca đã xác nhận kết quả pass unit + integration + e2e liên quan; đã tick Done theo DoD. | — | Resolved |

> Agent phải ghi blocker/risk tại đây khi gặp vướng mắc cần quyết định; **không được tự quyết rồi im lặng**.
