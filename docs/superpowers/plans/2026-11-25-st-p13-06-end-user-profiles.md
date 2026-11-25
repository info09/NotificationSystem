# ST-P13-06 EndUser Profile Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add EndUser profile management in IdentityAccess (create/update/lock/unlock/lookup by external source id) and expose a SharedKernel contract/port for Notification/InAppNotification business reference.

**Architecture:** Extend existing IdentityAccess module vertically: domain behavior on `EndUserProfile`, application repo abstraction + use cases/DTOs, EF repository, RBAC-protected admin endpoints, and DI registrations. Cross-module reference uses a new `BuildingBlocks.SharedKernel` interface implemented in IdentityAccess Infrastructure to avoid direct module references.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, EF Core, PostgreSQL, xUnit, WebApplicationFactory + Testcontainers.

**Spec:** `docs/superpowers/specs/2026-11-25-st-p13-06-end-user-profile-design.md`

## Global Constraints

- Do not mark ST-P13-06 Done until Đại ca confirms related unit+integration+e2e pass.
- Do not commit. Do not run terminal/install/deploy commands without explicit permission.
- Keep module boundaries: no direct reference between modules; cross-module port goes through `BuildingBlocks.SharedKernel`.
- All new/changed `src/**/*.cs` types/methods require Vietnamese `/// <summary>` XML docs; skip generated migrations designer/snapshot.
- Follow existing IdentityAccess patterns (`IdentityAccessAppResult`, use case style, endpoint `ToHttpResult`, RBAC `SystemAdministrator`).
- `EndUserProfile.ExternalUserId` is the unique business key from source system; duplicates return conflict.

## Review Focus

- Duplicate `ExternalUserId` on create/update returns conflict, not 500/incorrect overwrite.
- Locked EndUser profile remains queryable by admin but lock/unlock is idempotent-safe and updates `UpdatedAtUtc`.
- Lookup by external id trims input and returns NotFound for missing/blank id with validation behavior consistent with module.
- Cross-module port is in `BuildingBlocks.SharedKernel` and implemented only in IdentityAccess; Delivery/Notification projects gain no IdentityAccess reference.
- New endpoints are protected by `RbacPolicies.SystemAdministrator` and anonymous requests are rejected.

---

### Task 1: Domain behavior and unit tests

**Files:**
- Modify: `src/Modules/IdentityAccess/Domain/EndUserProfiles/EndUserProfile.cs`
- Modify: `tests/UnitTests/IdentityAccess/EndUserProfileTests.cs`

**Interfaces:**
- Consumes: existing `EndUserProfile.Create`, `EndUserProfileStatus`.
- Produces: `void Update(string displayName, string? email, string? phoneNumber, DateTime? updatedAtUtc = null)`, `void Lock(DateTime? updatedAtUtc = null)`, `void Unlock(DateTime? updatedAtUtc = null)`.

- [ ] **Step 1: Write failing tests**

Add tests in `EndUserProfileTests`:
```csharp
[Fact]
public void Update_WithValidData_UpdatesFields()
{
    var profile = EndUserProfile.Create(Guid.NewGuid(), "ext-1", "User 1", "u1@example.com", "0123");
    profile.Update("User 1 Updated", "new@example.com", null);
    Assert.Equal("User 1 Updated", profile.DisplayName);
    Assert.Equal("new@example.com", profile.Email);
    Assert.Null(profile.PhoneNumber);
}

[Fact]
public void Lock_ThenUnlock_ChangesStatus()
{
    var profile = EndUserProfile.Create(Guid.NewGuid(), "ext-1", "User 1", null, null);
    profile.Lock();
    Assert.Equal(EndUserProfileStatus.Locked, profile.Status);
    profile.Unlock();
    Assert.Equal(EndUserProfileStatus.Active, profile.Status);
}
```

- [ ] **Step 2: Run unit test to verify fail**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --filter EndUserProfileTests`
Expected: FAIL because `Update/Lock/Unlock` do not exist.

- [ ] **Step 3: Implement minimal domain behavior**

In `EndUserProfile.cs`, add methods:
```csharp
public void Update(string displayName, string? email, string? phoneNumber, DateTime? updatedAtUtc = null)
public void Lock(DateTime? updatedAtUtc = null)
public void Unlock(DateTime? updatedAtUtc = null)
```
Rules: validate `displayName` required/trimmed; normalize optional fields same as `Create`; set `UpdatedAtUtc`.

- [ ] **Step 4: Run unit test to verify pass**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --filter EndUserProfileTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Modules/IdentityAccess/Domain/EndUserProfiles/EndUserProfile.cs tests/UnitTests/IdentityAccess/EndUserProfileTests.cs
git commit -m "feat(identity-access): add end user profile domain behaviors"
```

