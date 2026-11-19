# Domain Glossary — Ngôn ngữ chung (Ubiquitous Language)

## 1. Mục đích file
- Đây là nơi **chốt DUY NHẤT 1 tên gọi** cho mỗi khái niệm nghiệp vụ trong dự án Notification System.
- File này **không** giải thích kiến trúc hệ thống (xem `docs/architecture.md`) và **không** mô tả UI/luồng màn hình (xem `docs/ux.md`).
- Mọi tài liệu sinh sau (`architecture`, `ux`, `conventions`, `status`), mã nguồn, schema, API contract và nội dung hiển thị đều **PHẢI** dùng đúng tên gọi đã chốt tại đây. Không tự ý dùng từ đồng nghĩa khác.

## 2. Quy ước đọc bảng
- **Thuật ngữ chuẩn:** Tên được phép dùng duy nhất. Ghi dạng **"Tiếng Việt hiển thị" / `EnglishCodeName`** khi hai dạng khác nhau; nếu trùng hoặc là mã/giá trị cố định thì ghi rõ dạng sử dụng.
- **Định nghĩa:** Theo đúng ngữ cảnh dự án này (1–2 câu).
- **KHÔNG được gọi là:** Các từ/cách gọi dễ nhầm **cấm** dùng thay thế trong ngữ cảnh tương ứng.
- **Thuật ngữ liên quan:** Các thuật ngữ khác trong bảng để tra cứu chéo.

## 3. Bảng thuật ngữ (nhóm theo domain)

### 3.1 Người dùng & Phân quyền
| Thuật ngữ chuẩn | Định nghĩa | KHÔNG được gọi là | Thuật ngữ liên quan |
|---|---|---|---|
| "Quản trị viên hệ thống" / `SystemAdministrator` | Vai trò sử dụng Admin UI để quản lý cấu hình hệ thống, Mẫu thông báo, Kênh thông báo và giám sát hoạt động. | Admin chung chung (không rõ vai trò), Superuser, Root | Admin UI, Mẫu thông báo, Kênh thông báo, RBAC |
| "Nhân viên vận hành" / `BusinessOperator` | Vai trò sử dụng Admin UI để theo dõi trạng thái gửi, tra cứu Nhật ký gửi thông báo, xem Hàng đợi thư chết và thực hiện Gửi lại theo quyền. | Operator kỹ thuật hạ tầng, Support agent (nếu không đúng ngữ cảnh), Người quản trị | Admin UI, Thông báo, Hàng đợi thư chết, Gửi lại, Nhật ký gửi thông báo |
| "Nhà phát triển tích hợp" / `IntegrationClient` | Vai trò/chủ thể hệ thống nghiệp vụ gọi API hoặc phát hành Sự kiện tích hợp để tạo/yêu cầu gửi Thông báo. | Application Developer/Service (cách gọi tự do), Client app chung chung, User API | Yêu cầu tạo thông báo, Sự kiện tích hợp, API Key/JWT Bearer, RBAC |
| "Người dùng cuối" / `EndUser` | Người nhận Thông báo qua Email hoặc Thông báo trong ứng dụng. | Customer/User/Khách hàng (khi dùng thay cho người nhận trong ngữ cảnh gửi thông báo), Recipient như vai trò hệ thống | Người nhận, Thông báo trong ứng dụng, Email |
| "Người nhận" / `Recipient` | Đối tượng nhận Thông báo (địa chỉ email, định danh người dùng in-app hoặc thông tin định danh kênh tương ứng). | Subscriber, Contact (nếu dùng không nhất quán), Target | Thông báo, Kênh thông báo, Email, Thông báo trong ứng dụng |
| "Kiểm soát truy cập theo vai trò" / `RBAC` | Cơ chế phân quyền dựa trên vai trò (`SystemAdministrator`, `BusinessOperator`, `IntegrationClient`) để giới hạn thao tác trên API/Admin UI. | ACL rời rạc, Permission list tự do (khi nói về mô hình phân quyền chính) | JWT Bearer, Quản trị viên hệ thống, Nhân viên vận hành, Nhà phát triển tích hợp |
| "JWT Bearer" / `JwtBearer` | Cơ chế xác thực token Bearer (JWT) dùng để bảo vệ API và thao tác theo RBAC trong MVP. | Session cookie auth, Basic Auth, API Key-only (trừ khi được bổ sung riêng) | RBAC, Nhà phát triển tích hợp, Admin UI |

