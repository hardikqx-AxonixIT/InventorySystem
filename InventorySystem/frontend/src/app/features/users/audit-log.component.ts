import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
<div class="page-wrapper">
  <div class="page-header">
    <h1>Audit Log</h1>
    <p class="subtitle">Full history of every create, update, and delete action in the system</p>
  </div>

  <!-- Filters -->
  <div class="filters-bar">
    <input type="text" [(ngModel)]="filter.entityName" placeholder="Filter by entity (e.g. Product)" (ngModelChange)="applyFilter()" />
    <input type="text" [(ngModel)]="filter.action" placeholder="Action (Added, Modified, Deleted)" (ngModelChange)="applyFilter()" />
    <input type="text" [(ngModel)]="filter.userId" placeholder="User ID" (ngModelChange)="applyFilter()" />
    <button class="btn btn-sm btn-outline" (click)="load()">🔄 Refresh</button>
  </div>

  <div *ngIf="loading" class="loading-state">
    <span class="spinner"></span> Loading audit logs...
  </div>
  <div *ngIf="error" class="alert-error">{{ error }}</div>

  <div *ngIf="!loading" class="log-table-wrap">
    <table class="log-table">
      <thead>
        <tr>
          <th>Timestamp</th>
          <th>Entity</th>
          <th>Entity ID</th>
          <th>Action</th>
          <th>User</th>
          <th>Changes</th>
        </tr>
      </thead>
      <tbody>
        <tr *ngFor="let log of filtered" (click)="select(log)" [class.selected]="selected?.id === log.id">
          <td class="ts">{{ log.timestamp | date:'dd MMM yy, HH:mm:ss' }}</td>
          <td><span class="entity-badge">{{ log.entityName }}</span></td>
          <td class="mono">{{ log.entityId }}</td>
          <td>
            <span class="action-badge" [class]="'action-' + (log.action | lowercase)">{{ log.action }}</span>
          </td>
          <td class="user">{{ log.userId }}</td>
          <td class="changes-preview">{{ getChangesSummary(log) }}</td>
        </tr>
        <tr *ngIf="filtered.length === 0">
          <td colspan="6" class="empty-row">No audit log entries found.</td>
        </tr>
      </tbody>
    </table>

    <!-- Pagination -->
    <div class="pagination">
      <button class="btn btn-sm btn-outline" [disabled]="page === 1" (click)="prevPage()">← Prev</button>
      <span class="page-info">Page {{ page }} of {{ totalPages }}</span>
      <button class="btn btn-sm btn-outline" [disabled]="page >= totalPages" (click)="nextPage()">Next →</button>
    </div>
  </div>

  <!-- Detail Panel -->
  <div *ngIf="selected" class="detail-panel">
    <div class="detail-header">
      <strong>{{ selected.entityName }} #{{ selected.entityId }} — {{ selected.action }}</strong>
      <button class="close-btn" (click)="selected = null">✕</button>
    </div>
    <div class="detail-body">
      <div *ngIf="selected.oldValues" class="diff-block">
        <h4>Before</h4>
        <pre>{{ selected.oldValues | json }}</pre>
      </div>
      <div *ngIf="selected.newValues" class="diff-block">
        <h4>After</h4>
        <pre>{{ selected.newValues | json }}</pre>
      </div>
    </div>
  </div>
