# Conventions — Chuẩn code & review (Notification System)

> Tài liệu này là checklist/chuẩn bắt buộc để tự review code trước khi báo "done". Mỗi quy tắc phải được áp dụng máy móc (đúng/sai). Phạm vi tech stack: **.NET 10 / ASP.NET Core / C#**, **EF Core + PostgreSQL**, **RabbitMQ**, **Angular (AdminUI)**, **Docker Compose**, test **xUnit/MSTest + Integration + E2E (nếu có)** theo `docs/architecture.md`, `docs/prd.md`, `docs/deploy.md`.

## 1. Naming convention

### 1.1 Quy tắc truy vết glossary (bắt buộc cho khái niệm nghiệp vụ)
- Mọi **class/entity/record/interface/enum/DTO/table/collection** đại diện khái niệm nghiệp vụ **PHẢI** truy vết về **đúng 1 thuật ngữ code** trong `docs/domain-glossary.md` (cột `EnglishCodeName`/thuật ngữ chuẩn).
- **Không** tự đổi nghĩa, không dùng từ đồng nghĩa cấm trong glossary (ví dụ: không dùng `Message` để đặt tên entity nghiệp vụ `Notification`; không dùng `Template` thay `NotificationTemplate`).
- **Quy tắc chuyển đổi từ glossary sang tên code:**
  - Giữ nguyên **gốc từ** của `EnglishCodeName` trong glossary; chỉ thay đổi **casing/số nhiều theo convention kỹ thuật**.
  - C# type/class/interface/enum/record/property/DB table: **PascalCase** từ gốc glossary (ví dụ `Notification`, `DeliveryAttempt`, `OutboxMessage`).
  - Biến local/parameter/field private/JSON property (nếu camelCase): **camelCase** từ cùng gốc (ví dụ `notification`, `deliveryAttempt`, `messageId`).
  - DB column: **snake_case** từ cùng gốc/ý nghĩa (ví dụ `notification_id`, `message_id`, `created_at`) hoặc **PascalCase** nếu dự án chốt một chuẩn duy nhất — **chỉ được chọn 1** và áp dụng toàn bộ (xem 1.6).
  - API route/resource: **kebab-case** hoặc **lowercase plural nouns** từ gốc glossary (xem 1.7), không đổi nghĩa.
- Nếu cần tên cho khái niệm nghiệp vụ **chưa có** trong glossary: **PHẢI** bổ sung glossary trước (đúng format 4 cột) rồi mới dùng trong code/UI (`docs/domain-glossary.md` mục 5).