### 3.2 Thông báo & Nội dung
| Thuật ngữ chuẩn | Định nghĩa | KHÔNG được gọi là | Thuật ngữ liên quan |
|---|---|---|---|
| "Thông báo" / `Notification` | Bản ghi nghiệp vụ đại diện cho một yêu cầu gửi thông tin đến Người nhận qua một Kênh thông báo, có vòng đời trạng thái và lịch sử xử lý. | Message (khi ám chỉ bản ghi nghiệp vụ), Alert/Push/Email rời rạc không gắn bản ghi, Event | Người nhận, Kênh thông báo, Trạng thái thông báo, Nhật ký gửi thông báo, Yêu cầu tạo thông báo |
| "Yêu cầu tạo thông báo" / `CreateNotificationRequest` | Đầu vào do Nhà phát triển tích hợp gửi qua API (hoặc phát sinh từ Sự kiện tích hợp) để hệ thống tạo Thông báo mới. | Command/Event payload chung chung (khi nói về API tạo), Notification object | Thông báo, Idempotency Key, Mẫu thông báo, Kênh thông báo |
| "Mẫu thông báo" / `NotificationTemplate` | Cấu hình nội dung/khung hiển thị được quản trị để render nội dung Thông báo theo kênh và dữ liệu đầu vào. | Template file mã nguồn, Email layout cứng trong code, Content snippet tự do | Quản trị viên hệ thống, Kênh thông báo, Email, Thông báo trong ứng dụng |
| "Kênh thông báo" / `NotificationChannel` | Phương thức phân phối Thông báo; MVP gồm `email` và `inapp`, thiết kế cho phép mở rộng `sms`/`push` sau. | Provider, Transport, Medium (dùng không nhất quán) | Email, Thông báo trong ứng dụng, Thông báo, Mẫu thông báo, Cấu hình kênh thông báo |
| "Cấu hình kênh thông báo" / `NotificationChannelConfig` | Bản ghi cấu hình quản trị cho một Kênh thông báo (ví dụ bật/tắt, tham số không nhạy cảm, liên kết thiết lập gửi) trong phạm vi MVP. | SMTP secret/credential, Provider account, Hardcode config | Kênh thông báo, Admin UI, Quản trị viên hệ thống, Nhà cung cấp Email |
| "Email" / `Email` | Kênh thông báo gửi thư điện tử đến Người nhận thông qua Nhà cung cấp Email/SMTP đã cấu hình. | Mail, Thư, SMTP (khi dùng thay cho kênh) | Kênh thông báo, Nhà cung cấp Email, Mẫu thông báo |
| "Thông báo trong ứng dụng" / `InAppNotification` | Thông báo được lưu trong hệ thống để Người dùng cuối truy xuất qua API pull/polling, có trạng thái đọc/chưa đọc trong MVP. | Push notification, Realtime message, Toast (khi ám chỉ bản ghi lưu trữ) | Kênh thông báo, Trạng thái đọc, Người dùng cuối |

### 3.3 Vòng đời & Trạng thái
| Thuật ngữ chuẩn | Định nghĩa | KHÔNG được gọi là | Thuật ngữ liên quan |
|---|---|---|---|
| "Trạng thái thông báo" / `NotificationStatus` | Trạng thái vòng đời của Thông báo do hệ thống quản lý: `Pending`, `Processing`, `Sent`, `Failed`. | Status chung chung không thuộc tập giá trị, Delivery state của provider (nếu khác ngữ cảnh) | Thông báo, Pending, Processing, Sent, Failed |
| `Pending` (Chờ xử lý) | Thông báo đã được tiếp nhận/lưu hợp lệ và chờ được Worker xử lý hoặc đưa vào luồng gửi. | Queued (khi dùng thay tên trạng thái), New, Created | Trạng thái thông báo, Worker |
| `Processing` (Đang xử lý) | Thông báo đang được Worker xử lý/gửi tại thời điểm ghi nhận. | InProgress, Sending (nếu dùng thay tên trạng thái chuẩn) | Trạng thái thông báo, Worker, Lần thử gửi |
| `Sent` (Đã gửi) | Thông báo đã được hệ thống gửi/dispatch thành công theo kết quả xử lý của kênh tương ứng. | Delivered (trừ khi provider xác nhận riêng và được định nghĩa sau), Completed | Trạng thái thông báo, Nhật ký gửi thông báo |
| `Failed` (Thất bại) | Thông báo không gửi thành công sau khi hết chính sách Thử lại hoặc gặp lỗi không thể phục hồi theo quy tắc. | Error, Dead (khi dùng thay trạng thái thông báo), Dropped | Trạng thái thông báo, Thử lại, Hàng đợi thư chết |
| "Trạng thái đọc" / `ReadStatus` | Trạng thái của Thông báo trong ứng dụng đối với Người dùng cuối: `Unread` (chưa đọc), `Read` (đã đọc). | Seen/Opened (nếu chưa định nghĩa), Email read receipt | Thông báo trong ứng dụng, Người dùng cuối |

