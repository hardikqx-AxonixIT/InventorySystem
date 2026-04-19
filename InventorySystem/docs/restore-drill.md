# Backup restore drill (operations)

## Goals

- Verify **RPO** (how much data you can lose) and **RTO** (how long to become operational again) for your deployment.
- Confirm both **SQL `.bak` restore** and expectations for **JSON snapshot fallback**.

## Prerequisites

- Staging or disposable environment that mirrors production connection string pattern.
- `Backup:BackupDirectory` writable by the API process.
- For SQL path: permission to run `BACKUP DATABASE` / `RESTORE DATABASE` (or DBA-assisted restore).

## Drill A: SQL backup and restore

1. Note current row counts or a checksum query for `Products`, `SalesInvoices` (optional baseline).
2. Call `POST /api/system/backup/create` with a SuperAdmin JWT (or use UI action wired to the same API).
3. Confirm a `.bak` appears under the configured backup directory when SQL backup succeeds.
4. Restore on a **separate** SQL instance or database name:
   - Stop the API or point it to the new DB.
   - Run `RESTORE DATABASE ... FROM DISK = '...bak' WITH REPLACE` (adjust paths; use DBA runbook).
5. Start the API against the restored database; run `GET /api/Transactions/bootstrap` and spot-check critical flows.

**Pass criteria:** Application starts; authentication works; sample transactions visible; no migration errors.

## Drill B: JSON snapshot fallback

When SQL backup is unavailable, the API may return `mode = json-snapshot`. Treat this as **logical export**, not a full native DB restore.

1. Create a snapshot backup via the same API.
2. Document that full **round-trip restore** into SQL Server may require a custom import procedure; use this mode for **audit exports and disaster copies**, not as the only production recovery path unless scripted.

**Pass criteria:** Team knows when snapshot mode is used; RPO/RTO assumptions are documented for that mode.

## Secrets

- Prefer environment variable `JWT__KEY` (or `Jwt__Key`) for signing keys in production instead of committed configuration.

## Redis (optional)

- Set `Redis:ConnectionString` to enable shared **rate limiting** and **idempotency** state across multiple API instances. If empty, the API uses in-process distributed memory cache (single-node friendly).
