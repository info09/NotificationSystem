import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GetDeadLetterQueueMessagesResponse, ResendDeadLetterMessageRequest, ResendDeadLetterMessageResponse } from './dead-letter-queue.models';

@Injectable({ providedIn: 'root' })
export class DeadLetterQueueService {
  private readonly http = inject(HttpClient);

  /**
   * Lấy danh sách Thông điệp trong Hàng đợi thư chết.
   * @param queueName Tên Hàng đợi thư chết.
   * @param limit Số lượng tối đa.
   * @returns Danh sách Thông điệp DLQ.
   */
  getMessages(queueName: string, limit: number): Observable<GetDeadLetterQueueMessagesResponse> {
    const params = new HttpParams().set('limit', String(limit));
    return this.http.get<GetDeadLetterQueueMessagesResponse>(
      `/api/admin/dead-letter-queues/${encodeURIComponent(queueName)}/messages`,
      { params }
    );
  }

  /**
   * Gửi lại Thông điệp từ Hàng đợi thư chết.
   * @param queueName Tên Hàng đợi thư chết.
   * @param messageId Mã định danh Thông điệp.
   * @param request Dữ liệu yêu cầu gửi lại.
   * @returns Kết quả gửi lại.
   */
  resendMessage(
    queueName: string,
    messageId: string,
    request: ResendDeadLetterMessageRequest
  ): Observable<ResendDeadLetterMessageResponse> {
    return this.http.post<ResendDeadLetterMessageResponse>(
      `/api/admin/dead-letter-queues/${encodeURIComponent(queueName)}/messages/${encodeURIComponent(messageId)}/resend`,
      request
    );
  }
}
