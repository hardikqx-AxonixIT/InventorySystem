# Inventory System Technical Architecture

## 1. Technology Stack
- Frontend: Angular standalone components, TypeScript, CSS
- Backend: ASP.NET Core Web API (.NET 6)
- Database: SQL Server with Entity Framework Core
- Auth: ASP.NET Identity + JWT (issuer/audience aware)
- Integrations: WhatsApp, Razorpay, Tally/GSP integration-ready endpoints

## 2. Solution Structure
- `frontend/`: Angular application
- `InventorySystem.WebAPI/`: API + middleware + startup
- `InventorySystem.Application/`: application services and DTOs
- `InventorySystem.Domain/`: domain entities and enums
- `InventorySystem.Infrastructure/`: EF context, seeders, identity config
- `.github/workflows/`: CI automation

## 3. Runtime and Middleware Pipeline
`ExceptionHandler -> ResponseCompression -> CORS -> SecurityHeaders -> RateLimit -> Idempotency -> AuthN -> AuthZ -> Controllers`

Key middleware introduced:
- `SecurityHeadersMiddleware`
- `SimpleRateLimitMiddleware`
- `IdempotencyMiddleware`

## 4. Security Model
- Global fallback auth policy enabled via configuration.
- Most operational endpoints now require JWT.
- Public endpoints intentionally limited to auth/bootstrap-sensitive routes only.
- JWT token validation now supports:
  - Signing key
  - Issuer
  - Audience
  - Clock skew control

Config sections:
- `Jwt`
- `Security`
- `Backup`
- `Integrations`

## 5. Key API Surface (Current)
- Core: `api/Transactions/*`
- Advanced: `api/AdvancedOperations/*`
- User management: `api/UserManagement/*`
- Commercial: `api/commercial/*`
- System backup: `api/system/backup/*`
- Export: `api/Export/*`

New endpoints added:
- `POST /api/AdvancedOperations/integrations/tally/import-masters`
- `POST /api/AdvancedOperations/integrations/tally/sync-ledgers-vouchers`
- `POST /api/AdvancedOperations/gst/gsp/file`
- `GET /api/system/backup/list`
- `POST /api/system/backup/create`
- `POST /api/system/backup/restore`
- `GET /api/commercial/plans`
- `POST /api/commercial/trial/start`
- `POST /api/commercial/subscription/activate`
- `POST /api/commercial/license/generate`
- `POST /api/commercial/license/validate`
- `GET /api/commercial/subscription/{tenantId}`

## 6. Backup Strategy
- Primary mode: SQL `.bak` backup and restore.
- Fallback mode: logical JSON snapshot backup when SQL backup is unavailable.
- Backup behavior is controlled by `Backup:BackupDirectory`.

## 7. Frontend Performance Architecture
- Angular routes migrated to lazy-loaded `loadComponent` pattern.
- Initial bundle reduced to ~379 KB.
- Larger modules are now delivered as lazy chunks.

## 8. Build and Run
1. `dotnet build InventorySystem.sln`
2. `dotnet run --project InventorySystem.WebAPI --urls http://localhost:5157`
3. `cd frontend && npm install && npm start`

## 9. CI/CD
- GitHub Actions workflow validates:
  - .NET restore/build
  - Angular install/build
- Current CI is build-focused; test gates are next step.

## 10. Known Remaining Gaps
- Production-grade secrets management and key rotation
- Persistent commercial subscription store (current service is in-memory)
- Real provider connector implementation for Tally/GSP/NIC
- Automated test coverage and release gating
- Full restore automation for fallback snapshot mode