### Task 2: Application contracts, DTOs, mapping

**Files:**
- Create: `src/Modules/IdentityAccess/Application/Abstractions/IEndUserProfileRepository.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/EndUserProfileDtos.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/EndUserProfileMapping.cs`

**Interfaces:**
- Produces: repository abstraction and DTOs for use cases/endpoints.
- Signature sketch:
```csharp
public interface IEndUserProfileRepository
{
    Task<EndUserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<EndUserProfile?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<EndUserProfile> Items, int TotalCount)> GetPagedAsync(EndUserProfileStatus? status, string? externalUserId, int page, int pageSize, CancellationToken cancellationToken);
    Task<bool> ExistsByExternalUserIdAsync(string externalUserId, Guid? excludeId, CancellationToken cancellationToken);
    Task AddAsync(EndUserProfile profile, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
```

- [ ] **Step 1: Write failing compile/use-case test placeholder (optional) or proceed with DTO-first**

Create DTO records: `EndUserProfileDto`, `CreateEndUserProfileAppRequest`, `UpdateEndUserProfileAppRequest`, `GetEndUserProfilesAppRequest`, `GetEndUserProfilesResponse`.

- [ ] **Step 2: Add mapping**

`EndUserProfileMapping.ToDto` maps entity to DTO with status string.

- [ ] **Step 3: Build targeted project**

Run: `dotnet build src/Modules/IdentityAccess/Application/IdentityAccess.Application.csproj`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add src/Modules/IdentityAccess/Application/Abstractions/IEndUserProfileRepository.cs src/Modules/IdentityAccess/Application/EndUserProfiles
git commit -m "feat(identity-access): add end user profile application contracts"
```

### Task 3: Application use cases

**Files:**
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/CreateEndUserProfileUseCase.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/UpdateEndUserProfileUseCase.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/GetEndUserProfileByIdUseCase.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/GetEndUserProfilesUseCase.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/GetEndUserProfileByExternalUserIdUseCase.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/LockEndUserProfileUseCase.cs`
- Create: `src/Modules/IdentityAccess/Application/EndUserProfiles/UnlockEndUserProfileUseCase.cs`
- Test: `tests/UnitTests/IdentityAccess/EndUserProfileUseCaseTests.cs`

**Interfaces:**
- Consumes: `IEndUserProfileRepository`, `IdentityAccessAppResult<T>`.
- Produces: use cases with `HandleAsync(...)` returning `IdentityAccessAppResult<EndUserProfileDto>` or paged response.

- [ ] **Step 1: Write failing unit tests**

Create `EndUserProfileUseCaseTests` covering:
```csharp
[Fact] Create_WithValidRequest_ReturnsCreated();
[Fact] Create_WithDuplicateExternalUserId_ReturnsConflict();
[Fact] GetByExternalUserId_WithMissing_ReturnsNotFound();
[Fact] Lock_WithExistingProfile_ReturnsOkAndLockedStatus();
```
Use fake `IEndUserProfileRepository`.

- [ ] **Step 2: Run tests to verify fail**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --filter EndUserProfileUseCaseTests`
Expected: FAIL with missing types/methods.

- [ ] **Step 3: Implement use cases**

Follow UserAccount use case patterns: trim inputs, validation dictionary, conflict on duplicate external id, NotFound when missing. `GetEndUserProfilesUseCase` supports status/externalUserId filter and paging cap 100.

- [ ] **Step 4: Run tests to verify pass**

Run: `dotnet test tests/UnitTests/UnitTests.csproj --filter EndUserProfileUseCaseTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Modules/IdentityAccess/Application/EndUserProfiles tests/UnitTests/IdentityAccess/EndUserProfileUseCaseTests.cs
git commit -m "feat(identity-access): add end user profile use cases"
```

### Task 4: Infrastructure repository, DI, and SharedKernel port

**Files:**
- Create: `src/Modules/IdentityAccess/Infrastructure/Persistence/Repositories/EndUserProfileRepository.cs`
- Create: `src/BuildingBlocks/SharedKernel/EndUserProfiles/EndUserProfileReference.cs`
- Create: `src/BuildingBlocks/SharedKernel/EndUserProfiles/IEndUserProfileReferenceProvider.cs`
- Create: `src/Modules/IdentityAccess/Infrastructure/EndUserProfiles/EndUserProfileReferenceProvider.cs`
- Modify: `src/Modules/IdentityAccess/Infrastructure/Persistence/IdentityAccessPersistenceServiceCollectionExtensions.cs`

**Interfaces:**
- Produces SharedKernel port:
```csharp
namespace BuildingBlocks.SharedKernel.EndUserProfiles;

public sealed record EndUserProfileReference(string ExternalUserId, string DisplayName, string Status, string? Email, string? PhoneNumber);