### 3.4 Tích hợp & Vận chuyển thông điệp
| Thuật ngữ chuẩn | Định nghĩa | KHÔNG được gọi là | Thuật ngữ liên quan |
|---|---|---|---|
| "Sự kiện tích hợp" / `IntegrationEvent` | Sự kiện nghiệp vụ do hệ thống nguồn phát hành để Notification System tiếp nhận và chuyển thành yêu cầu xử lý/gửi Thông báo. | Domain event nội bộ tùy ý, Log event, Notification | Nhà phát triển tích hợp, Thông điệp, Transactional Outbox, MessageId |
| "Thông điệp" / `Message` | Gói dữ liệu được vận chuyển qua Message Broker (RabbitMQ) giữa producer và consumer để xử lý bất đồng bộ. | Thông báo (bản ghi nghiệp vụ), Email body, Chat message | Sự kiện tích hợp, Exchange, Queue, Routing Key, MessageId |
| "Message Broker" / `MessageBroker` | Hạ tầng trung gian vận chuyển Thông điệp; MVP sử dụng RabbitMQ. | Database queue, Event store (khi dùng thay broker), Kafka (ngoài MVP) | RabbitMQ, Thông điệp, Exchange, Queue |
| "RabbitMQ" / `RabbitMQ` | Message Broker được chọn cho MVP để publish/consume Thông điệp. | Kafka, Redis Streams, Service Bus (trong ngữ cảnh lựa chọn MVP) | Message Broker, Exchange, Queue, Routing Key, Hàng đợi thư chết |
| "Exchange" / `Exchange` | Thành phần RabbitMQ nhận Thông điệp từ producer và định tuyến tới Queue theo Routing Key/binding. | Queue, Topic (khái niệm chung không gắn RabbitMQ) | RabbitMQ, Routing Key, Queue, Thông điệp |
| "Queue" / `Queue` | Hàng đợi RabbitMQ lưu Thông điệp để Worker/consumer tiêu thụ. | Bảng DB, Job list trong memory, Hàng đợi thư chết (khi nói queue xử lý chính) | RabbitMQ, Exchange, Worker, Hàng đợi thư chết |
| "Routing Key" / `RoutingKey` | Khóa định tuyến gắn với Thông điệp khi publish để Exchange quyết định đưa vào Queue phù hợp. | Queue name, Event type tùy ý (nếu không map rõ), Binding key ngược nghĩa | Exchange, Queue, Thông điệp |
| "Transactional Outbox" / `TransactionalOutbox` | Mẫu đảm bảo ghi dữ liệu nghiệp vụ và bản ghi outbox trong cùng transaction DB trước khi publish Thông điệp, tránh mất sự kiện. | Outbox email, Fire-and-forget publish, Dual-write không kiểm soát | Sự kiện tích hợp, Thông điệp, Cơ sở dữ liệu, Bản ghi Outbox |
| "Bản ghi Outbox" / `OutboxMessage` | Bản ghi lưu trong Cơ sở dữ liệu đại diện cho Thông điệp/Sự kiện cần publish theo Transactional Outbox, gồm trạng thái publish và thông tin cần thiết để gửi an toàn. | Outbox table chung chung không chuẩn tên, Event log tùy ý, Queue item | Transactional Outbox, Thông điệp, Sự kiện tích hợp, Cơ sở dữ liệu |
| "Inbox Pattern" / `InboxPattern` | Mẫu lưu dấu vết tiếp nhận/xử lý Thông điệp để phát hiện trùng và đảm bảo xử lý idempotent ở consumer. | Outbox, Deduplication tùy ý không có lưu vết chuẩn | Idempotency, MessageId, Thông điệp, Bản ghi Inbox |
| "Bản ghi Inbox" / `InboxMessage` | Bản ghi lưu dấu vết Thông điệp đã tiếp nhận/xử lý (theo `MessageId` và ngữ cảnh xử lý) để thực thi Idempotency tại consumer. | Processed message log tự do, Dedup cache không bền vững (khi nói bản ghi chuẩn), Retry log | Inbox Pattern, Idempotency, MessageId, Cơ sở dữ liệu |
| "Idempotency" / `Idempotency` | Tính chất cho phép xử lý lặp cùng yêu cầu/thông điệp mà không tạo hiệu ứng gửi hoặc bản ghi trùng ngoài mong muốn. | Exactly-once, Retry-only (không có chống trùng) | Idempotency Key, MessageId, Inbox Pattern, Gửi lại |
| "MessageId" / `MessageId` | Định danh duy nhất của Thông điệp/Sự kiện tích hợp dùng để truy vết và chống trùng xử lý (khóa chính idempotency ở Inbox). | NotificationId, CorrelationId tùy ý, EventId (nếu dùng thay mà không map) | Thông điệp, Sự kiện tích hợp, Inbox Pattern, Idempotency |
| "Idempotency Key" / `IdempotencyKey` | Khóa do client/source cung cấp (đặc biệt khi tạo Thông báo qua API) để chống tạo trùng yêu cầu theo phạm vi nguồn/client. | MessageId (khi dùng thay nhau), RequestId chung chung | Yêu cầu tạo thông báo, Nhà phát triển tích hợp, Idempotency |
| "Định tuyến sự kiện tích hợp" / `IntegrationEventRoute` | Cấu hình ánh xạ loại `IntegrationEvent` (và điều kiện liên quan nếu có) tới Mẫu thông báo và/hoặc Kênh thông báo cần tạo/gửi trong MVP. | Hardcode routing trong code, Event handler mapping tự do (khi nói cấu hình quản trị), Workflow engine | Sự kiện tích hợp, Mẫu thông báo, Kênh thông báo, Admin UI |

