# Inventory ERP Product Blueprint

## Product Vision

Build an India-ready inventory and ERP platform for distributors, wholesalers, retailers, and light manufacturers. The first release should focus on operational control, GST-compliant billing, and warehouse accuracy while keeping the architecture flexible for accounting, manufacturing, POS, and mobile expansion.

## Target Users

- Business owner who wants sales, stock alerts, and receivable visibility.
- Inventory manager who needs stock accuracy across warehouses and bins.
- Purchase executive who manages vendors, POs, GRNs, and landed cost.
- Sales operator who handles quotations, orders, dispatch, and invoicing.
- Accountant who needs GST, ledgers, receivables, payables, and exports.
- Warehouse staff who scan, pick, transfer, and adjust stock.

## Core Release Scope

### 1. Master Data

- Item master with SKU, item code, barcode, HSN/SAC, brand, category, unit, tax class, reorder level, opening stock, valuation method, batch/serial/expiry flags, active status.
- Category and brand management.
- Units of measure and conversion support for future purchase/sales pack sizes.
- Warehouse master with multiple storage locations.
- Customer master with GSTIN, billing/shipping addresses, payment terms, credit limit, contact details.
- Vendor master with GSTIN, addresses, bank details, payment terms, contact details.

### 2. Inventory Management

- Stock in, stock out, adjustment, reservation, and transfer.
- Available, reserved, ordered, damaged, and in-transit stock views.
- Bin/rack level tracking inside warehouses.
- Batch, serial, and expiry tracking.
- Reorder level alerts and minimum stock thresholds.
- Stock valuation support for FIFO and weighted average.
- Full stock ledger and audit trail.

### 3. Purchase

- Purchase requisition later, purchase order now.
- GRN workflow with partial receipts.
- Purchase invoice and landed cost support.
- Supplier payment tracking.
- Auto stock update on GRN approval.

### 4. Sales

- Quotation, sales order, pick list, delivery challan, invoice, return.
- Order-to-cash flow with partial dispatch and backorders.
- GST-compliant invoice structure.
- Outstanding tracking and payment status.

### 5. India Compliance

- CGST, SGST, IGST logic by place of supply.
- HSN/SAC capture on items.
- GST summary and return-ready reports.
- Future integration hooks for e-invoice and e-way bill providers.

### 6. Accounting Integration

- Auto-posting hooks from purchase, sales, inventory adjustments, and payments.
- Ledger, journal, bank/cash, receivable, payable, P&L, and balance sheet in later phases.
- Tally export/import as a commercial differentiator.

### 7. Warehouse Operations

- Multi-warehouse and multi-bin support.
- Barcode-enabled receiving, picking, transfer, and dispatch.
- Pick-pack-dispatch workflow.
- Stock transfer between warehouses with in-transit status.

### 8. Optional Manufacturing

- BOM, production order, issue/receipt, and finished goods entry.

### 9. Reports

- Stock summary, stock aging, batch expiry, low stock, stock valuation.
- Sales, purchase, margin, outstanding, and demand reports.
- Fast/slow-moving analysis and reorder suggestions.

### 10. Security

- Role-based access for admin, manager, accountant, warehouse staff, and sales staff.
- Audit logs for stock and master changes.
- Approval workflows for sensitive stock adjustments.

### 11. Mobile and Owner Dashboard

- Today sales, low stock, pending approvals, outstanding collections, top products.
- Mobile-friendly UI from day one.

## Recommended Module Roadmap

### Phase 1

- Authentication and roles
- Dashboard
- Master data
- Product catalog
- Warehouse/bin master
- Stock adjustments and stock ledger
- Basic inventory overview

### Phase 2

- Purchase order and GRN
- Sales order and invoice
- GST-ready invoice calculations
- Reports

### Phase 3

- Accounting integration
- Barcode flows
- Tally/Excel integrations
- WhatsApp invoice delivery
- Offline/POS support

### Phase 4

- Manufacturing
- Mobile app
- AI demand prediction

## Backend Architecture

- Frontend: Angular admin panel and future POS/mobile clients.
- Backend: ASP.NET Core Web API with clean architecture.
- Database: PostgreSQL preferred for production; SQL Server/localdb acceptable for development.
- Identity: ASP.NET Core Identity with JWT auth.
- Persistence: EF Core with audit logging and transaction-safe stock movements.

## Key Database Areas

- Security: users, roles, permissions, audit_logs
- Masters: products, item_categories, brands, units, warehouses, warehouse_bins, customers, vendors
- Inventory: stock_levels, stock_ledgers, stock_reservations, stock_transfers, stock_transfer_items, batches, serial_numbers
- Purchase: purchase_orders, purchase_order_items, grns, grn_items, purchase_invoices
- Sales: quotations, sales_orders, sales_order_items, delivery_challans, invoices, invoice_items, sales_returns
- Finance: ledgers, journal_entries, payments, receivables, payables

## Immediate Build Goal

Start with a usable inventory foundation:

1. Dashboard with ERP module overview and live KPIs.
2. Master data entities for products, categories, units, warehouses, customers, and vendors.
3. Inventory overview APIs.
4. Purchase and sales modules as the next vertical slices.
