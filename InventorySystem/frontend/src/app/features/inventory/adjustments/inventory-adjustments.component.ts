import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryService } from '../../../core/services/inventory.service';
import { TransactionDataService } from '../../../core/services/transaction-data.service';

interface AdjustmentReasonCode {
  code: string;
  label: string;
}

@Component({
  selector: 'app-inventory-adjustments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
<div class="page-wrapper">
  <div class="page-header">
    <h1>Inventory Adjustments</h1>
    <p class="subtitle">Request and approve stock corrections with full audit trail</p>
  </div>

  <div *ngIf="loading" class="loading-state">
    <span class="spinner"></span> Loading...
  </div>
  <div *ngIf="error" class="alert alert-error">{{ error }}</div>
  <div *ngIf="success" class="alert alert-success">{{ success }}</div>

  <div class="two-col" *ngIf="!loading">
    <!-- Create Adjustment Request -->
    <div class="card">
      <h2 class="card-title">New Adjustment Request</h2>
      <form (ngSubmit)="submitAdjustment()">
        <div class="form-field">
          <label>Product</label>
          <select [(ngModel)]="form.productId" name="productId" required>
            <option [value]="0" disabled>— Select Product —</option>
            <option *ngFor="let p of products" [value]="p.id">{{ p.name }} ({{ p.sku }})</option>
          </select>
        </div>

        <div class="form-field">
          <label>Adjustment Quantity <span class="hint">(use negative for write-off)</span></label>
          <input type="number" [(ngModel)]="form.amount" name="amount" step="0.01" required />
        </div>

        <div class="form-field">
          <label>Reason Code</label>
          <select [(ngModel)]="form.reason" name="reason" required>
            <option value="" disabled>— Select Reason —</option>
            <option *ngFor="let r of reasonCodes" [value]="r.code">{{ r.label }}</option>
          </select>
        </div>

        <div class="form-field">
          <label>Notes (optional)</label>
          <textarea [(ngModel)]="form.notes" name="notes" rows="2" placeholder="Additional context..."></textarea>
        </div>

        <button type="submit" class="btn btn-primary" [disabled]="submitting">
          <span *ngIf="submitting" class="spinner-sm"></span>
          {{ submitting ? 'Submitting...' : 'Submit Adjustment' }}
        </button>
      </form>
    </div>

    <!-- Pending Adjustments -->
    <div class="card">
      <h2 class="card-title">Pending Approvals
        <span class="badge-count">{{ pendingAdjustments.length }}</span>
      </h2>

      <div *ngIf="pendingAdjustments.length === 0" class="empty-state">
        <span class="checkmark">✓</span>
        <p>No pending adjustments</p>
      </div>

      <div *ngFor="let adj of pendingAdjustments" class="adjustment-card">
        <div class="adj-header">
          <span class="product-name">{{ getProductName(adj.productId) }}</span>
          <span class="qty" [class.positive]="adj.requestedAmount > 0" [class.negative]="adj.requestedAmount < 0">
            {{ adj.requestedAmount > 0 ? '+' : '' }}{{ adj.requestedAmount }}
          </span>
        </div>
        <div class="adj-meta">
          <span class="reason-tag">{{ adj.reasonCode }}</span>
          <span class="date">{{ adj.createdAt | date:'dd MMM, HH:mm' }}</span>
        </div>
        <button class="btn btn-approve" (click)="approve(adj.id)">✓ Approve</button>
      </div>
    </div>
  </div>
</div>
  `,
  styles: [`
    .page-wrapper { padding: 24px; max-width: 1100px; margin: 0 auto; }
    .page-header { margin-bottom: 24px; }
    .page-header h1 { font-size: 1.6rem; font-weight: 700; color: #1a202c; margin: 0; }
    .subtitle { color: #718096; margin: 4px 0 0; font-size: 0.9rem; }
    .two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    @media (max-width: 768px) { .two-col { grid-template-columns: 1fr; } }
    .card { background: #fff; border-radius: 12px; padding: 24px; border: 1px solid #e2e8f0; box-shadow: 0 1px 4px rgba(0,0,0,.04); }
    .card-title { font-size: 1rem; font-weight: 600; color: #2d3748; margin: 0 0 20px; display: flex; align-items: center; gap: 10px; }
    .badge-count { background: #667eea; color: #fff; border-radius: 12px; padding: 2px 8px; font-size: 0.75rem; }
    .form-field { margin-bottom: 16px; }
    .form-field label { display: block; font-size: 0.82rem; font-weight: 600; color: #4a5568; margin-bottom: 6px; }
    .hint { font-weight: 400; color: #a0aec0; font-size: 0.78rem; }
    .form-field select, .form-field input, .form-field textarea {
      width: 100%; border: 1.5px solid #e2e8f0; border-radius: 8px; padding: 9px 12px;
      font-size: 0.9rem; background: #f7fafc; box-sizing: border-box;
      transition: border-color .2s;
    }
    .form-field select:focus, .form-field input:focus, .form-field textarea:focus {
      outline: none; border-color: #667eea; background: #fff;
    }
    .btn { padding: 10px 20px; border-radius: 8px; border: none; cursor: pointer; font-weight: 600; font-size: 0.88rem; transition: all .2s; }
    .btn-primary { background: linear-gradient(135deg, #667eea, #764ba2); color: #fff; width: 100%; display: flex; align-items: center; justify-content: center; gap: 8px; }
    .btn-primary:hover { opacity: .92; transform: translateY(-1px); }
    .btn-primary:disabled { opacity: .6; cursor: not-allowed; transform: none; }
    .btn-approve { background: #48bb78; color: #fff; padding: 6px 14px; font-size: 0.8rem; border-radius: 6px; margin-top: 10px; }
    .btn-approve:hover { background: #38a169; }
    .adjustment-card { border: 1px solid #e2e8f0; border-radius: 10px; padding: 14px; margin-bottom: 12px; }
    .adj-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .product-name { font-weight: 600; color: #2d3748; font-size: 0.9rem; }
    .qty { font-weight: 700; font-size: 1rem; }
    .qty.positive { color: #38a169; }
    .qty.negative { color: #e53e3e; }
    .adj-meta { display: flex; gap: 10px; align-items: center; }
    .reason-tag { background: #ebf4ff; color: #3182ce; border-radius: 6px; padding: 2px 8px; font-size: 0.75rem; font-weight: 600; }
    .date { color: #a0aec0; font-size: 0.78rem; }
    .empty-state { text-align: center; padding: 40px 20px; color: #a0aec0; }
    .checkmark { font-size: 2rem; display: block; color: #48bb78; margin-bottom: 8px; }
    .alert { padding: 12px 16px; border-radius: 8px; margin-bottom: 16px; font-size: 0.88rem; font-weight: 500; }
    .alert-error { background: #fff5f5; color: #c53030; border: 1px solid #feb2b2; }
    .alert-success { background: #f0fff4; color: #276749; border: 1px solid #9ae6b4; }
    .loading-state { text-align: center; padding: 60px; color: #718096; }
    .spinner { display: inline-block; width: 20px; height: 20px; border: 2px solid #e2e8f0; border-top-color: #667eea; border-radius: 50%; animation: spin .6s linear infinite; }
    .spinner-sm { display: inline-block; width: 14px; height: 14px; border: 2px solid rgba(255,255,255,.4); border-top-color: #fff; border-radius: 50%; animation: spin .6s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class InventoryAdjustmentsComponent implements OnInit {
  loading = true;
  submitting = false;
  error = '';
  success = '';
  products: any[] = [];
  pendingAdjustments: any[] = [];

  form = {
    productId: 0,
    amount: 0,
    reason: '',
    notes: ''
  };

  reasonCodes: AdjustmentReasonCode[] = [
    { code: 'CYCLE_COUNT', label: 'Cycle Count Correction' },
    { code: 'DAMAGE', label: 'Damaged / Write-Off' },
    { code: 'EXPIRY', label: 'Expired Goods' },
    { code: 'FOUND', label: 'Found Stock' },
    { code: 'RETURNS', label: 'Customer Returns' },
    { code: 'THEFT', label: 'Theft / Shrinkage' },
    { code: 'SYSTEM_ERROR', label: 'System Error Correction' },
    { code: 'OTHER', label: 'Other' }
  ];

  constructor(
    private inventoryService: InventoryService,
    private transactions: TransactionDataService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getBootstrap().subscribe({
      next: (data) => {
        this.products = data.products ?? [];
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load products.';
        this.loading = false;
      }
    });

    this.inventoryService.getPendingAdjustments().subscribe({
      next: (adj) => { this.pendingAdjustments = adj ?? []; },
      error: () => {}
    });
  }

  submitAdjustment(): void {
    if (!this.form.productId || !this.form.amount || !this.form.reason) return;
    this.submitting = true;
    this.error = '';
    this.success = '';
    this.inventoryService.requestAdjustment(
      +this.form.productId,
      +this.form.amount,
      this.form.reason + (this.form.notes ? ' | ' + this.form.notes : '')
    ).subscribe({
      next: () => {
        this.success = 'Adjustment request submitted and is pending approval.';
        this.form = { productId: 0, amount: 0, reason: '', notes: '' };
        this.submitting = false;
        this.load();
      },
      error: (err) => {
        this.error = err?.error ?? 'Failed to submit adjustment.';
        this.submitting = false;
      }
    });
  }

  approve(id: number): void {
    this.inventoryService.approveAdjustment(id).subscribe({
      next: () => {
        this.success = 'Adjustment approved and stock updated.';
        this.load();
      },
      error: (err) => { this.error = err?.error ?? 'Approval failed.'; }
    });
  }

  getProductName(productId: number): string {
    return this.products.find(p => p.id === productId)?.name ?? `Product #${productId}`;
  }
}
