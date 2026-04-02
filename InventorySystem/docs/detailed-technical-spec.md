# Detailed Technical Specification: Axonix Inventory System

## 1. Project Overview
**Axonix Inventory System** is a premium, enterprise-grade ERP solution tailored for the Indian SME market. It is built on a modern, scalable stack focusing on high performance, robust security, and full GST compliance.

- **Objective:** To provide a complete, localized ERP experience with real-time inventory tracking, multi-warehouse management, and deep accounting integration.
- **Key Focus Areas:** India-first GST compliance, Role-Based Access Control (RBAC) with profit masking, rapid-fire barcode/serial scanning, and mobile responsiveness.

---

## 2. System Architecture

The project follows **Clean Architecture** principles to ensure maintainability, testability, and separation of concerns.

### 2.1 Backend Architecture (.NET Core 6+)
- **Domain Layer (`InventorySystem.Domain`):** 
  - Contains POCO entities (Products, Customers, Vendors, Transactions).
  - Business rules and Enums (e.g., Transaction Status, Stock Movement Types).
  - Base classes for audit trails (`CreatedBy`, `ModifiedAt`).
- **Application Layer (`InventorySystem.Application`):**
  - Interfaces for services (`ITransactionService`, `IExportService`).
  - DTOs (Data Transfer Objects) for API requests and responses.
  - Business logic orchestration (GST calculations, stock reservation logic).
  - Validators using FluentValidation.
- **Infrastructure Layer (`InventorySystem.Infrastructure`):**
  - EF Core `DbContext` implementation with SQL Server.
  - Migrations and Seed Data.
  - Implementation of external services (WhatsApp, Razorpay).
  - ASP.NET Identity configuration and Role/Permission setup.
- **WebAPI Layer (`InventorySystem.WebAPI`):**
  - RESTful Controllers.
  - Middleware (Exception handling, JWT Auth, Audit logging).
  - Configuration management (`appsettings.json`).

### 2.2 Frontend Architecture (Angular)
- **Framework:** Angular (latest) using Standalone Components for reduced boilerplate.
- **Design System:** Custom CSS reflecting a premium "Apple-style" aesthetic (glassmorphism, subtle micro-animations, curated color palettes).
- **State Management:** Service-based state management using RxJS observables and Subjects.
- **Feature Modules:**
  - `Masters`: CRUD for core entities.
  - `Sales & Purchase`: Full transaction lifecycle management.
  - `Warehouse`: Stock moves, transfers, and barcode scanning.
  - `GST`: Compliance reports and e-invoice generation.
  - `Accounting`: Ledgers, Profit & Loss, Balance Sheet.

---

## 3. Core Business Workflows

### 3.1 Advanced Sales Lifecycle
1. **Quotation:** Non-stock-affecting proposal.
2. **Sales Order:** Reserves stock in the warehouse to prevent double-selling.
3. **Delivery Challan:** Goods movement without financial impact (yet).
4. **Sales Invoice:** Financial transaction, GST posting, and final stock deduction.
5. **Credit Management:** Automatic checks against `Customer.CreditLimit` and `Customer.PaymentTermsDays`.

### 3.2 Intelligent Inventory Management
- **Stock Tracking:** Real-time balance updates at the Warehouse/Bin level.
- **Scanning:** `ScanBatchToOrderAsync` enables rapid barcode/serial scanning directly into orders.
- **Reorder Logic:** Auto-suggestions based on `ReorderLevel` and `DemandPredictionAsync`.
- **In-Transit Tracking:** For inter-warehouse transfers.

### 3.3 Finance & GST Engine
- **Tax Calculation:** Supports IGST (Inter-state) and CGST/SGST (Intra-state) splits based on Gstin analysis.
- **E-Invoice (IRN) / E-Way Bill:** System-generated simulation, ready for GSP integration hookup.
- **Double-Entry Accounting:** Automated ledger posting for every transaction.

---

## 4. Security & Compliance

### 4.1 Granular RBAC (Profit Masking)
The system implements a security-first approach to information visibility:
- **Profit Masking:** Roles like "Warehouse Staff" or "Sales Executive" can view stock quantities but are masked from seeing cost prices or profit margins.
- **Permissions Matrix:** Fine-grained control over View/Edit/Delete/Export permissions per module.

### 4.2 Audit & Traceability
- **Entity Auditing:** Every change records the user ID and timestamp.
- **Audit Explorer:** A dedicated UI for administrators to track data modifications across the system.

---

## 5. Integrations

### 5.1 WhatsApp Business API
- **Automated Alerts:** Invoices, payment receipts, and low-stock alerts sent directly to customers/vendors.
- **Config-Driven:** Easily swappable tokens and IDs in `appsettings.json`.

### 5.2 Razorpay / UPI
- **Direct Payments:** QR code generation and payment link integration.
- **Webhook Verification:** Secure callback validation using HMAC-SHA256 signatures to prevent payment spoofing.

---

## 6. Technical Implementation Details

### 6.1 Database Schema
Key entities include:
- `BaseEntity`: (Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted).
- `Product`: (Name, SKU, Barcode, HSN, TaxRate, PurchasePrice, SalePrice).
- `Customer`: (Name, Gstin, Phone, CreditLimit, PaymentTermsDays).
- `StockMovement`: (ProductId, WarehouseId, Quantity, Type).
- `TransactionDetail`: (ParentId, Type, Quantity, Price, TaxAmount).

### 6.2 Advanced API Features
- **Pagination & Filtering:** Standardized across all `Paged` endpoints.
- **Unit of Work:** Transactional integrity for complex operations (e.g., Sales Invoice creating Inventory deduction + Ledger post).
- **Export Service:** CSV/Excel generation for GSTR reports.

---

## 7. Developer Guide

### 7.1 Local Environment Setup
1. **Backend:**
   ```bash
   dotnet build InventorySystem.sln
   dotnet ef database update --project InventorySystem.Infrastructure --startup-project InventorySystem.WebAPI
   dotnet run --project InventorySystem.WebAPI --urls http://localhost:5157
   ```
2. **Frontend:**
   ```bash
   cd frontend
   npm install
   npm start
   ```

### 7.2 Key Configuration
Update `appsettings.json` for:
- `ConnectionStrings:DefaultConnection`
- `Jwt:Secret`
- `Integrations:WhatsApp`
- `Integrations:Razorpay`

---

## 8. Roadmap & Extension Points
- **GSP Direct Integration:** Replace simulated IRN/E-Way flows with official NIC/IRIS APIs.
- **Mobile App:** Potential Flutter/React Native wrapper for the responsive Angular PWA.
- **AI Forecasting:** Enhancing `DemandPredictionAsync` with more sophisticated time-series models.