### 3.5 Xử lý, Gửi & Phục hồi
| Thuật ngữ chuẩn | Định nghĩa | KHÔNG được gọi là | Thuật ngữ liên quan |
|---|---|---|---|
| "Worker" / `NotificationWorker` | Thành phần xử lý nền (BackgroundService) tiêu thụ Thông điệp từ Queue và thực hiện gửi/dispatch Thông báo theo Kênh thông báo. | Web API handler, Cron job chung chung, Consumer không rõ vai trò | Queue, Thông báo, Kênh thông báo, Thử lại, Gửi lại |
| "Thử lại" / `Retry` | Cơ chế tự động xử lý lại Lần thử gửi thất bại do lỗi tạm thời theo chính sách giới hạn và backoff. | Resend thủ công, Requeue tay (khi nói cơ chế tự động), Infinite retry | Lần thử gửi, Exponential Backoff, Hàng đợi thư chết, Trạng thái thông báo |
| "Exponential Backoff" / `ExponentialBackoff` | Chiến lược tăng dần thời gian chờ giữa các lần Thử lại (kèm jitter/cap theo cấu hình MVP) để giảm tải khi lỗi tạm thời. | Fixed delay, Immediate retry, Linear backoff | Thử lại, Worker |
| "Hàng đợi thư chết" / `DeadLetterQueue` (`DLQ`) | Hàng đợi đích chứa Thông điệp không xử lý thành công sau khi vượt chính sách Thử lại hoặc bị loại theo quy tắc, để vận hành kiểm tra và Gửi lại. | Error log, Failed status table (khi nói queue), Archive queue chung chung | Thử lại, Gửi lại, Queue, Nhân viên vận hành, Thông điệp |
| "Gửi lại" / `Resend` | Thao tác vận hành (qua Admin UI/API theo quyền) để đưa Thông báo/Thông điệp lỗi hoặc trong DLQ vào luồng xử lý lại theo cơ chế idempotent. | Reprocess/Retry thủ công tự do, Duplicate send không kiểm soát | Hàng đợi thư chết, Nhật ký gửi thông báo, Idempotency, Nhân viên vận hành |
| "Nhật ký gửi thông báo" / `NotificationDeliveryLog` | Tập hợp bản ghi lịch sử gửi/xử lý của một Thông báo, dùng để tra cứu trạng thái, lỗi và các lần thử gửi. | Audit log hệ thống chung, Application log, Message trace rời rạc | Thông báo, Lần thử gửi, Trạng thái thông báo, Nhân viên vận hành |
| "Lần thử gửi" / `DeliveryAttempt` | Một lần thực hiện gửi/dispatch Thông báo tới kênh/provider trong một thời điểm, gồm kết quả, lỗi (nếu có) và thứ tự/thời gian thử. | Retry count đơn thuần, Log line, Notification status | Nhật ký gửi thông báo, Thử lại, Worker, Nhà cung cấp Email |
| "Nhà cung cấp Email" / `EmailProvider` | Dịch vụ/cấu hình SMTP hoặc gateway thực hiện gửi Email; MVP dùng SMTP cấu hình được (dev: Mailpit/MailHog). | Email channel, Notification system, Mail server hạ tầng chung (khi dùng thay provider tích hợp) | Email, Kênh thông báo, Mẫu thông báo |

