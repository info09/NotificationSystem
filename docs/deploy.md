# Deploy & Vận hành — Notification System

> Tài liệu này là hướng dẫn vận hành để agent thực hiện hoặc hỗ trợ deploy đúng quy trình, bám sát `docs/prd.md` (NFR-04, NFR-05, NFR-06, NFR-09, NFR-10) và `docs/architecture.md` (Modular Monolith .NET 10, Angular `AdminUI`, PostgreSQL, RabbitMQ, Docker Compose, GitHub Actions).

## 1. Môi trường (Environments)

| Môi trường | Mục đích | Hạ tầng/cấu hình | Database | Domain/TLS | Ghi chú |
|---|---|---|---|---|---|
| **local/dev** | Phát triển, debug, chạy test cục bộ | Docker Compose trên máy dev; có thể chạy service .NET/Angular ngoài container khi debug | PostgreSQL container (dữ liệu dev, có thể reset) | Không có (localhost/HTTP nội bộ) | Có Mailpit cho `EmailProvider` SMTP dev; log/secret dùng giá trị dev, không dùng dữ liệu thật nhạy cảm |
| **staging** | Kiểm thử tích hợp, UAT nội bộ, xác minh deploy/smoke test trước production | Docker Compose trên server staging (single-instance) | PostgreSQL self-host trong Compose (staging) hoặc volume riêng; dữ liệu tách biệt production | **Hiện chưa có** (chưa chốt domain/TLS) | Cấu hình gần production; dùng SMTP/test provider hoặc cấu hình kiểm soát; cho phép deploy khi pipeline pass |
| **production** | Phục vụ vận hành thực tế (phạm vi MVP, single-instance) | Docker Compose trên server production (single-instance) | PostgreSQL self-host trong Compose (production) với volume bền vững và backup theo quy trình vận hành | **Hiện chưa có** (chưa chốt domain/TLS) | Uptime mục tiêu 99% best effort (NFR-09), RTO < 30 phút; mọi deploy phải có xác nhận người dùng (xem mục 9) |

**Khác biệt chính giữa môi trường:** connection strings, credentials/secret, cấu hình `EmailProvider` SMTP, cấu hình `JwtBearer` (issuer/audience/keys), CORS/origin `AdminUI`, mức log/retention, dữ liệu và chính sách backup. RabbitMQ chạy self-host trong Docker Compose ở các môi trường (theo kiến trúc MVP, chưa dùng managed service).

## 2. Hạ tầng

- **Nơi host:** Server (staging/production) chạy Docker Engine + Docker Compose; local chạy Docker Compose trên máy dev. (Chưa chốt cloud provider/domain cụ thể — hiện chạy theo mô hình Docker Compose.)
- **Map với kiến trúc (`docs/architecture.md`):** Modular Monolith backend .NET 10 + `AdminUI` Angular SPA + `NotificationWorker`/BackgroundService + PostgreSQL + RabbitMQ + Mailpit (dev).
- **Thành phần/service (Compose):**
  - `api`: ASP.NET Core Web API (tiếp nhận `CreateNotificationRequest`/`IntegrationEvent`, Admin API, In-app pull, `HealthCheck` `/health`, `/health/ready`)
  - `worker`: `NotificationWorker` (BackgroundService, có thể scale-out bằng số replica/process trong phạm vi host/compose khi cần; MVP mặc định 1 instance)
  - `admin-ui`: `AdminUI` Angular SPA (static build phục vụ qua Nginx/container web)
  - `postgres`: PostgreSQL 16+ (volume dữ liệu bền vững ở staging/production)
  - `rabbitmq`: RabbitMQ (exchange/queue/DLQ theo topology MVP)
  - `mailpit`: chỉ môi trường local/dev (SMTP test). **Staging/production không dùng Mailpit**, dùng SMTP/`EmailProvider` thật theo cấu hình env.
