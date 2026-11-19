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
*(Chưa có quyết định. Các entry sẽ được bổ sung trong quá trình thiết kế và implement.)*