### 3.6 Vận hành & Quan sát
| Thuật ngữ chuẩn | Định nghĩa | KHÔNG được gọi là | Thuật ngữ liên quan |
|---|---|---|---|
| "Admin UI" / `AdminUI` | Giao diện web quản trị/vận hành cho Quản trị viên hệ thống và Nhân viên vận hành thực hiện cấu hình, tra cứu, xem DLQ và Gửi lại theo RBAC. | Portal người dùng cuối, Swagger/Scalar docs, CLI công cụ hạ tầng | Quản trị viên hệ thống, Nhân viên vận hành, RBAC, Mẫu thông báo, Hàng đợi thư chết |
| "Kiểm tra sức khỏe" / `HealthCheck` | Endpoint/cơ chế kiểm tra tình trạng sẵn sàng và sống của API cùng phụ thuộc (DB, RabbitMQ) phục vụ giám sát/vận hành. | Smoke test nghiệp vụ, Monitoring dashboard, Uptime report | API, Cơ sở dữ liệu, RabbitMQ |
| "Mã tương quan" / `CorrelationId` | Giá trị định danh (như `messageId`, `notificationId`, trace id) được gắn vào log/bản ghi để liên kết và truy vết xuyên suốt luồng xử lý. | RequestId ngẫu nhiên không lưu, Primary key nghiệp vụ (khi dùng sai mục đích), Span id đơn lẻ không có ngữ cảnh | MessageId, Thông báo, Nhật ký gửi thông báo, Structured Log |
| "Cơ sở dữ liệu" / `Database` | Nguồn dữ liệu chính (source of truth) của hệ thống, lưu Thông báo, cấu hình, outbox/inbox, nhật ký và trạng thái; MVP dùng PostgreSQL. | Cache, Message Broker, File storage | Transactional Outbox, Inbox Pattern, Thông báo, Nhật ký gửi thông báo |
| "Structured Log" / `StructuredLog` | Nhật ký sự kiện/lỗi dạng có cấu trúc (Serilog) kèm trường ngữ cảnh và Mã tương quan để tra cứu và phân tích. | Plain text log rời rạc, Console print, Audit UI record | Mã tương quan, Kiểm tra sức khỏe, Lần thử gửi |
| "Nhật ký kiểm toán" / `AuditLog` | Bản ghi lưu vết các thao tác quản trị/vận hành nhạy cảm (ví dụ cấu hình, Gửi lại từ DLQ) để phục vụ truy vết trách nhiệm và điều tra. | Structured Log kỹ thuật thuần túy, NotificationDeliveryLog, Nhật ký hệ thống hạ tầng | Admin UI, Gửi lại, Quản trị viên hệ thống, Nhân viên vận hành, Mã tương quan |