- **Cấu trúc thư mục triển khai (khuyến nghị trên server):**
  - `/opt/notification-system/compose.yml` (hoặc file compose môi trường)
  - `/opt/notification-system/.env.staging` hoặc `.env.production` (không commit, quyền truy cập hạn chế)
  - `/opt/notification-system/volumes/` hoặc Docker named volumes cho postgres/rabbitmq (backup định kỳ)

## 3. Quản lý biến môi trường/secret

- **Nguyên tắc:** **Không bao giờ commit** secret, token, password, connection string thật, JWT signing key vào repo (kể cả trong compose file). Chỉ commit file mẫu giá trị giả (`.env.example`, `appsettings.Example.json` nếu có).
- **Nơi lưu:**
  - **Local/dev:** file `.env` (hoặc user-secrets/dotnet secrets cho dev) trên máy cá nhân, bị ignore bởi git.
  - **CI (GitHub Actions):** GitHub Actions Secrets/Variables cho giá trị dùng trong pipeline (ví dụ registry credentials nếu có, token cần thiết). Không in secret ra log.
  - **Staging/Production (server Docker Compose):** file env trên server (`.env.staging`/`.env.production`) với quyền truy cập hạn chế (chỉ user vận hành/CI deploy được phép đọc). **Hiện chưa có** secret manager/domain/TLS tập trung; khi bổ sung (vault/cloud secret manager), cập nhật tài liệu và quy trình.
- **Cách agent lấy giá trị khi chạy local:**
  1. Sao chép `.env.example` thành `.env` (hoặc file env tương ứng).
  2. Điền giá trị local/dev (DB, RabbitMQ, JWT dev key, SMTP Mailpit, CORS origin local cho `AdminUI`).
  3. Chạy `docker compose up` / lệnh dev theo hướng dẫn repo; không yêu cầu người dùng cung cấp secret production.
