import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, NgChartsModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']
})
export class ReportsComponent implements OnInit {
  loading = true;
  error = '';
  report: any = null;
  ai: any = null;
  reorder: any = null;
  profitByItem: Array<{ name: string; amount: number }> = [];
  profitByCustomer: Array<{ name: string; amount: number }> = [];
  deadStock: Array<{ name: string; available: number }> = [];

  // Chart configuration
  public lineChartData?: ChartData<'line'>;
  public barChartData?: ChartData<'bar'>;
  public lineChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: true } }
  };
  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y', // horizontal bar
    plugins: { legend: { display: false } }
  };

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getReportsSnapshot().subscribe({
      next: (data) => {
        this.report = data;
        this.setupCharts(data);
        
        this.transactions.getDemandPrediction(30).subscribe({
          next: (ai) => {
            this.ai = ai;
            this.transactions.getAutoReorderSuggestions(30, 15).subscribe({
              next: (reorder) => {
                this.reorder = reorder;
                this.loading = false;
              },
              error: () => this.loading = false
            });
          },
          error: () => this.loading = false
        });
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load reports.';
        this.loading = false;
      }
    });

    this.transactions.getBootstrap().subscribe({
      next: (data) => this.buildDecisionReports(data),
      error: () => {}
    });
  }

  setupCharts(data: any): void {
    // Line Chart: Trend
    if (data.salesTrend && data.purchaseTrend) {
      this.lineChartData = {
        labels: data.salesTrend.map((x: any) => x.date),
        datasets: [
          {
            data: data.salesTrend.map((x: any) => x.total),
            label: 'Sales',
            backgroundColor: 'rgba(54, 162, 235, 0.2)',
            borderColor: 'rgba(54, 162, 235, 1)',
            fill: 'origin',
          },
          {
            data: data.purchaseTrend.map((x: any) => x.total),
            label: 'Purchase',
            backgroundColor: 'rgba(255, 99, 132, 0.2)',
            borderColor: 'rgba(255, 99, 132, 1)',
            fill: 'origin',
          }
        ]
      };
    }

    // Bar Chart: Top Products
    if (data.fastMoving) {
      this.barChartData = {
        labels: data.fastMoving.map((x: any) => x.productName),
        datasets: [{
          data: data.fastMoving.map((x: any) => x.qty),
          label: 'Quantity Sold',
          backgroundColor: '#4e73df',
          hoverBackgroundColor: '#2e59d9',
          borderColor: '#4e73df',
        }]
      };
    }
  }

  exportInventory(): void {
    this.transactions.downloadInventoryExcel();
  }

  exportTallyXml(): void {
    // defaults to last month as per service
    this.transactions.downloadTallySalesXml();
  }

  private buildDecisionReports(data: TransactionBootstrap): void {
    const products = data?.products ?? [];
    const salesOrders = data?.salesOrders ?? [];
    const salesInvoices = data?.salesInvoices ?? [];
    const stockLevels = data?.stockLevels ?? [];
    const customers = data?.customers ?? [];

    const itemTotals = new Map<number, number>();
    salesOrders.forEach((order: any) => {
      (order.items ?? []).forEach((line: any) => {
        const productId = +(line.productId ?? 0);
        if (!productId) return;
        const qty = +(line.quantity ?? 0);
        const rate = +(line.unitPrice ?? line.rate ?? 0);
        const estCost = rate * 0.75;
        const profit = Math.max(0, (rate - estCost) * qty);
        itemTotals.set(productId, (itemTotals.get(productId) ?? 0) + profit);
      });
    });

    this.profitByItem = Array.from(itemTotals.entries())
      .map(([productId, amount]) => ({
        name: products.find((p: any) => p.id === productId)?.name ?? `Product #${productId}`,
        amount
      }))
      .sort((a, b) => b.amount - a.amount)
      .slice(0, 8);

    const customerTotals = new Map<number, number>();
    salesInvoices.forEach((invoice: any) => {
      const customerId = +(invoice.customerId ?? invoice.partyId ?? 0);
      const net = +(invoice.grandTotal ?? 0);
      const estProfit = net * 0.2;
      if (!customerId || estProfit <= 0) return;
      customerTotals.set(customerId, (customerTotals.get(customerId) ?? 0) + estProfit);
    });

    this.profitByCustomer = Array.from(customerTotals.entries())
      .map(([customerId, amount]) => ({
        name: customers.find((c: any) => c.id === customerId)?.name ?? `Customer #${customerId}`,
        amount
      }))
      .sort((a, b) => b.amount - a.amount)
      .slice(0, 8);

    const soldProductIds = new Set<number>();
    salesOrders.forEach((order: any) => {
      (order.items ?? []).forEach((line: any) => soldProductIds.add(+(line.productId ?? 0)));
    });

    const availableByProduct = new Map<number, number>();
    stockLevels.forEach((level: any) => {
      const productId = +(level.productId ?? 0);
      if (!productId) return;
      const available = +(level.availableQuantity ?? 0);
      availableByProduct.set(productId, (availableByProduct.get(productId) ?? 0) + available);
    });

    this.deadStock = products
      .filter((p: any) => !soldProductIds.has(+p.id) && (availableByProduct.get(+p.id) ?? 0) > 0)
      .map((p: any) => ({
        name: p.name,
        available: availableByProduct.get(+p.id) ?? 0
      }))
      .sort((a, b) => b.available - a.available)
      .slice(0, 10);
  }
}
