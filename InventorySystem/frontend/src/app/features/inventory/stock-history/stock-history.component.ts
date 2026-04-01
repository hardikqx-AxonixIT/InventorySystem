import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TransactionDataService } from '../../../core/services/transaction-data.service';

@Component({
  selector: 'app-stock-history',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './stock-history.component.html',
  styleUrls: ['./stock-history.component.css']
})
export class StockHistoryComponent implements OnInit {
  productId: number = 0;
  productName: string = '';
  history: any[] = [];
  loading: boolean = true;
  error: string = '';

  constructor(
    private route: ActivatedRoute,
    private transactions: TransactionDataService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.productId = +params['id'];
      if (this.productId) {
        this.loadHistory();
      }
    });
  }

  loadHistory(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getStockHistory(this.productId).subscribe({
      next: (data) => {
        this.productName = data.productName;
        this.history = data.history;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load stock history.';
        this.loading = false;
      }
    });
  }

  getReasonLabel(code: string): string {
    const map: { [key: string]: string } = {
      'PURCHASE_GRN': 'Purchase Receipt',
      'STOCK_IN': 'Manual In',
      'STOCK_OUT': 'Manual Out',
      'STOCK_TRANSFER_IN': 'Transfer In',
      'STOCK_TRANSFER_OUT': 'Transfer Out',
      'MFG_CONSUME': 'Manufacturing Consumption',
      'MFG_OUTPUT': 'Manufacturing Output',
      'ADJUSTMENT': 'Stock Adjustment'
    };
    return map[code] || code;
  }
}