- **Danh sách biến bắt buộc tối thiểu (phải có để chạy được):**
  - **Database:** `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `ConnectionStrings__Database` (hoặc biến tương đương theo cấu hình .NET).
  - **RabbitMQ:** `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`, `RABBITMQ_VHOST` (nếu dùng), exchange/queue config nếu tách biến.
  - **API/Worker app:** `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS` (hoặc cấu hình port), cấu hình `MessageId`/`Idempotency` retention, retry/concurrency (mặc định theo PRD: retry 3 lần, concurrency 5/worker) nếu override.
  - **JWT Bearer (`JwtBearer`) & RBAC:** `JWT__Issuer`, `JWT__Audience`, `JWT__SigningKey` (hoặc key/certificate tương đương), thời hạn token/refresh theo thiết kế.
  - **AdminUI (Angular):** `API_BASE_URL` (hoặc cấu hình environment build/runtime tương đương), allowed origin/CORS phía API (`CORS__AllowedOrigins`).
  - **SMTP/`EmailProvider`:** `SMTP__Host`, `SMTP__Port`, `SMTP__User`, `SMTP__Password` (nếu có), `SMTP__From`, `SMTP__EnableSsl` (local dùng Mailpit; staging/prod dùng provider thật).
  - **Observability:** `OTEL_*` (nếu bật OpenTelemetry), log level/Serilog sink config theo môi trường.
  - **CI/CD deploy (GitHub Actions):** secrets truy cập server/registry (SSH key hoặc cơ chế deploy được chọn), image tags/registry credentials nếu publish image; **chỉ cấu hình khi phương thức deploy được chốt**.

## 4. Quy trình build & CI/CD (GitHub Actions)

- **Nền tảng:** GitHub Actions (workflow trong `.github/workflows/`).
- **Các bước build (đề xuất chuẩn):** checkout → setup .NET/Node (cho Angular) → restore dependencies → lint/format cơ bản (nếu cấu hình) → build backend → build `AdminUI` → build Docker images (khi áp dụng) → publish artifacts/images theo nhánh/môi trường.
- **Thứ tự chạy test như gate bắt buộc trước khi cho phép deploy:**
  1. **Unit tests** (nhanh, không phụ thuộc hạ tầng ngoài)
  2. **Integration tests** (API + DB + RabbitMQ, ưu tiên Testcontainers/Docker theo NFR-08)
  3. **E2E tests** (nếu có trong pipeline/scope; ví dụ luồng `AdminUI` hoặc luồng end-to-end triển khai sau)
- **Quy tắc gate:** Nếu **bất kỳ tầng test nào fail**, pipeline **phải dừng** và **không được deploy tiếp** (bao gồm không publish artifact/image dùng cho deploy và không chạy bước deploy). Deploy staging/production chỉ được thực hiện từ commit/artifact đã pass đầy đủ các tầng test được kích hoạt trong pipeline.
- **Ghi chú:** Load test (k6/NBomber) theo PRD/NFR-08 là kiểm thử hiệu năng định kỳ/benchmark, không bắt buộc chạy trong mọi pipeline deploy trừ khi được cấu hình riêng.

## 5. Database migration

- **Công cụ:** Entity Framework Core migrations (theo `docs/architecture.md`).
- **Nguyên tắc:** Migration áp dụng lên production phải **reversible** hoặc có **kế hoạch rollback rõ ràng** được chuẩn bị và review trước khi chạy. Ưu tiên migration tương thích ngược (expand/contract) khi thay đổi schema có rủi ro.
- **Cách chạy theo môi trường:**
  - **local/dev:** chạy migration tự động hoặc thủ công khi khởi tạo/cập nhật schema dev; có thể reset DB khi cần.
  - **staging:** chạy migration như bước bắt buộc trong quy trình deploy staging (sau pre-check/build, trước hoặc trong bước rollout có kiểm soát), dùng cùng quy trình với production.
  - **production:** chỉ chạy sau pre-check và backup/điểm khôi phục sẵn sàng; thực hiện theo checklist deploy, giám sát lỗi và sẵn sàng rollback theo kế hoạch.
- **Rollback migration:**
  - Nếu migration có down script an toàn: rollback bằng EF Core xuống migration trước đó đã xác định.
  - Nếu thay đổi không reversible an toàn: chuẩn bị script/chiến lược bù (data fix/forward fix) và kế hoạch khôi phục từ backup; **không** chạy migration loại này trên production khi chưa có kế hoạch rollback được xác nhận.

## 6. Quy trình deploy từng bước

Checklist tuần tự (agent phải làm theo đúng thứ tự khi được yêu cầu deploy):

1. **Pre-check**
   - Xác nhận môi trường mục tiêu (staging/production), commit/tag/image cần deploy.
   - Xác nhận pipeline CI (GitHub Actions) đã pass: build + unit → integration → e2e (nếu có). Nếu fail: **dừng**.
   - Xác nhận biến môi trường/secret trên môi trường đích đã đầy đủ (mục 3), không thiếu key bắt buộc.
   - Kiểm tra kết nối/khả dụng hạ tầng đích (Docker host, dung lượng đĩa, volume DB), và backup/restore point cho production.
   - Đối chiếu thay đổi DB: có migration không, có reversible/rollback plan không.
2. **Build**
   - Build artifact/image từ commit đã pass CI (ưu tiên dùng image/artifact từ CI; tránh build ad-hoc khác biệt với CI).
   - Gắn tag phiên bản rõ ràng (commit SHA/tag) để truy vết và rollback.
3. **Migrate**
   - Áp dụng database migration cho môi trường đích (staging/production) theo kế hoạch đã review.
   - Xác nhận migration thành công (log/exit code, schema version).
4. **Deploy**
   - Cập nhật image/tag và cấu hình compose/env trên môi trường đích.
   - Chạy `docker compose pull` (nếu dùng registry) và `docker compose up -d` (hoặc quy trình tương đương) theo thứ tự an toàn (DB/RabbitMQ sẵn sàng trước khi app/worker phụ thuộc).
   - Đảm bảo service khởi động đúng: `api`, `worker`, `admin-ui` (và hạ tầng cần thiết).
5. **Smoke test sau deploy**
   - Kiểm tra `GET /health` (liveness) và `GET /health/ready` (readiness: API, DB, RabbitMQ) trả về trạng thái OK.
   - Kiểm tra `AdminUI` tải được và gọi API thành công (theo origin/CORS cấu hình).
   - Kiểm tra luồng tối thiểu: API tiếp nhận/tra cứu cơ bản hoạt động; worker kết nối queue (không có lỗi kết nối nghiêm trọng trong log).
   - Kiểm tra log lỗi ngay sau deploy (xem mục 8).

## 7. Rollback

- **Điều kiện phải rollback (ví dụ):** health/readiness fail kéo dài, lỗi nghiêm trọng lan rộng (5xx tăng, không xử lý được message), migration gây lỗi ứng dụng/dữ liệu, worker crash loop, hoặc không đạt smoke test sau deploy.
- **Nguyên tắc:** Ưu tiên khôi phục dịch vụ nhanh (RTO mục tiêu < 30 phút, NFR-04/NFR-09) bằng rollback phiên bản đã biết ổn định; xử lý triệt để nguyên nhân sau khi hệ thống ổn.
- **Rollback theo môi trường:**
  - **local/dev:** `docker compose down/up` với image/config trước đó, reset/rollback DB dev nếu cần.
  - **staging:** quay lại image/tag và compose config trước đó (`docker compose up -d`), rollback migration nếu an toàn/cần thiết theo kế hoạch; chạy smoke test lại.
  - **production:**
    1. Dừng rollout tiếp (nếu đang thực hiện từng phần).
    2. Khôi phục phiên bản ứng dụng trước đó (image/tag + compose/env đã biết tốt).
    3. Xử lý DB: chỉ rollback migration nếu có down-safe; nếu không, áp dụng kế hoạch bù/khôi phục từ backup theo kịch bản đã chuẩn bị.
    4. Khởi động lại services cần thiết, xác nhận `/health` và `/health/ready`.
    5. Theo dõi log/lỗi và backlog queue/DLQ sau rollback; ghi nhận sự cố và nguyên nhân.
- **Sau rollback:** Ghi nhận sự kiện (thời gian, phiên bản, nguyên nhân, hành động) và cập nhật `docs/decisions.md`/incident note nếu có quyết định hoặc thay đổi quy trình.

## 8. Monitoring & Logging sau deploy

Cần kiểm tra ngay sau khi deploy để xác nhận hệ thống ổn (đáp ứng NFR-04, NFR-06, NFR-09):

- **Health check:** `GET /health` và `GET /health/ready` (API, Database, RabbitMQ) phải OK; thiết lập kiểm tra lặp lại trong vài phút đầu sau deploy.
- **Log lỗi & structured log:** kiểm tra log của `api`, `worker`, `admin-ui` (nếu có), RabbitMQ/Postgres container khi cần; tìm lỗi startup, kết nối DB/RabbitMQ, lỗi `Retry`/DLQ bất thường, exception tăng đột biến. Đảm bảo log có `CorrelationId` (`messageId`, `notificationId`, trace id) khi truy vết sự cố.
- **Chức năng nền:** xác nhận `OutboxMessage` được publish, `NotificationWorker` consume bình thường, không có backlog tăng bất thường do lỗi cấu hình.
- **Smoke nghiệp vụ tối thiểu:** một luồng tạo/tra cứu `Thông báo` hoặc luồng in-app/email test (staging) theo kịch bản an toàn.
- **Alert (nếu có):** hiện MVP chưa yêu cầu dashboard/alerting chuyên dụng; nếu môi trường có cấu hình alert cơ bản (healthcheck/uptime/log), phải xác nhận hoạt động. Khi chưa có, thực hiện kiểm tra thủ công theo checklist trên sau mỗi lần deploy.

## 9. Quy tắc bắt buộc dành cho agent

**KHÔNG được tự ý deploy lên production khi chưa có xác nhận rõ ràng từ người dùng trong cùng phiên làm việc, kể cả khi mọi test đã pass. Deploy lên staging có thể tự thực hiện nếu pipeline cho phép, nhưng production luôn cần xác nhận thủ công.**
