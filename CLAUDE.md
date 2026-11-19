# CLAUDE.md — Chỉ dẫn bắt buộc cho AI agent (Notification System)

File này áp dụng cho mọi session trong repo này. Trước mỗi task: tuân thủ toàn bộ quy tắc dưới đây. Không dựa vào ký ức session trước.

## 1. Đọc file trước khi bắt đầu task
- **Trước khi viết code mới, sửa bug, hoặc refactor:** bắt buộc đọc lại:
  - `docs/prd.md` — phần FR/NFR liên quan đến task.
  - `docs/domain-glossary.md` — **toàn bộ** (mọi khái niệm nghiệp vụ phải đúng thuật ngữ).
  - `docs/architecture.md` — phần module/liên quan đến task.
  - `docs/conventions.md` — **toàn bộ**.
  - `docs/status.md` — phase/story hiện tại và mục **Deferred Work** (không lặp việc đã cố ý hoãn).
  - `docs/decisions.md` — rà quyết định cũ liên quan task (không đi ngược quyết định đã chốt khi chưa biết).
- **Task liên quan giao diện** (tạo/sửa màn hình, component, form, UI copy): **bắt buộc** đọc thêm `docs/ux.md` trước khi code. Không tự sáng tạo pattern UI khác `docs/ux.md`.
- **Task liên quan build/deploy/release/CI-CD:** **bắt buộc** đọc thêm `docs/deploy.md` trước khi thực hiện.
- **Thiếu hoặc rỗng bất thường:** nếu bất kỳ file nào trong 9 file bắt buộc (`docs/prd.md`, `docs/domain-glossary.md`, `docs/decisions.md`, `docs/architecture.md`, `docs/ux.md`, `docs/deploy.md`, `docs/conventions.md`, `docs/status.md`, `CLAUDE.md`) không tồn tại hoặc trống khi lẽ ra phải có, **DỪNG LẠI** và báo người dùng. Không tự suy diễn nội dung thay thế.

## 2. Định nghĩa "Done" (Definition of Done)
- Task **chỉ** hoàn thành khi đồng thời có: (a) **unit test** cho logic mới/thay đổi, (b) **integration test** cho luồng liên quan, (c) **e2e test** cho hành vi người dùng cuối nếu task ảnh hưởng luồng user-facing — và **cả 3 tầng test đều pass**.
- Nếu thiếu bất kỳ tầng test nào (khi tầng đó áp dụng được), task **CHƯA xong**. Phải báo rõ còn thiếu tầng test nào; **không** được báo "hoàn thành" khi chưa đủ.

## 3. Quy trình sau khi hoàn thành 1 task
1. Chạy đủ 3 tầng test liên quan và xác nhận pass.
2. Tick checkbox/story tương ứng trong `docs/status.md` (và cập nhật **Trạng thái hiện tại** nếu đổi story/phase). Nếu có việc bị hoãn trong quá trình làm, thêm ngay vào **Deferred Work**.
3. Review code vừa viết theo checklist trong `docs/conventions.md` trước khi báo kết quả.
- Tuân thủ quy tắc trong `docs/status.md`: không tick khi chưa đủ 3 tầng test liên quan pass.

## 4. Quy tắc ghi quyết định (`docs/decisions.md`)
- Khi phải chọn giữa nhiều cách làm hợp lý (chọn thư viện, đổi cách tiếp cận so với `docs/architecture.md`, chọn giải pháp chưa có tiền lệ, hoặc bỏ qua best practice vì lý do thực tế): **PHẢI** ghi entry vào `docs/decisions.md` theo đúng format trong file đó (Context/Options/Decision/Rationale/Trade-off/Status).
- Ghi **TRƯỚC KHI** báo hoàn thành task. Không gộp ghi sau.
- Việc nhỏ không ảnh hưởng người khác (tên biến cục bộ, format code thuần túy) không cần ghi, theo ngưỡng trong `docs/decisions.md`.
- Khi phát sinh quyết định đáng ghi trong lúc làm: ghi `docs/decisions.md` trước, rồi mới tick `docs/status.md` (theo quy tắc status).

