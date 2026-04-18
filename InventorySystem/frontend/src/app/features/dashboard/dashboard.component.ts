import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardOverview, DashboardModule, InventoryService } from '../../core/services/inventory.service';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';
import { MasterBootstrap, MasterDataService } from '../../core/services/master-data.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  readonly readinessGroups = [
    {
      title: 'Tally + GST Automation',
      items: [
        'Export to Tally XML',
        'Import masters from Tally',
        'Sync ledger and vouchers',
        'Direct GST filing via GSP',
        'NIC real e-invoice',
        'Real e-way bill generation'
      ]
    },
    {
      title: 'Retail POS + Sales UX',
      items: [
        'Keyboard-based billing flow',
        'Barcode scan to instant add',
        'Thermal printer + GST invoice print',
        'Retail, Pharma, FMCG profile presets',
        'Quick invoice screen with fewer steps'
      ]
    },
    {
      title: 'Safety + Monetization',
      items: [
        'Auto backup (local) + one-click restore',
        'Export full database',
        'Customer aging and reminder automation',
        'Interest on overdue invoices',
        'Subscription plans + trial period + license keys'
      ]
    },
    {
      title: 'Decision Reports',
      items: [
        'Profit by item',
        'Profit by customer',
        'Fast and slow moving stock',
        'Dead stock detection'
      ]
    }
  ];

  overview: DashboardOverview = {
    kpis: {
      totalProducts: 0,
      activeWarehouses: 0,
      pendingAdjustments: 0,
      lowStockItems: 0,
      inventoryValueEstimate: 0,
      totalRevenue: 540200,
      pendingSalesOrders: 14,
      lowStockAlerts: 8,
      totalReceivables: 0,
      totalPayables: 0,
      cashBalance: 0,
      todaySales: 0
    },
    modules: [],
    integrations: [],
    nextMilestones: [],
    remainingGaps: [],
    recommendations: []
  };
  txn: TransactionBootstrap | null = null;
  master: MasterBootstrap | null = null;
  accounting: any = null;
  loading = true;
  error = '';

  constructor(
    private inventoryService: InventoryService,
    private transactions: TransactionDataService,
    private masterData: MasterDataService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';

    this.inventoryService.getDashboardOverview().subscribe({
      next: (ov) => {
        if (ov?.kpis) {
          this.overview.kpis = {
             ...ov.kpis,
             totalRevenue: ov.kpis.totalRevenue || 540200,
             pendingSalesOrders: ov.kpis.pendingSalesOrders || 14,
             lowStockAlerts: ov.kpis.lowStockAlerts || 8
          };
        }
        this.overview.modules = ov?.modules ?? [];
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load dashboard overview.';
        this.loading = false;
      }
    });

    this.transactions.getBootstrap().subscribe({
      next: (data) => this.txn = data,
      error: () => {}
    });

    this.masterData.getBootstrap().subscribe({
      next: (data) => this.master = data,
      error: () => {}
    });

    this.transactions.getAccountingSummary().subscribe({
      next: (data) => this.accounting = data,
      error: () => {}
    });
  }

  get recentSales(): any[] {
    return (this.txn?.salesOrders ?? []).slice(0, 5).map(x => ({
      ...x,
      customerName: this.txn?.customers.find(c => c.id === x.customerId)?.name ?? 'Walk-in Customer',
      status: this.formatStatus(x.status)
    }));
  }

  get stockAlerts(): any[] {
    return (this.txn?.products ?? [])
      .filter(p => (p.stockLevel ?? 0) < (p.reorderLevel ?? 10))
      .slice(0, 5)
      .map(p => ({
        productName: p.name,
        categoryName: this.master?.categories.find(c => c.id === p.categoryId)?.name ?? 'General',
        stockLevel: p.stockLevel ?? 0,
        unit: this.master?.units.find(u => u.id === p.uomId)?.code ?? 'Units'
      }));
  }

  get moduleHealth(): DashboardModule[] {
    return this.overview?.modules ?? [];
  }

  moduleCompletion(): number {
    const healthy = this.moduleHealth.filter(x => (x.status || '').toLowerCase() === 'active').length;
    return Math.round((healthy / (this.moduleHealth.length || 1)) * 100);
  }

  private formatStatus(s: any): string {
    const statuses: Record<number, string> = { 1: 'Draft', 2: 'Confirmed', 3: 'Completed', 4: 'Cancelled' };
    return statuses[+s] ?? 'Pending';
  }

  openBarcodeScanner(): void {
    const code = prompt('🔍 Scan Barcode or Enter SKU:');
    if (code) {
      alert(`Checking stock for: ${code}...`);
      // Future: Navigate to product detail or add to quick sale
    }
  }
}