public interface IEndUserProfileReferenceProvider
{
    Task<EndUserProfileReference?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken);
}
```
- IdentityAccess implements provider using `IdentityAccessDbContext`/repository; registers scoped repository, provider, and use cases.

- [ ] **Step 1: Write failing build/test for provider contract**

Add small unit/integration coverage later; first ensure contract compiles and provider resolves from DI in integration setup.

- [ ] **Step 2: Implement repository and provider**

Repository follows `UserAccountRepository` style with `AsNoTracking` for reads, trim externalUserId, unique checks. Provider returns null for missing/blank.

- [ ] **Step 3: Register DI**

Add `IEndUserProfileRepository`, `IEndUserProfileReferenceProvider`, and all EndUserProfile use cases in `AddIdentityAccessPersistence`.

- [ ] **Step 4: Build solution projects touched**

Run: `dotnet build NotificationSystem.sln`
Expected: PASS (or report env limitation).

- [ ] **Step 5: Commit**

```bash
git add src/BuildingBlocks/SharedKernel/EndUserProfiles src/Modules/IdentityAccess/Infrastructure src/Modules/IdentityAccess/Infrastructure/Persistence/IdentityAccessPersistenceServiceCollectionExtensions.cs
git commit -m "feat(identity-access): add end user profile persistence and reference port"
```

### Task 5: Presentation endpoints

**Files:**
- Modify: `src/Modules/IdentityAccess/Presentation/IdentityAccessEndpointRouteBuilderExtensions.cs`

**Interfaces:**
- Produces endpoints under `/api/admin/end-user-profiles` with `RequireAuthorization(RbacPolicies.SystemAdministrator)`.
- Routes:
  - `GET /` query: `status`, `externalUserId`, `page`, `pageSize`
  - `GET /by-external-id/{externalUserId}`
  - `GET /{id:guid}`
  - `POST /`
  - `PUT /{id:guid}`
  - `POST /{id:guid}/lock`
  - `POST /{id:guid}/unlock`

- [ ] **Step 1: Write failing integration test (Task 6 can drive) or add endpoint skeleton**

Add request records: `CreateEndUserProfileRequest`, `UpdateEndUserProfileRequest`.

- [ ] **Step 2: Implement handlers**

Map requests to app requests/use cases; return `ToHttpResult`. Use `Created` location `/api/admin/end-user-profiles/{id}`.

- [ ] **Step 3: Build**

Run: `dotnet build src/Modules/IdentityAccess/Presentation/IdentityAccess.Presentation.csproj`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add src/Modules/IdentityAccess/Presentation/IdentityAccessEndpointRouteBuilderExtensions.cs
git commit -m "feat(identity-access): expose end user profile admin endpoints"
```

### Task 6: Integration tests and status update

**Files:**
- Create: `tests/IntegrationTests/IdentityAccess/EndUserProfileApiIntegrationTests.cs`
- Modify: `docs/status.md`

**Interfaces:**
- Consumes: existing IdentityAccess integration test setup (`WebApplicationFactory<Program>`, Testcontainers Postgres, JWT env settings, migrations).

- [ ] **Step 1: Write integration tests**

Cover:
```csharp
[Fact] CreateEndUserProfile_WithValidRequest_ReturnsCreated();
[Fact] CreateEndUserProfile_WithDuplicateExternalUserId_ReturnsConflict();
[Fact] GetByExternalUserId_ReturnsProfile();
[Fact] LockAndUnlockEndUserProfile_ChangesStatus();
```
Use SystemAdministrator JWT as in existing integration tests.

- [ ] **Step 2: Run tests to verify**

Run: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter EndUserProfileApiIntegrationTests`
Expected: PASS when env allows Docker/Testcontainers/.NET.

- [ ] **Step 3: Update status doc**

In `docs/status.md`, set current story to ST-P13-06 implemented awaiting Đại ca confirmation; keep ST-P13-06 unchecked. Add ST-P13-06 to "Chờ xác nhận test pass" with previous P13 stories as applicable; do not mark Done.

- [ ] **Step 4: Commit**

```bash
git add tests/IntegrationTests/IdentityAccess/EndUserProfileApiIntegrationTests.cs docs/status.md
git commit -m "test(identity-access): cover end user profile api"
```

## Self-Review

- Spec coverage: create/update/lock/unlock/lookup by external id covered by Tasks 1-6; contract/port covered by Task 4; status update covered by Task 6. AdminUI and P13-09 intentionally out.
- Placeholder scan: commands and signatures are concrete; implementation bodies left to implementer except small signatures/rules.
- Type consistency: DTO/use case/repo names consistent (`EndUserProfile*`); SharedKernel port name used consistently.
- Proportion: plan is structured by decisions, not full production code.
