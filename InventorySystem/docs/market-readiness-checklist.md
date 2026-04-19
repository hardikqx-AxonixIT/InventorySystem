# Market Readiness Checklist (Go-to-Market Preparation)

## Status Legend
- `Live`: Implemented and operational.
- `Partial`: Implemented but needs provider/infra/data-hardening for production scale.
- `Gap`: Not implemented.

## 1. Product Capability Readiness

### 1.1 Core Operations
- Masters + inventory + purchase + sales: `Live`
- POS quick flow + barcode add: `Partial` (pending device/printer validation)
- GST summaries + reporting: `Live`
- e-Invoice/e-Way + GSP direct filing: `Partial` (API ready, provider connector pending)
- Tally import/sync + accounting bridge: `Partial` (API ready, provider connector pending)

### 1.2 Finance & Collections
- Outstanding aging view: `Live`
- Reminder trigger flow: `Partial` (provider templates/config required)
- Overdue interest computation: `Live` (basic formula)

### 1.3 Reports for Decisions
- Fast/slow + low stock + dead stock: `Live`
- Profit by item/customer (estimated): `Live`
- COGS-accurate profitability: `Gap`

## 2. Engineering Readiness

### 2.1 Security
- Authenticated API baseline enforced: `Live`
- JWT issuer/audience model added: `Live`
- Rate limiting middleware: `Live`
- Idempotency middleware: `Live`
- Secrets vault + key rotation policy: `Gap`
- Automated security scanning (SAST/DAST/dependency policy): `Gap`

### 2.2 Reliability & Data Safety
- Backup APIs implemented: `Live`
- SQL backup + logical fallback mode: `Live`
- One-click tested full restore across environments: `Partial`
- Monitoring/alerting + incident response: `Gap`

### 2.3 Performance & Scale
- Frontend lazy loading and bundle reduction: `Live`
- API/db load testing and tuning baseline: `Gap`
- Distributed cache/rate-limit/idempotency stores (Redis): `Gap`

### 2.4 Quality & Release
- CI pipeline (frontend + backend build): `Live`
- Automated tests with coverage gates: `Gap`
- Staging/prod parity and controlled deployment strategy: `Gap`

## 3. Commercial Readiness

### 3.1 SaaS Business Features
- Plan/trial/license API and UI flow: `Live` (MVP)
- Durable subscription persistence and billing-grade enforcement: `Partial`
- Razorpay subscription lifecycle automation: `Gap`

### 3.2 Compliance & Legal
- Privacy policy / terms / DPA / retention policy: `Gap`
- Compliance disclaimers for GST/e-invoice workflows: `Gap`

### 3.3 Customer Success
- In-app onboarding and setup wizard: `Gap`
- Tenant-level support playbook and SLA integration: `Gap`

## 4. Recommended Next Steps (Launch-Focused)
1. Move commercial module from in-memory to persistent DB entities.
2. Implement real Tally and GSP/NIC provider connectors.
3. Add test suite (API integration + critical frontend e2e) and enforce CI gating.
4. Add monitoring/alerts and environment secrets management.
5. Complete restore drill automation and document RPO/RTO.

## 5. Go/No-Go Criteria
Launch when all are true:
- Auth hardening and access reviews complete.
- Backup restore drill passed in staging.
- CI includes tests (not build-only).
- Provider-critical flows are either production integrated or feature-flagged off.
- Monitoring and incident response are active.
