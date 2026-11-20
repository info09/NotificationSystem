# Notification API Application Logic Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move all Notification API logic (validation, parsing, scope resolution, orchestration/result decisions) into Application so Presentation endpoints only bind raw input/caller context, call Application, and map Application results to HTTP responses.

**Architecture:** Introduce Application-level request/result models and handlers for create/list/detail. Handlers accept raw inputs plus caller context (subject, roles), perform parsing/validation/scope logic, invoke domain/repositories, and return typed results (success/validation/not found). Presentation maps results to `IResult` without business logic and preserves existing HTTP behavior/messages.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, C#, existing Notification module Clean Architecture, xUnit/Testcontainers tests.

**Spec:** User request in session (2026-04-03): "tất cả logic sẽ xử lý bên Application và bên API chỉ cần gọi sang Application và trả về kết quả thôi" (approved approach).

## Global Constraints

- Keep existing API behavior/contracts stable where possible (status codes, ProblemDetails validation messages/fields) so ST-P3-03/ST-P3-04 tests remain valid.
- Follow `docs/conventions.md`, `docs/domain-glossary.md`, and module boundaries (no business logic in Presentation).
- No commits; agent cannot run dotnet/Docker tests, so do not claim tests passed.

## Review Focus

- Create request validation/parsing must preserve required fields and channel/content/template/idempotency rules and return same validation semantics.
- List query parsing (status/channel/dates/page/pageSize/range) must reject invalid inputs with 400 validation errors as before.
- Read scope logic must preserve privileged readers (SystemAdministrator/BusinessOperator) vs scoped IntegrationClient behavior and 404 for out-of-scope detail access.

---

### Task 1: Application result/input/handler contracts

**Files:**
- Create: `src/Modules/Notification/Application/Notifications/NotificationAppResult.cs`
- Create: `src/Modules/Notification/Application/Notifications/NotificationCallerContext.cs`
- Create: `src/Modules/Notification/Application/Notifications/CreateNotification/CreateNotificationAppRequest.cs`
- Create: `src/Modules/Notification/Application/Notifications/GetNotifications/GetNotificationsAppRequest.cs`
- Create: `src/Modules/Notification/Application/Notifications/GetNotificationById/GetNotificationByIdAppRequest.cs`
- Create: `src/Modules/Notification/Application/Notifications/NotificationApiDtos.cs` (or equivalent app response DTOs)
- Modify: Application use cases/handlers

- [ ] **Step 1:** Define app-level result/error/request/caller types for create/list/detail.
- [ ] **Step 2:** Implement handlers in Application containing parsing, validation, scope resolution, and orchestration.
- [ ] **Step 3:** Update DI to register new handlers/services as needed.

### Task 2: Thin Presentation endpoints

**Files:**
- Modify: `src/Modules/Notification/Presentation/NotificationEndpointRouteBuilderExtensions.cs`
- Modify/Create: Presentation response/request types only if needed for binding/serialization

- [ ] **Step 1:** Replace endpoint logic with raw-input extraction, Application call, and result-to-HTTP mapping helper.
- [ ] **Step 2:** Keep route paths, auth policies, and response contracts consistent.

### Task 3: Tests and docs

**Files:**
- Modify: `tests/UnitTests/Notification/Application/*`
- Modify: `tests/IntegrationTests/Notification/*` and `tests/E2ETests/Notification/*` as needed
- Modify: `docs/status.md`

- [ ] **Step 1:** Update unit tests to target Application handlers/logic and preserve behavior expectations.
- [ ] **Step 2:** Adjust integration/e2e tests only if required by compatible refactor (prefer no behavior changes).
- [ ] **Step 3:** Record implementation/refactor status in `docs/status.md` without falsely marking Done.