## 5. Kỷ luật thuật ngữ nghiệp vụ (`docs/domain-glossary.md`)
- Mọi tên biến/class/entity/bảng dữ liệu/label UI/copy đại diện khái niệm nghiệp vụ **PHẢI** truy ngược được về đúng 1 thuật ngữ trong `docs/domain-glossary.md`.
- Nếu cần khái niệm nghiệp vụ chưa có trong glossary: **PHẢI** bổ sung vào `docs/domain-glossary.md` (đúng format: thuật ngữ chuẩn / định nghĩa / KHÔNG được gọi là / thuật ngữ liên quan) **TRƯỚC KHI** viết code hoặc UI copy dùng tên đó. Không đặt tên tạm, không dịch/diễn đạt/viết tắt khác với glossary.
- Nếu phát hiện code/UI hiện tại dùng tên khác glossary: **báo người dùng và hỏi** có đổi cho khớp hay cập nhật glossary theo tên hiện có. **Không tự ý sửa hàng loạt.**

## 6. Quy tắc implement giao diện
- Mọi màn hình/component mới phải khớp nguyên tắc thiết kế, design token, và component pattern trong `docs/ux.md`.
- Nếu cần pattern UI chưa có trong `docs/ux.md`: bổ sung vào `docs/ux.md` trước (và ghi `docs/decisions.md` nếu là lựa chọn đáng kể). Không tạo pattern rời rạc riêng cho từng màn hình.
- UI copy/label/trạng thái hiển thị phải dùng đúng thuật ngữ trong `docs/domain-glossary.md`.

## 7. Quy tắc deploy/release
- Thực hiện đúng tuần tự trong `docs/deploy.md`.
- **Tuyệt đối KHÔNG** tự ý deploy lên **production** khi chưa có xác nhận rõ ràng từ người dùng trong phiên làm việc hiện tại, kể cả khi mọi test đã pass và pipeline cho phép chạy tự động.
- Deploy staging: chỉ khi pipeline cho phép và theo quy trình; vẫn tuân thủ `docs/deploy.md`.

## 8. Mâu thuẫn giữa các file
- Áp dụng cho **mọi cặp file** bắt buộc (ví dụ `docs/conventions.md` ↔ `docs/ux.md`, `docs/architecture.md` ↔ `docs/deploy.md`, `docs/domain-glossary.md` ↔ tên trong `docs/architecture.md` hoặc code hiện có). Nếu mô tả khác nhau cho cùng vấn đề, hoặc không thể đáp ứng đúng một file vì lý do kỹ thuật: **DỪNG LẠI và hỏi người dùng**. Không tự chọn một bên, không tự sửa file gốc để hợp thức hóa lựa chọn.
- **Thứ tự ưu tiên tạm** (chỉ để không kẹt, chưa hỏi kịp): `docs/prd.md` và `docs/domain-glossary.md` (nghiệp vụ & tên gọi) > `docs/architecture.md` / `docs/ux.md` / `docs/deploy.md` (thiết kế kỹ thuật) > `docs/conventions.md` (chi tiết code). Quyết định cuối **phải** xác nhận lại với người dùng và ghi vào `docs/decisions.md`.
- Nếu người dùng đồng ý đổi hướng: ghi `docs/decisions.md` với **Status: Active** và đánh dấu quyết định cũ liên quan là **Superseded**.

## 9. `[CẦN LÀM RÕ]` trong `docs/prd.md`
- Nếu task liên quan đến mục `[CẦN LÀM RÕ]` chưa giải quyết trong `docs/prd.md`: **phải hỏi người dùng trước khi code**. Không tự giả định.

## 10. Giới hạn phạm vi task
- Không tự ý implement thêm FR/chức năng chưa được yêu cầu trong task hiện tại, dù thấy "tiện làm luôn".
- Chỉ thực hiện đúng phạm vi được yêu cầu; nếu phát hiện việc liên quan cần làm, báo đề xuất hoặc thêm **Deferred Work**/Blocker theo `docs/status.md` (không tự mở rộng scope).