| Loại | Quy tắc | Đúng | Sai |
|---|---|---|---|
| File (C#) | PascalCase, trùng tên type chính trong file; 1 type public/top-level chính mỗi file | `Notification.cs`, `CreateNotificationHandler.cs`, `OutboxMessage.cs` | `notification.cs`, `notif.cs`, `Helper.cs` (mơ hồ), `NotificationEntity.cs` (thêm hậu tố đổi nghĩa nếu glossary không có) |
| File (Angular/TS) | kebab-case theo Angular convention + suffix (`.component.ts`, `.service.ts`, `.module.ts`, `.model.ts`) | `notification-list.component.ts`, `dead-letter-queue.service.ts`, `delivery-attempt.model.ts` | `NotificationList.ts`, `dlqSvc.ts`, `notification.component.tsx` (sai stack) |
| Class / Record / Struct (C#) | PascalCase, danh từ rõ nghĩa; entity nghiệp vụ = gốc glossary | `Notification`, `NotificationDeliveryLog`, `IntegrationEventRoute` | `Noti`, `MsgRecord` (cho `Notification`), `NotificationTbl` |
| Interface (C#) | PascalCase, prefix `I` | `INotificationRepository`, `IEmailProvider` | `NotificationRepositoryInterface`, `iEmailProvider` |
| Enum & Enum value (C#) | PascalCase; giá trị trạng thái nghiệp vụ khớp glossary | `NotificationStatus.Pending`, `ReadStatus.Unread` | `notificationStatus.pending`, `Status.P` |
| Method / Function (C#) | PascalCase, động từ + mục đích rõ | `CreateNotificationAsync`, `MarkAsReadAsync`, `PublishOutboxMessageAsync` | `Do`, `HandleStuff`, `CreateNoti` |
| Method / Function (TS) | camelCase, động từ + mục đích rõ | `loadNotifications()`, `resendMessage()`, `markAsRead()` | `LoadNotifications()`, `doIt()` |
| Biến / Parameter (C#) | camelCase; không viết tắt tùy tiện | `notificationId`, `idempotencyKey`, `deliveryAttempt` | `nId`, `tmp`, `data1` (trừ lambda cực ngắn theo chuẩn rõ ràng) |
| Biến / Property (TS) | camelCase; model nghiệp vụ giữ gốc glossary ở dạng camelCase | `messageId`, `notificationStatus`, `deliveryAttempts` | `msg`, `stat`, `arr` |
| Const (C#) | PascalCase cho `const`/`static readonly` public theo .NET, hoặc UPPER_SNAKE_CASE chỉ khi quy ước project thống nhất — **chọn 1 và nhất quán** | `MaxRetryCount = 3`, `DefaultConcurrency = 5` | trộn nhiều kiểu trong cùng scope |
| DB table | PascalCase số nhiều hoặc snake_case số nhiều từ gốc glossary — **chọn 1 chuẩn duy nhất**; tên truy vết glossary | `Notifications`, `DeliveryAttempts`, `OutboxMessages` (nếu chọn PascalCase plural) | `tbl_notification`, `noti`, `Messages` (cho bảng `Notification`) |
| DB column | snake_case từ gốc glossary/field chuẩn — **khuyến nghị nhất quán** (`notification_id`, `message_id`, `created_at`, `notification_status`) | `notification_id`, `idempotency_key`, `retry_count` | `nid`, `col1`, `NotificationID` (trộn style) |
| API endpoint (URL path) | lowercase, kebab-case, resource số nhiều theo glossary; versioning nếu dùng phải nhất quán (`/api/v1/...`) | `/api/v1/notifications`, `/api/v1/dead-letter-queues/{id}/resend`, `/api/v1/notification-templates` | `/api/CreateNoti`, `/api/getNotification`, `/api/dlq/do` |
| API query/body field | camelCase JSON (mặc định ASP.NET/Angular) nhưng **ngữ nghĩa** phải truy vết glossary (`messageId`, `idempotencyKey`, `notificationId`) | `{"messageId":"...", "idempotencyKey":"..."}` | `{"msg_id":"...", "key":"..."}` (đổi nghĩa/tên gốc tùy tiện) |
| Git branch | kebab-case có prefix loại công việc: `feature/`, `fix/`, `chore/`, `docs/`, `refactor/`, `test/` + mô tả ngắn | `feature/notification-create-api`, `fix/dlq-resend-idempotency` | `mybranch`, `update`, `Feature/ABC` (trộn hoa/thường không chuẩn) |
| Commit message | Conventional Commits: `type(scope): short summary` (scope tùy chọn nhưng nên có module/feature); summary rõ, không mơ hồ | `feat(notification): add create notification API`, `fix(delivery): prevent duplicate send on retry` | `update`, `fix bug`, `done`, `aaaa` |

> **Ghi chú DB naming:** Project phải chốt **một** bộ chuẩn (ví dụ tables PascalCase plural + columns snake_case) trong lần thiết lập đầu tiên và áp dụng đồng nhất; trộn chuẩn = sai.

## 2. Cấu trúc thư mục

Cây thư mục chuẩn (Modular Monolith, map `docs/architecture.md` mục 3). **Mỗi module backend là một Clean Architecture riêng** (`Domain`, `Application`, `Infrastructure`, `Presentation`). Tên thư mục/module nghiệp vụ dùng gốc glossary khi đại diện domain.

```text
/
├─ docs/                         # PRD, architecture, glossary, conventions, deploy, ux, decisions, status
├─ src/
│  ├─ BuildingBlocks/            # Cross-cutting dùng chung: Observability, abstractions, shared contracts (không chứa business logic domain)
│  │  ├─ Observability/          # StructuredLog, CorrelationId, HealthCheck extensions (FR-15, NFR-06)
│  │  └─ SharedKernel/           # Base types, result/error primitives, common interfaces (nếu cần)
│  ├─ Modules/
│  │  ├─ Notification/           # Module Notification
│  │  │  ├─ Domain/              # Entities, value objects, enums, domain rules (Notification, NotificationStatus)
│  │  │  ├─ Application/         # Use cases/CQRS handlers, DTOs, validators, ports (CreateNotification, queries)
│  │  │  ├─ Infrastructure/      # EF Core mappings/repos, external adapters trong module
│  │  │  └─ Presentation/        # API endpoints/controllers mapping cho module (nếu tách theo module)
│  │  ├─ TemplateConfiguration/  # NotificationTemplate, NotificationChannelConfig, IntegrationEventRoute
│  │  │  ├─ Domain/ Application/ Infrastructure/ Presentation/
│  │  ├─ IntegrationMessaging/   # IntegrationEvent, TransactionalOutbox/OutboxMessage, InboxPattern/InboxMessage, RabbitMQ publish/consume contracts
│  │  │  ├─ Domain/ Application/ Infrastructure/ Presentation/
│  │  ├─ Delivery/               # NotificationWorker logic, Retry/ExponentialBackoff, DeliveryAttempt, NotificationDeliveryLog, channel dispatch
│  │  │  ├─ Domain/ Application/ Infrastructure/ Presentation/
│  │  ├─ Administration/         # Admin API use cases, AuditLog, DLQ view/resend orchestration
│  │  │  ├─ Domain/ Application/ Infrastructure/ Presentation/
│  │  └─ IdentityAccess/         # JwtBearer auth, RBAC policies/authorization handlers
│  │     ├─ Domain/ Application/ Infrastructure/ Presentation/
│  ├─ Hosts/
│  │  ├─ Api/                    # ASP.NET Core host: composition root, middleware, endpoint mapping, DI wiring, HealthCheck
│  │  └─ Worker/                 # Worker host: BackgroundService hosting, consumers, outbox publisher runtime
│  └─ AdminUI/                   # Angular SPA (FR-16)
│     ├─ src/app/
│     │  ├─ core/                # auth, interceptors, guards, api clients, layout shell
│     │  ├─ shared/              # reusable components/pipes/directives/models (theo pattern UX)
│     │  ├─ features/
│     │  │  ├─ login/
│     │  │  ├─ dashboard/
│     │  │  ├─ notifications/
│     │  │  ├─ dead-letter-queue/
│     │  │  ├─ notification-templates/
│     │  │  ├─ notification-channel-configs/
│     │  │  └─ integration-event-routes/
│     │  └─ app.routes.ts
│     └─ ...
├─ tests/
│  ├─ UnitTests/                 # Domain/Application unit tests (nhanh, không hạ tầng ngoài)
│  ├─ IntegrationTests/          # API + DB + RabbitMQ (Testcontainers/Docker), luồng chính
│  └─ E2ETests/                  # E2E (nếu có: API/AdminUI flow)
├─ docker/                       # Dockerfiles, compose files, init scripts
├─ .github/workflows/            # GitHub Actions CI/CD
├─ .env.example                  # Mẫu biến môi trường (không có secret thật)
└─ README.md
```

**Quy tắc lớp/chứa gì (áp dụng cho từng module backend theo Clean Architecture, kiểm tra đúng/sai):**
- `Domain`: chỉ business rules, entities/value objects/enums/interfaces thuần domain; **không** tham chiếu EF Core, ASP.NET, RabbitMQ client, HTTP, Angular.
- `Application`: use cases/handlers, orchestration, validation, DTO/command/query, ports/interfaces; **không** chứa chi tiết hạ tầng.
- `Infrastructure`: implement ports, EF Core DbContext/migrations/repository, RabbitMQ/SMTP/external adapters; phụ thuộc Domain/Application.
- `Presentation`/`Hosts`: controllers/endpoints/UI wiring, middleware, DI composition; **không** chứa business logic.
- `AdminUI`: chỉ UI/client logic; **không** truy cập DB trực tiếp, **không** chứa business rule quyết định trạng thái nghiệp vụ nguồn sự thật.

## 3. Coding style theo ngôn ngữ đã chọn

### 3.1 Backend (C# / .NET 10 / ASP.NET Core / EF Core)
- **Nullable & types:** bật nullable reference types; không tắt cảnh báo nullability hàng loạt. Kiểu dữ liệu rõ ràng; tránh `dynamic` trừ khi bắt buộc và có lý do.
- **Async:** I/O-bound dùng `async/await` end-to-end; suffix `Async` cho method async; truyền `CancellationToken` trong request path/handler/worker khi có thể. Không dùng `.Result`/`.Wait()` trong code ứng dụng.
- **DI & lifetime:** đăng ký service qua DI; không `new` trực tiếp dependency hạ tầng trong business logic (trừ value object/factory đơn giản). Chọn lifetime đúng (Scoped cho DbContext/request services, Singleton cho stateless thread-safe, v.v.).
- **EF Core:** truy vấn dùng parameterized LINQ; không nối chuỗi SQL từ input; dùng transaction khi cần nhất quán (Outbox/Inbox/state changes); tránh N+1 cho truy vấn danh sách; projection/paging cho list API.
- **API:** controller/endpoint mỏng: validate input, gọi Application handler, map kết quả/lỗi; dùng DTO/command/request rõ ràng (chống mass assignment).
- **Immutability/encapsulation:** ưu tiên bất biến cho DTO/command; entity/domain bảo vệ invariants qua method, không expose setter tùy ý khi không cần.
- **Logging:** dùng `StructuredLog` (Serilog) với `CorrelationId` (`messageId`, `notificationId`, trace id); không log secret/PII nhạy cảm.
- **Formatting:** tuân thủ `.editorconfig`/formatter của repo; không để cảnh báo build quan trọng bị bỏ qua tùy tiện.

### 3.2 Frontend (Angular / TypeScript)
- **TypeScript strict:** bật strict mode; không dùng `any` trừ trường hợp bất khả kháng có comment lý do và phạm vi hẹp.
- **Kiến trúc:** tách `core` (singleton services, auth/interceptor/guard), `shared` (component tái dùng), `features` (màn hình theo IA/UX). Component tập trung hiển thị/tương tác; logic gọi API/xử lý dữ liệu đặt trong service/facade phù hợp.
- **RxJS & state:** quản lý subscription (async pipe ưu tiên, hoặc unsubscribe rõ ràng); không lồng subscribe sâu gây rò rỉ/khó kiểm soát; xử lý loading/empty/error/success theo `docs/ux.md`.
- **HTTP & auth:** gọi backend qua API client/service tập trung; gắn `JwtBearer` qua interceptor đúng chuẩn; xử lý 401/403/5xx nhất quán (redirect/hiển thị lỗi theo UX). Không hardcode base URL/secret.
- **UI terms:** label, tên màn hình, trạng thái hiển thị phải khớp `docs/domain-glossary.md` và `docs/ux.md`.
- **Styling:** theo design tokens/component patterns trong `docs/ux.md`; không tạo pattern mới tùy ý cho cùng loại component.
- **Lint/format:** tuân thủ ESLint/Prettier/cấu hình Angular của repo; không commit lỗi lint.

## 4. Error handling convention

- **Không silent fail:** mọi lỗi xử lý nghiệp vụ/kỹ thuật phải được (1) xử lý có chủ đích, hoặc (2) trả về kết quả lỗi rõ ràng, hoặc (3) log đầy đủ ngữ cảnh và ném/propagate theo cơ chế chuẩn. **Cấm** `catch` rỗng, nuốt exception, hoặc trả success giả.
- **Phân loại lỗi:**
  - **Validation/input lỗi:** trả 4xx (thường `400`) với chi tiết trường/lý do; không retry tự động.
  - **Unauthorized/Forbidden:** `401`/`403` theo `JwtBearer`/`RBAC`.
  - **Not found:** `404` khi resource không tồn tại.
  - **Conflict/idempotency conflict nghiệp vụ:** `409` (hoặc kết quả idempotent đã định nghĩa) theo contract, không tạo trùng bản ghi/side-effect.
  - **Lỗi tạm thời phụ thuộc ngoài (provider/network/5xx):** trong worker áp dụng `Retry` + `ExponentialBackoff` + DLQ theo PRD; tại API không che giấu bằng 200.
  - **Lỗi hệ thống không lường trước:** trả `500` (hoặc mã phù hợp) với thông điệp an toàn cho client, log chi tiết nội bộ kèm `CorrelationId`.
- **API error response:** dùng format nhất quán (khuyến nghị `ProblemDetails` của ASP.NET Core) gồm `type/title/status/detail/traceId` (và errors cho validation). Không trả stack trace/secret/PII ra client.
- **Worker/messaging:** lỗi xử lý phải tuân thủ at-least-once + idempotent (`InboxMessage`/`MessageId`), `ack` chỉ khi an toàn; lỗi theo chính sách → retry/DLQ, không mất im lặng.
- **Logging lỗi:** log mức phù hợp (`Warning`/`Error`/`Critical`) với ngữ cảnh (`messageId`, `notificationId`, kênh, attempt) nhưng không log PII nhạy cảm/secret.
- **Frontend:** hiển thị thông báo lỗi thân thiện theo `docs/ux.md`; không hiển thị raw stack trace; vẫn log/trace kỹ thuật ở mức cho phép (dev/telemetry) không lộ secret.

## 5. Testing convention

### 5.1 Cấu trúc & naming
- **Cấu trúc:** Arrange-Act-Assert (AAA) hoặc Given-When-Then nhất quán trong từng test.
- **Tên test case:** mô tả hành vi/kết quả rõ ràng, dạng `MethodOrScenario_Condition_ExpectedResult` (C#) hoặc tương đương rõ nghĩa.
  - Đúng: `CreateNotification_WithValidRequest_ReturnsPendingNotification`, `ConsumeMessage_WhenMessageIdDuplicated_DoesNotCreateDuplicateDeliveryAttempt`
  - Sai: `Test1`, `Works`, `ShouldBeOk`
- **Tên file test:** `<Target>Tests.cs` (C#), `<target>.spec.ts` (Angular).
  - Đúng: `NotificationServiceTests.cs`, `OutboxPublisherIntegrationTests.cs`, `notification-list.component.spec.ts`
  - Sai: `Tests.cs`, `MyTest.cs`
- **Phân tầng:** `UnitTests` (nhanh, cô lập), `IntegrationTests` (hạ tầng thực/containers), `E2ETests` (nếu có). Test phải đặt đúng thư mục tầng tương ứng.

### 5.2 Mock & test data strategy
- **Unit tests:** mock/fake dependencies ngoài đơn vị (repository, broker, clock, provider) qua interface/port; không kết nối DB/RabbitMQ/SMTP thật. Kiểm tra logic domain/application, validation, mapping.
- **Integration tests:** dùng hạ tầng thực qua Testcontainers/Docker (PostgreSQL, RabbitMQ, Mailpit khi cần) để xác minh wiring, EF Core, transaction/outbox/inbox, consumer/ack/retry/DLQ/idempotency, API contract chính.
- **Không mock cái cần chứng minh:** trong integration test của luồng chính (outbox/inbox/retry/DLQ/idempotency) không thay thế các thành phần cốt lõi bằng mock làm mất ý nghĩa kiểm chứng.
- **Dữ liệu test:** tách biệt, tự tạo/dọn dẹp; không phụ thuộc dữ liệu môi trường thật; không dùng secret production.

### 5.3 Coverage & mức nghiêm ngặt (bám NFR độ tin cậy/chất lượng)
- **Tối thiểu (CI gate mục tiêu):**
  - **Unit tests:** coverage **>= 70% line** cho các module core `Domain` và `Application` (Notification, IntegrationMessaging, Delivery, TemplateConfiguration, Administration) — theo định hướng NFR-08; module không đạt phải có lý do và kế hoạch bổ sung, không bỏ qua im lặng.
  - **Integration tests (bắt buộc pass):** phải có test cho các luồng chính: tạo/tiếp nhận `Notification`, `TransactionalOutbox` publish, consume + `InboxPattern`/`Idempotency` (chống trùng `MessageId`/`IdempotencyKey`), `Retry` (3 lần, lỗi tạm thời), chuyển `DeadLetterQueue` và `Resend`, dispatch kênh `Email`/`InAppNotification` ở mức abstraction/integration khả thi.
  - **E2E tests:** nếu có pipeline E2E, phải pass cho các luồng Admin UI/API được kích hoạt; không deploy khi fail (theo `docs/deploy.md`).
- **Quy tắc chất lượng:** test flaky không được chấp nhận là "pass"; phải sửa hoặc cô lập nguyên nhân. Thay đổi logic reliability (retry/idempotency/outbox/inbox/DLQ) **bắt buộc** có/ cập nhật test tương ứng.

## 6. Quy tắc review (checklist trước khi báo "done")

Đánh dấu từng mục **Đúng/Sai** trước khi hoàn tất task:

- [ ] Tên class/entity/table/field nghiệp vụ truy vết đúng 1 thuật ngữ trong `docs/domain-glossary.md` (không dùng từ cấm/đồng nghĩa sai ngữ cảnh).
- [ ] Cấu trúc code đặt đúng lớp/module theo `docs/architecture.md` và mục 2 (Domain/Application/Infrastructure/Presentation/Hosts/AdminUI).
- [ ] Không có business logic trong controller/endpoint/UI component; controller/endpoint chỉ điều phối input → handler → response.
- [ ] Không hardcode secret, token, password, connection string; cấu hình qua biến môi trường/secret store (`docs/deploy.md`).
- [ ] Không silent fail: không có `catch` rỗng, không nuốt lỗi, API/worker trả/log/xử lý lỗi đúng quy ước mục 4.
- [ ] Không còn `TODO`/`FIXME` chưa giải quyết, code bị comment-out không cần thiết, `console.log`/`print`/debug log tạm sót lại.
- [ ] Tên biến/hàm/class rõ nghĩa, không viết tắt tùy tiện, tuân thủ casing theo mục 1.
- [ ] Thay đổi liên quan DB có migration hợp lệ và cân nhắc rollback/reversible theo `docs/deploy.md`.
- [ ] Có test phù hợp (unit/integration/e2e nếu áp dụng); luồng reliability liên quan (outbox/inbox/retry/DLQ/idempotency) được bảo vệ khi bị ảnh hưởng.
- [ ] Đã chạy build/test/lint liên quan và kết quả pass (hoặc nêu rõ phần chưa chạy được do môi trường).
- [ ] API error/validation trả format nhất quán, không lộ stack trace/PII/secret.
- [ ] Logging có `CorrelationId` khi phù hợp, không log PII nhạy cảm/secret.

## 7. Điều cấm tuyệt đối

- **Cấm** đặt tên entity/class/table/biến nghiệp vụ khác với hoặc sai nghĩa so với `docs/domain-glossary.md`; cấm dùng từ đồng nghĩa nằm trong cột "KHÔNG được gọi là".
- **Cấm** đặt business logic/domain rule trong controller, endpoint, Angular component/page, hoặc middleware UI; logic nghiệp vụ phải nằm ở Domain/Application/use case tương ứng.
- **Cấm** gọi trực tiếp Cơ sở dữ liệu từ frontend (`AdminUI`) hoặc từ client ngoài backend API; mọi truy cập dữ liệu đi qua backend theo kiến trúc.
- **Cấm** truy cập/inject hạ tầng trực tiếp vào Domain (EF Core DbContext, RabbitMQ, SMTP, HTTP client, ASP.NET types).
- **Cấm** hardcode secret/credential/token/connection string trong source code, test, compose commit, hoặc log.
- **Cấm** silent fail: nuốt exception, bỏ qua lỗi validation/idempotency, hoặc `ack`/đánh dấu thành công khi xử lý chưa an toàn.
- **Cấm** commit/giữ lại debug artifacts: `console.log`/debug print tạm, breakpoint artifacts, code tắt tạm không lý do, `TODO` chưa xử lý trong phần hoàn tất.
- **Cấm** dùng truy vấn SQL/command ghép chuỗi từ input không tham số hóa; cấm bypass validation cho `CreateNotificationRequest`/`IntegrationEvent`.
- **Cấm** tự ý deploy production khi chưa có xác nhận rõ ràng từ người dùng trong cùng phiên làm việc (xem `docs/deploy.md` mục 9).
- **Cấm** sửa/xóa trạng thái lịch sử sai cách trong tài liệu quyết định: không xóa entry cũ trong `docs/decisions.md` khi bị thay thế (phải đánh `Superseded` và trỏ entry mới).