</div>
  `,
  styles: [`
    .page-wrapper { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .page-header { margin-bottom: 20px; }
    .page-header h1 { font-size: 1.6rem; font-weight: 700; color: #1a202c; margin: 0; }
    .subtitle { color: #718096; margin: 4px 0 0; font-size: 0.9rem; }
    .filters-bar { display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 20px; align-items: center; }
    .filters-bar input { flex: 1; min-width: 160px; border: 1.5px solid #e2e8f0; border-radius: 8px; padding: 8px 12px; font-size: 0.88rem; background: #f7fafc; }
    .filters-bar input:focus { outline: none; border-color: #667eea; background: #fff; }
    .log-table-wrap { background: #fff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 1px 4px rgba(0,0,0,.04); }
    .log-table { width: 100%; border-collapse: collapse; font-size: 0.82rem; }
    .log-table thead tr { background: #f7fafc; border-bottom: 2px solid #e2e8f0; }
    .log-table th { padding: 12px 14px; text-align: left; font-weight: 700; color: #4a5568; text-transform: uppercase; font-size: 0.72rem; letter-spacing: .04em; }
    .log-table td { padding: 11px 14px; border-bottom: 1px solid #f0f4f8; color: #2d3748; vertical-align: middle; }
    .log-table tr:hover { background: #f7fafc; cursor: pointer; }
    .log-table tr.selected { background: #ebf4ff; }
    .ts { color: #718096; white-space: nowrap; }
    .mono { font-family: monospace; font-size: 0.8rem; color: #4a5568; }
    .user { color: #667eea; font-weight: 600; }
    .changes-preview { color: #a0aec0; font-size: 0.78rem; max-width: 220px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .empty-row { text-align: center; padding: 40px; color: #a0aec0; }
    .entity-badge { background: #e9d8fd; color: #553c9a; border-radius: 6px; padding: 2px 8px; font-weight: 600; font-size: 0.75rem; }
    .action-badge { border-radius: 6px; padding: 3px 9px; font-weight: 700; font-size: 0.73rem; }
    .action-added { background: #f0fff4; color: #276749; }
    .action-modified { background: #fffbeb; color: #92400e; }
    .action-deleted { background: #fff5f5; color: #9b2c2c; }
    .pagination { display: flex; align-items: center; gap: 12px; padding: 14px 18px; border-top: 1px solid #f0f4f8; }
    .page-info { color: #718096; font-size: 0.85rem; flex: 1; text-align: center; }
    .btn { padding: 8px 14px; border-radius: 8px; border: none; cursor: pointer; font-weight: 600; font-size: 0.82rem; transition: all .2s; }
    .btn-sm { padding: 6px 12px; }
    .btn-outline { background: #fff; border: 1.5px solid #e2e8f0; color: #4a5568; }
    .btn-outline:hover:not(:disabled) { border-color: #667eea; color: #667eea; }
    .btn-outline:disabled { opacity: .5; cursor: not-allowed; }
    .detail-panel { margin-top: 20px; background: #fff; border-radius: 12px; border: 1px solid #e2e8f0; box-shadow: 0 2px 8px rgba(0,0,0,.06); overflow: hidden; }
    .detail-header { display: flex; justify-content: space-between; align-items: center; padding: 14px 20px; background: #f7fafc; border-bottom: 1px solid #e2e8f0; font-size: 0.9rem; color: #2d3748; }
    .close-btn { background: none; border: none; cursor: pointer; color: #a0aec0; font-size: 1rem; padding: 0 4px; }
    .close-btn:hover { color: #e53e3e; }
    .detail-body { display: grid; grid-template-columns: 1fr 1fr; gap: 0; }
    .diff-block { padding: 16px 20px; }
    .diff-block:first-child { border-right: 1px solid #f0f4f8; }
    .diff-block h4 { font-size: 0.78rem; font-weight: 700; color: #718096; text-transform: uppercase; margin: 0 0 10px; letter-spacing: .05em; }
    .diff-block pre { font-size: 0.78rem; background: #f7fafc; border-radius: 8px; padding: 12px; overflow-x: auto; color: #2d3748; margin: 0; white-space: pre-wrap; word-break: break-all; }
    .loading-state { text-align: center; padding: 60px; color: #718096; }
    .alert-error { background: #fff5f5; color: #c53030; border: 1px solid #feb2b2; padding: 12px; border-radius: 8px; margin-bottom: 16px; }
    .spinner { display: inline-block; width: 20px; height: 20px; border: 2px solid #e2e8f0; border-top-color: #667eea; border-radius: 50%; animation: spin .6s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class AuditLogComponent implements OnInit {
  loading = true;
  error = '';
  logs: any[] = [];
  filtered: any[] = [];
  selected: any = null;

  page = 1;
  pageSize = 30;
  totalPages = 1;

  filter = {
    entityName: '',
    action: '',
    userId: ''
  };

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getAuditLogs({ page: this.page, pageSize: this.pageSize }).subscribe({
      next: (data: any) => {
        this.logs = data?.items ?? data ?? [];
        this.totalPages = Math.max(1, Math.ceil((data?.totalCount ?? this.logs.length) / this.pageSize));
        this.applyFilter();
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load audit logs.';
        this.loading = false;
      }
    });
  }

  applyFilter(): void {
    const entityName = this.filter.entityName.toLowerCase();
    const action = this.filter.action.toLowerCase();
    const userId = this.filter.userId.toLowerCase();

    this.filtered = this.logs.filter(log =>
      (!entityName || (log.entityName ?? '').toLowerCase().includes(entityName)) &&
      (!action || (log.action ?? '').toLowerCase().includes(action)) &&
      (!userId || (log.userId ?? '').toLowerCase().includes(userId))
    );
  }

  select(log: any): void {
    this.selected = this.selected?.id === log.id ? null : log;
  }

  getChangesSummary(log: any): string {
    try {
      const vals = log.newValues ? JSON.parse(log.newValues) : (log.oldValues ? JSON.parse(log.oldValues) : null);
      if (!vals) return '—';
      const keys = Object.keys(vals).slice(0, 3);
      return keys.map(k => `${k}: ${vals[k]}`).join(' • ');
    } catch {
      return '—';
    }
  }

  prevPage(): void {
    if (this.page > 1) { this.page--; this.load(); }
  }

  nextPage(): void {
    if (this.page < this.totalPages) { this.page++; this.load(); }
  }
}
