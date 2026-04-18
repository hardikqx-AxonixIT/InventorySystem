# System Features and Workflows (Client Guide)

## 1. What This System Covers
This ERP currently supports:
- Master setup
- Purchase and sales operations
- GST and compliance-ready flows
- Inventory and warehouse operations
- Accounting and collections visibility
- User management with role controls
- Integrations (WhatsApp, Razorpay/UPI, Tally/GSP-ready endpoints)
- Commercial SaaS controls (plans, trial, license)
- Backup operations API

---

## 2. Security and Access Workflow
1. User logs in via `Auth/login` and receives JWT.
2. JWT is attached by frontend interceptor for API calls.
3. Protected modules require authenticated access.
4. Unauthorized requests are blocked and redirected to login.

---

## 3. Master Setup Workflow
1. Create categories and units.
2. Create products with SKU/barcode/HSN/GST.
3. Create warehouses and bins.
4. Add customers and vendors with GST/payment terms.

---

## 4. Purchase Workflow
1. Create PO.
2. Receive GRN (full/partial).
3. Generate purchase invoice.
4. Record supplier payment.

System impact:
- Stock increases on GRN.
- Payable values and GST are tracked.

---

## 5. Sales Workflow
1. Create quotation.
2. Convert quotation to sales order.
3. Create delivery challan.
4. Create sales invoice.
5. Process return if required.
6. Use quick invoice shortcut for reduced steps.

System impact:
- Stock reservation at order stage.
- Stock deduction at invoice stage.

---

## 6. GST Workflow
1. Run GSTR-1 and GSTR-3B exports.
2. Generate IRN/e-way records from UI.
3. Trigger GSP filing endpoint from GST automation section.

Note:
- Direct provider integration is endpoint-ready and configuration-driven.

---

## 7. Inventory and Warehouse Workflow
1. Stock in/out/transfer.
2. Transfer approvals.
3. Pick list creation and scan flow.
4. Pack/dispatch confirmation.

---

## 8. Accounting and Collections Workflow
1. Review accounting KPIs.
2. Post vouchers and drill into ledgers.
3. View P&L and balance sheet.
4. Use collections tab for aging, reminder trigger, and overdue interest.

---

## 9. User and Permission Workflow
1. Create users and assign roles.
2. Edit users inline (name/email/role).
3. Activate/deactivate users.
4. Maintain permission matrix.
5. Review audit logs.

---

## 10. Integrations Workflow
### 10.1 WhatsApp
- Send invoice notifications.

### 10.2 Razorpay
- Create order and verify callback signature.

### 10.3 Tally
- Export XML.
- Trigger import and ledger/voucher sync endpoints.

### 10.4 GST GSP
- Trigger direct filing endpoint from GST module.

---

## 11. Backup and Recovery Workflow
1. Create system backup via API/UI action.
2. List available backups.
3. Restore from SQL `.bak` file.

Note:
- If SQL backup is not available in environment, system creates JSON snapshot fallback backup.

---

## 12. SaaS Commercial Workflow
1. Load subscription plans.
2. Start tenant trial.
3. Activate subscription plan.
4. Generate license key.
5. Validate license.

---

## 13. Suggested Daily Usage Pattern
1. Check dashboard KPIs.
2. Process purchase and sales queues.
3. Resolve stock alerts.
4. Review collections and pending invoices.
5. Run GST and backup tasks as scheduled.