### 3.7 Điều hướng & Màn hình Admin UI
| Thuật ngữ chuẩn | Định nghĩa | KHÔNG được gọi là | Thuật ngữ liên quan |
|---|---|---|---|
| "Đăng nhập" / `Login` | Màn hình/chức năng xác thực người dùng để truy cập `AdminUI` theo `RBAC`. | Sign in page tùy ý, Authentication portal chung chung (khi đặt tên màn hình) | Admin UI, JWT Bearer, RBAC, Quản trị viên hệ thống, Nhân viên vận hành |
| "Đăng xuất" / `Logout` | Chức năng kết thúc phiên làm việc hiện tại trên `AdminUI`. | Sign out tùy ý, Disconnect | Đăng nhập, Admin UI |
| "Tổng quan" / `Dashboard` | Màn hình chính của `AdminUI` hiển thị tóm tắt hoạt động và tình trạng hệ thống phục vụ theo dõi vận hành. | Home chung chung, Overview page tùy ý, Monitoring dashboard chuyên dụng | Admin UI, Kiểm tra sức khỏe, Thông báo, Hàng đợi thư chết |

## 4. Nhóm domain và lưu ý đồng âm khác nghĩa
- Các nhóm domain trong file: **Người dùng & Phân quyền**, **Thông báo & Nội dung**, **Vòng đời & Trạng thái**, **Tích hợp & Vận chuyển thông điệp**, **Xử lý, Gửi & Phục hồi**, **Vận hành & Quan sát**.
- **Phân biệt bắt buộc:**
  - **"Thông báo" (`Notification`)** là bản ghi nghiệp vụ/vòng đời gửi; **"Thông điệp" (`Message`)** là gói vận chuyển trên Message Broker. Không dùng thay thế cho nhau.
  - **`MessageId`** định danh Thông điệp/Sự kiện để chống trùng xử lý; **`IdempotencyKey`** là khóa chống trùng yêu cầu tạo từ client/source. Không dùng thay thế cho nhau.
  - **"Queue"** (hàng đợi xử lý chính) khác **"Hàng đợi thư chết" (`DLQ`)**.
  - **"Thử lại" (`Retry`)** là cơ chế tự động; **"Gửi lại" (`Resend`)** là thao tác vận hành thủ công/có kiểm soát.
  - **"Trạng thái thông báo" (`NotificationStatus`)** khác **"Trạng thái đọc" (`ReadStatus`)** của Thông báo trong ứng dụng.
- Hiện chưa phát hiện thuật ngữ nào trong `docs/prd.md` vừa được sử dụng để gọi hai khái niệm nghiệp vụ khác nhau trong hai domain khác nhau ngoài các cặp phân biệt đã liệt kê trên. Nếu phát sinh, phải tách thành hai dòng/bảng chú thích rõ domain.

## 5. Quy tắc bổ sung thuật ngữ mới
Khi implement phát sinh khái niệm nghiệp vụ mới chưa có trong bảng trên, **PHẢI** thêm vào đây (đúng format 4 cột) **TRƯỚC KHI** dùng tên đó trong code hoặc UI, không được tự đặt tên tạm rồi dùng luôn. Nếu có nhiều cách gọi hợp lý và cần chọn 1, ghi quyết định chọn tên vào `docs/decisions.md`.

## 6. Ghi chú về đa ngôn ngữ
- UI hiển thị tiếng Việt, code/contract kỹ thuật viết tiếng Anh (trường hợp phổ biến của dự án).
- Thuật ngữ chuẩn trong bảng **luôn đi kèm cả 2 dạng** (tiếng Việt hiển thị và tên tiếng Anh dùng trong code) khi hai dạng khác nhau; đây là **bảng map DUY NHẤT** giữa hai ngôn ngữ.
- Không được tự dịch khác hoặc tự đổi tên tiếng Anh/Việt ở bất kỳ tài liệu, API, DB schema, biến, class, label UI hay log message nào ngoài mapping đã chốt tại đây. Nếu cần thay đổi, cập nhật bảng này trước và ghi quyết định tại `docs/decisions.md` (khi có nhiều lựa chọn).
