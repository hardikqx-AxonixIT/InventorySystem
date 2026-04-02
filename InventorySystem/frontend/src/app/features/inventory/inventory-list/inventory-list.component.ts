import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TransactionBootstrap, TransactionDataService } from '../../../core/services/transaction-data.service';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './inventory-list.component.html',
  styleUrls: ['./inventory-list.component.css']
})
export class InventoryListComponent implements OnInit {
  data: TransactionBootstrap | null = null;
  loading = true;
  error = '';
  activeTab: 'operations' | 'stock' | 'products' = 'operations';
  readonly pageSize = 25;
  stockPage = 1;
  productsPage = 1;

  stockInForm = {
    productId: 0,
    binId: 0,
    quantity: 1,
    referenceNo: ''
  };

  stockOutForm = {
    productId: 0,
    binId: 0,
    quantity: 1,
    referenceNo: ''
  };

  transferForm = {
    productId: 0,
    fromBinId: 0,
    toBinId: 0,
    quantity: 1,
    referenceNo: ''
  };

  constructor(private transactions: TransactionDataService) {}

  ngOnInit() {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getBootstrap().subscribe({
      next: (data) => {
        this.data = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load inventory records.';
        this.loading = false;
      }
    });
  }

  createStockIn(): void {
    if (!this.stockInForm.productId || !this.stockInForm.binId || this.stockInForm.quantity <= 0) return;
    this.transactions.createStockIn({
      productId: +this.stockInForm.productId,
      binId: +this.stockInForm.binId,
      quantity: +this.stockInForm.quantity,
      referenceNo: this.stockInForm.referenceNo
    }).subscribe({
      next: () => {
        this.stockInForm = { productId: 0, binId: 0, quantity: 1, referenceNo: '' };
        this.load();
      },
      error: (err) => {
        this.error = err?.error ?? 'Stock-in failed.';
      }
    });
  }

  createStockOut(): void {
    if (!this.stockOutForm.productId || !this.stockOutForm.binId || this.stockOutForm.quantity <= 0) return;
    this.transactions.createStockOut({
      productId: +this.stockOutForm.productId,
      binId: +this.stockOutForm.binId,
      quantity: +this.stockOutForm.quantity,
      referenceNo: this.stockOutForm.referenceNo
    }).subscribe({
      next: () => {
        this.stockOutForm = { productId: 0, binId: 0, quantity: 1, referenceNo: '' };
        this.load();
      },
      error: (err) => {
        this.error = err?.error ?? 'Stock-out failed.';
      }
    });
  }

  createTransfer(): void {
    if (!this.transferForm.productId || !this.transferForm.fromBinId || !this.transferForm.toBinId || this.transferForm.quantity <= 0) return;
    this.transactions.createStockTransfer({
      productId: +this.transferForm.productId,
      fromBinId: +this.transferForm.fromBinId,
      toBinId: +this.transferForm.toBinId,
      quantity: +this.transferForm.quantity,
      referenceNo: this.transferForm.referenceNo
    }).subscribe({
      next: () => {
        this.transferForm = { productId: 0, fromBinId: 0, toBinId: 0, quantity: 1, referenceNo: '' };
        this.load();
      },
      error: (err) => {
        this.error = err?.error ?? 'Stock transfer failed.';
      }
    });
  }

  pagedRows(rows: any[] | null | undefined, page: number): any[] {
    const list = rows ?? [];
    const start = (page - 1) * this.pageSize;
    return list.slice(start, start + this.pageSize);
  }

  prevStockPage(): void {
    if (this.stockPage <= 1) return;
    this.stockPage -= 1;
  }

  nextStockPage(): void {
    const total = (this.data?.stockLevels ?? []).length;
    if ((this.stockPage * this.pageSize) >= total) return;
    this.stockPage += 1;
  }

  prevProductsPage(): void {
    if (this.productsPage <= 1) return;
    this.productsPage -= 1;
  }

  nextProductsPage(): void {
    const total = (this.data?.products ?? []).length;
    if ((this.productsPage * this.pageSize) >= total) return;
    this.productsPage += 1;
  }
}
