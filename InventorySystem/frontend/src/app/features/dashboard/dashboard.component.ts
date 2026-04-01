import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardOverview, DashboardModule, InventoryService } from '../../core/services/inventory.service';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  overview: DashboardOverview = {
    kpis: {
      totalProducts: 0,
      activeWarehouses: 0,
      pendingAdjustments: 0,
      lowStockItems: 0,
      inventoryValueEstimate: 0
    },
    modules: [],
    integrations: [],
    nextMilestones: [],
    remainingGaps: [],
    recommendations: []
  };
  txn: TransactionBootstrap | null = null;
  accounting: any = null;
  loading = true;
  error = '';

  constructor(private inventoryService: InventoryService, private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';

    this.inventoryService.getDashboardOverview().subscribe({
      next: (overview) => {
        this.overview = {
          kpis: overview?.kpis ?? this.overview.kpis,
          modules: overview?.modules ?? [],
          integrations: overview?.integrations ?? [],
          nextMilestones: overview?.nextMilestones ?? [],
          remainingGaps: overview?.remainingGaps ?? [],
          recommendations: overview?.recommendations ?? []
        };
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

    this.transactions.getAccountingSummary().subscribe({
      next: (data) => this.accounting = data,
      error: () => {}
    });
  }

  get moduleHealth(): DashboardModule[] {
    return this.overview?.modules ?? [];
  }

  pendingSalesOrders(): number {
    return (this.txn?.salesOrders ?? []).filter(x => +x.status !== 3).length;
  }

  pendingPurchaseInvoices(): number {
    return (this.txn?.purchaseInvoices ?? []).filter(x => +(x.balanceAmount ?? 0) > 0).length;
  }

  pendingDispatches(): number {
    return this.pendingSalesOrders();
  }

  supplierPaymentsToday(): number {
    return +(this.accounting?.totalSupplierPayments ?? 0);
  }

  expiryAlerts: any[] = [];
  isOwner = true; // Toggle this for Salesman view demo

  toggleProfitMask(): void {
    this.isOwner = !this.isOwner;
  }

  recentSalesOrders(): any[] {
    return (this.txn?.salesOrders ?? []).slice(0, 5);
  }

  recentPurchaseOrders(): any[] {
    return (this.txn?.purchaseOrders ?? []).slice(0, 5);
  }

  lowStockRows(): any[] {
    return (this.overview?.nextMilestones ?? []).slice(0, 5);
  }

  moduleCompletion(): number {
    const total = this.moduleHealth.length;
    if (!total) return 0;
    const healthy = this.moduleHealth.filter(x => (x.status || '').toLowerCase() === 'active').length;
    return Math.round((healthy / total) * 100);
  }

  openBarcodeScanner(): void {
    const code = prompt('🔍 Scan Barcode or Enter SKU:');
    if (code) {
      alert(`Checking stock for: ${code}...`);
      // Future: Navigate to product detail or add to quick sale
    }
  }
}
