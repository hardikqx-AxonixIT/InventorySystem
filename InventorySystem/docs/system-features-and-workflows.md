# System Features and Workflows (Client Guide)

## 1. What This System Covers
This ERP includes day-to-day operations for:
- Master setup
- Purchase
- Sales
- GST
- Inventory and warehouse
- Accounting
- Manufacturing
- User permissions and audit logs
- Integrations (WhatsApp and Razorpay/UPI)

---

## 2. Master Setup Workflow
1. Create categories and units.
2. Create products with SKU, barcode, HSN, brand, GST rate, reorder level.
3. Create warehouses and bins (rack/bin-ready structure).
4. Add customers and vendors with GST and payment terms.

Result: system is ready for purchase/sales transactions.

---

## 3. Purchase Workflow
1. Create Purchase Order (PO).
2. Receive goods through GRN (full or partial).
3. Generate Purchase Invoice from GRN.
4. Record Supplier Payment.

System impact:
- Stock increases on GRN.
- Payable and invoice balance are tracked.
- GST purchase values are included in reports.

---

## 4. Sales Workflow (Advanced)
1. Create Quotation.
2. Convert Quotation to Sales Order.
3. Generate Delivery Challan.
4. Create Sales Invoice.
5. Process Sales Return (if needed).

System impact:
- Stock is reserved at sales order stage.
- Stock is deducted at invoice stage.
- Returned quantity is added back to stock.
- GST is applied in invoice values.

---

## 5. GST Compliance Workflow
1. Generate GSTR-1 summary/export.
2. Generate GSTR-3B summary/export.
3. Generate e-Invoice IRN for selected sales invoice.
4. Generate e-Way Bill with vehicle and distance details.

Note:
- Current IRN/e-way flow is system-generated and integration-ready.
- Direct government portal/GSP submission can be connected as next phase.

---

## 6. Inventory and Warehouse Workflow
1. Perform stock in/out/transfer.
2. Raise transfer request for controlled movement.
3. Approve/reject transfer from approval queue.
4. Create pick list from sales order.
5. Scan barcode to mark picked quantity.
6. Mark pick list packed and dispatch.

Result: warehouse operations are traceable and controlled.

---

## 7. Accounting Workflow
1. Review accounting summary dashboard.
2. Post Journal Voucher (manual entries).
3. View ledger drill-down.
4. View Profit & Loss.
5. View Balance Sheet.

Result: finance team can monitor books and track profitability.

---

## 8. Manufacturing Workflow
1. Create BOM template for finished goods.
2. Create production work order with planned date and quantity.
3. Release work order.

System impact:
- Component stock is consumed.
- Finished goods stock is produced to output bin.

---

## 9. User Access and Audit Workflow
1. Create users and assign roles.
2. Configure granular permission matrix by module.
3. Use audit log explorer to track changes (entity/action/user/time).

Result: controlled access and traceability for compliance.

---

## 10. Integrations Workflow
### WhatsApp Invoice
1. Select invoice and phone number.
2. Send via WhatsApp integration endpoint.

### Razorpay / UPI
1. Create payment order from system.
2. Verify gateway callback signature.
3. Store callback log for audit/security.

---

## 11. Roles (Recommended)
- Super Admin: full access
- Inventory Manager: stock + warehouse + approvals
- Sales Manager: quotation/order/invoice/returns
- Accounts Manager: vouchers, ledgers, GST review
- Warehouse Staff: pick/pack/stock handling only

---

## 12. Suggested Daily Usage Pattern
1. Check dashboard KPIs and action queue.
2. Process pending sales dispatch and purchase receipts.
3. Resolve low-stock and create replenishment actions.
4. Close payment and accounting entries.
5. Run GST summaries at period-end.
