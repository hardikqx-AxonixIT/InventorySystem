# Inventory System Technical Architecture

## 1. Technology Stack
- Frontend: Angular (standalone components), TypeScript, CSS
- Backend: ASP.NET Core Web API (.NET 6)
- Database: SQL Server with Entity Framework Core (Code-First migrations)
- Auth: ASP.NET Identity + role-based access model
- Integrations: WhatsApp Business API (config-driven), Razorpay API + callback verification

## 2. Solution Structure
- `frontend/`: Angular app (dashboard, masters, purchase, sales, GST, accounting, warehouse, manufacturing, users, POS)
- `InventorySystem.WebAPI/`: API controllers and startup
- `InventorySystem.Application/`: business services and DTOs
- `InventorySystem.Domain/`: entity models and enums
- `InventorySystem.Infrastructure/`: EF DbContext, migrations, seed data, Identity setup

## 3. Core Runtime Architecture
- UI calls API through service classes (`transaction-data.service.ts`, `user-management.service.ts`)
- Controllers delegate business logic to application services:
  - `TransactionService` for core purchase/sales/inventory/accounting snapshots
  - `AdvancedErpService` for enterprise workflows (GST exports, quotation/challan/returns, BOM/work-orders, permission matrix, audit explorer, integrations)
- Service layer writes to EF DbContext and executes stock/accounting effects in transactional flows

## 4. Key API Surface
- Core: `api/Transactions/*`
- Advanced: `api/AdvancedOperations/*`
- User management: `api/UserManagement/*`
- Dashboard: `api/Dashboard/overview`

## 5. Data Model Coverage
- Masters: products, categories, units, warehouses, bins, customers, vendors
- Transactions: purchase orders, GRN, purchase invoices, supplier payments, sales orders, sales invoices
- Advanced Sales: quotations, delivery challans, sales returns
- Warehouse Advanced: transfer requests/approvals, pick list and pick scan lines
- Manufacturing Advanced: BOM templates/items, production work orders
- GST Advanced: e-invoice records (IRN), e-way bill records
- Accounting Advanced: ledgers, journal vouchers, voucher lines
- Security/Audit: role permissions matrix, audit logs
- Integrations: payment callback logs

## 6. GST, Inventory, Accounting Coupling
- GST split logic (intra/inter-state) is computed in service layer
- Sales order reserves stock; sales invoice deducts reserved stock
- GRN posts stock-in; transfer approval posts out/in stock moves
- Sales return posts stock back in
- Journal vouchers can be posted manually from accounting UI

## 7. Configuration Requirements
Set in backend app settings/environment:
- `Integrations:WhatsApp:AccessToken`
- `Integrations:WhatsApp:PhoneNumberId`
- `Integrations:Razorpay:KeyId`
- `Integrations:Razorpay:KeySecret`

If these are missing, integration endpoints return safe “not configured” responses.

## 8. Build and Run
1. `dotnet build InventorySystem.sln`
2. `dotnet ef database update --project InventorySystem.Infrastructure --startup-project InventorySystem.WebAPI`
3. Run API: `dotnet run --project InventorySystem.WebAPI --urls http://localhost:5157`
4. Frontend: `cd frontend && npm install && npm start`

## 9. Migrations (latest enterprise additions)
- `20260321231159_AddAdvancedEnterpriseModules`
- `20260321231227_FixDeliveryChallanPrecision`

## 10. Known Extension Points
- Replace simulated GST IRN/e-way generation with official GSP connector
- Add webhook controller security hardening and idempotency keys for callbacks
- Add approval workflow notifications (email/WhatsApp/internal queue)
- Add full accounting auto-post map per document event
