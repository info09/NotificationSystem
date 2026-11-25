# ST-P13-06 EndUser Profile Management - Short Design

## Intent
Implement ST-P13-06: manage `EndUserProfile` (create/update/lock/unlock, lookup by source identifier) in IdentityAccess, and provide a cross-module contract/port for Notification/InAppNotification to reference EndUser by business identity.

## Scope
- In: IdentityAccess domain behavior, application use cases/repos/DTOs, presentation endpoints, DI, SharedKernel contract/port + IdentityAccess implementation, unit/integration/e2e tests as applicable, status doc update.
- Out: AdminUI screens (ST-P13-08), broader P13 tests consolidation (ST-P13-09), IdP integration.

## Decisions (approved by Đại ca)
- Domain: add `EndUserProfile.Update(...)`, `Lock(...)`, `Unlock(...)`, update `UpdatedAtUtc`.
- Application: `IEndUserProfileRepository` + use cases `Create/GetById/GetPaged/Update/Lock/Unlock/GetByExternalUserId`; validate/conflict on duplicate `ExternalUserId`.
- Presentation: `/api/admin/end-user-profiles`, RBAC `SystemAdministrator`; GET list, GET by id, GET by external id/query, POST, PUT, POST lock/unlock.
- Cross-module contract/port: interface in `BuildingBlocks.SharedKernel` (business reference DTO, ExternalUserId/status/display name as needed), implemented in IdentityAccess Infrastructure, registered in DI. No direct module reference.
- DoD: do not mark Done until Đại ca confirms related unit+integration+e2e pass.

## Risks / Notes
- Endpoint shape for lookup by external id to be concrete during implementation (route segment or query) following existing conventions.
- Port naming kept business-focused and minimal.
- Environment cannot run tests; report files/tests added for Đại ca verification.
