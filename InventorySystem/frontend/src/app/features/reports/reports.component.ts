import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { TransactionDataService } from '../../core/services/transaction-data.service';
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
}
