import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ERP_MODULES, ErpModuleDefinition, getModuleDefinition } from '../../core/erp-modules';
import { map } from 'rxjs/operators';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-module-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './module-page.component.html',
  styleUrls: ['./module-page.component.css']
})
export class ModulePageComponent implements OnInit {
  readonly relatedModules = ERP_MODULES;
  data: TransactionBootstrap | null = null;
  accounting: any = null;
  error = '';
  moduleKey = '';

  readonly module$ = this.route.data.pipe(
    map(data => getModuleDefinition(data['moduleKey']) ?? ERP_MODULES[0])
  );

  constructor(private route: ActivatedRoute, private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.route.data.subscribe(data => this.moduleKey = data['moduleKey'] ?? '');

    this.transactions.getBootstrap().subscribe({
      next: (result) => this.data = result,
      error: () => this.error = 'Unable to load module live data.'
    });

    this.transactions.getAccountingSummary().subscribe({
      next: (result) => this.accounting = result,
      error: () => {}
    });
  }

  trackByLabel(_: number, item: { label: string }): string {
    return item.label;
  }

  statusClass(module: ErpModuleDefinition): string {
    return module.status.toLowerCase().replace(/\s+/g, '-');
  }

  liveMetrics(moduleKey: string): { label: string; value: string }[] {
    if (!this.data) return [];

    if (moduleKey === 'reports') {
      return [
        { label: 'Low stock items', value: `${this.data.products.filter(p => (p.reorderLevel ?? 0) > 0 && ((this.data?.stockLevels ?? []).filter(s => s.productId === p.id).reduce((sum, x) => sum + (x.availableQuantity ?? 0), 0) <= p.reorderLevel)).length}` },
        { label: 'Purchase invoices', value: `${this.data.purchaseInvoices.length}` },
        { label: 'Sales invoices', value: `${this.data.salesInvoices.length}` }
      ];
    }

    if (moduleKey === 'users') {
      return [
        { label: 'Roles available', value: 'SuperAdmin, InventoryManager, WarehouseManager, Accountant, Staff' },
        { label: 'Audit-ready modules', value: 'Masters, Inventory, Purchase, Sales' },
        { label: 'Current mode', value: 'JWT auth enabled' }
      ];
    }

    if (moduleKey === 'mobile') {
      return [
        { label: 'Open sales orders', value: `${(this.data.salesOrders ?? []).filter(x => +x.status !== 3).length}` },
        { label: 'Pending supplier invoices', value: `${(this.data.purchaseInvoices ?? []).filter(x => +(x.balanceAmount ?? 0) > 0).length}` },
        { label: 'Net GST', value: `${this.data.gstSummary.netGstPayable}` }
      ];
    }

    return [
      { label: 'Products', value: `${this.data.products.length}` },
      { label: 'Warehouses', value: `${this.data.warehouses.length}` },
      { label: 'Stock bins', value: `${this.data.bins.length}` }
    ];
  }

  liveRecords(moduleKey: string): { label: string; value: string }[] {
    if (!this.data) return [];

    if (moduleKey === 'manufacturing') {
      return this.data.products.slice(0, 5).map(item => ({
        label: `BOM template for ${item.sku}`,
        value: `${item.name} | UOM ${item.uomId} | Reorder ${item.reorderLevel}`
      }));
    }

    if (moduleKey === 'reports') {
      return this.data.stockLevels.slice(0, 8).map(item => ({
        label: `${item.productName ?? item.productId} @ ${item.binCode ?? item.binId}`,
        value: `OH ${item.quantityOnHand} | Res ${item.reservedQuantity} | Avl ${item.availableQuantity}`
      }));
    }

    if (moduleKey === 'mobile') {
      return [
        { label: 'Today sales', value: `${this.accounting?.totalSales ?? 0}` },
        { label: 'Outstanding payable', value: `${this.accounting?.totalPayables ?? 0}` },
        { label: 'Supplier payments', value: `${this.accounting?.totalSupplierPayments ?? 0}` }
      ];
    }

    return this.data.salesOrders.slice(0, 6).map(item => ({
      label: item.orderNumber,
      value: `Customer #${item.customerId} | Total ${item.grandTotal}`
    }));
  }
}
