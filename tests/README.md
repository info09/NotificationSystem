# Test framework

Khung kiểm thử theo `docs/conventions.md` và `docs/status.md` (ST-P0-06).

## Cấu trúc

- `tests/UnitTests/`: unit tests (nhanh, không phụ thuộc hạ tầng ngoài).
- `tests/IntegrationTests/`: integration tests (API/DB/RabbitMQ, dùng Testcontainers/Docker khi áp dụng).
- `tests/E2ETests/`: end-to-end tests (luồng API/AdminUI khi được triển khai).

## Lệnh chạy test chuẩn

Chạy từ thư mục gốc repository:

```bash
# Restore (nếu cần)
dotnet restore NotificationSystem.sln

# Chạy từng tầng
dotnet test tests/UnitTests/UnitTests.csproj
dotnet test tests/IntegrationTests/IntegrationTests.csproj
dotnet test tests/E2ETests/E2ETests.csproj

# Chạy toàn bộ test trong solution
dotnet test NotificationSystem.sln
```

Gợi ý theo gate CI (`unit → integration → e2e`):

```bash
dotnet test tests/UnitTests/UnitTests.csproj && dotnet test tests/IntegrationTests/IntegrationTests.csproj && dotnet test tests/E2ETests/E2ETests.csproj
```

Lưu ý: `IntegrationTests` có cấu hình Testcontainers (`TestcontainersConfiguration.cs`) và có thể yêu cầu Docker khi bổ sung test dùng container thật.