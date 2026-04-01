import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-warehouse',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.css']
})
export class WarehouseComponent implements OnInit {
  data: TransactionBootstrap | null = null;
  advanced: any = null;
  loading = true;
  error = '';
  validationMessage = '';
  successMessage = '';
  activeTab: 'transfers' | 'queue' | 'picking' | 'stock' = 'transfers';
  readonly pageSize = 25;
  openOrdersPage = 1;
  queuePage = 1;
  pickPage = 1;
  stockPage = 1;

  transferForm = {
    productId: 0,
    fromBinId: 0,
    toBinId: 0,
    quantity: 1,
    referenceNo: ''
  };

  transferRequestForm = {
    productId: 0,
    fromBinId: 0,
    toBinId: 0,
    quantity: 1,
    requestedBy: 'warehouse.staff@axonix.local'
  };

  pickForm = {
    salesOrderId: 0,
    pickListId: 0,
    barcode: '',
    quantity: 1
  };

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getBootstrap().subscribe({
      next: (data) => {
        this.data = data;
        this.loadAdvanced();
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load warehouse data.';
        this.loading = false;
      }
    });
  }

  loadAdvanced(): void {
    this.transactions.getAdvancedSnapshot().subscribe({
      next: (data) => this.advanced = data,
      error: () => {}
    });
  }

  createTransfer(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.transferForm.productId || !this.transferForm.fromBinId || !this.transferForm.toBinId || this.transferForm.quantity <= 0) {
      this.validationMessage = 'Product, source bin, destination bin and quantity are required.';
      return;
    }
    this.transactions.createStockTransfer({
      productId: +this.transferForm.productId,
      fromBinId: +this.transferForm.fromBinId,
      toBinId: +this.transferForm.toBinId,
      quantity: +this.transferForm.quantity,
      referenceNo: this.transferForm.referenceNo
    }).subscribe({
      next: () => {
        this.transferForm = { productId: 0, fromBinId: 0, toBinId: 0, quantity: 1, referenceNo: '' };
        this.successMessage = 'Stock transfer posted.';
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Stock transfer failed.'
    });
  }

  dispatchOrder(orderId: number): void {
    if (!confirm('Create invoice and mark this order dispatched?')) return;
    this.transactions.createSalesInvoice({ salesOrderId: orderId }).subscribe({
      next: () => {
        this.successMessage = 'Order dispatched and invoice generated.';
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Dispatch failed during invoice creation.'
    });
  }

  openSalesOrders(): any[] {
    return (this.data?.salesOrders ?? []).filter(x => +x.status !== 3);
  }

  requestTransfer(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.transferRequestForm.productId || !this.transferRequestForm.fromBinId || !this.transferRequestForm.toBinId || this.transferRequestForm.quantity <= 0) {
      this.validationMessage = 'Transfer request needs product, bins and quantity.';
      return;
    }
    this.transactions.createTransferRequest({
      productId: +this.transferRequestForm.productId,
      fromBinId: +this.transferRequestForm.fromBinId,
      toBinId: +this.transferRequestForm.toBinId,
      quantity: +this.transferRequestForm.quantity,
      requestedBy: this.transferRequestForm.requestedBy
    }).subscribe({
      next: () => {
        this.transferRequestForm = { productId: 0, fromBinId: 0, toBinId: 0, quantity: 1, requestedBy: 'warehouse.staff@axonix.local' };
        this.successMessage = 'Transfer request raised.';
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Transfer request failed.'
    });
  }

  approveTransfer(requestId: number, approve: boolean): void {
    if (!confirm(approve ? 'Approve this transfer request?' : 'Reject this transfer request?')) return;
    this.transactions.approveTransferRequest({
      requestId,
      approve,
      approvedBy: 'inventory.manager@axonix.local',
      approvalNote: approve ? 'Approved from queue' : 'Rejected from queue'
    }).subscribe({
      next: () => {
        this.successMessage = approve ? 'Transfer request approved.' : 'Transfer request rejected.';
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Transfer approval failed.'
    });
  }

  createPickList(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.pickForm.salesOrderId) {
      this.validationMessage = 'Select sales order before creating pick list.';
      return;
    }
    this.transactions.createPickList({ salesOrderId: +this.pickForm.salesOrderId }).subscribe({
      next: () => {
        this.pickForm.salesOrderId = 0;
        this.successMessage = 'Pick list created.';
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Pick list creation failed.'
    });
  }

  scanPickItem(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.pickForm.pickListId || !this.pickForm.barcode) {
      this.validationMessage = 'Pick list and barcode are required for scan.';
      return;
    }
    this.transactions.scanPick({
      pickListId: +this.pickForm.pickListId,
      barcode: this.pickForm.barcode,
      quantity: +this.pickForm.quantity
    }).subscribe({
      next: () => {
        this.pickForm.barcode = '';
        this.pickForm.quantity = 1;
        this.successMessage = 'Pick scan captured.';
        this.loadAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Pick scan failed.'
    });
  }

  packPickList(pickListId: number): void {
    if (!confirm('Mark this pick list as packed?')) return;
    this.transactions.packPickList(pickListId).subscribe({
      next: () => {
        this.successMessage = 'Pick list marked as packed.';
        this.loadAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Pack operation failed.'
    });
  }

  pagedRows(rows: any[] | null | undefined, page: number): any[] {
    const list = rows ?? [];
    const start = (page - 1) * this.pageSize;
    return list.slice(start, start + this.pageSize);
  }

  prevOpenOrdersPage(): void { if (this.openOrdersPage > 1) this.openOrdersPage -= 1; }
  nextOpenOrdersPage(): void {
    const total = this.openSalesOrders().length;
    if ((this.openOrdersPage * this.pageSize) < total) this.openOrdersPage += 1;
  }

  prevQueuePage(): void { if (this.queuePage > 1) this.queuePage -= 1; }
  nextQueuePage(): void {
    const total = (this.advanced?.transferRequests ?? []).length;
    if ((this.queuePage * this.pageSize) < total) this.queuePage += 1;
  }

  prevPickPage(): void { if (this.pickPage > 1) this.pickPage -= 1; }
  nextPickPage(): void {
    const total = (this.advanced?.pickLists ?? []).length;
    if ((this.pickPage * this.pageSize) < total) this.pickPage += 1;
  }

  prevStockPage(): void { if (this.stockPage > 1) this.stockPage -= 1; }
  nextStockPage(): void {
    const total = (this.data?.stockLevels ?? []).length;
    if ((this.stockPage * this.pageSize) < total) this.stockPage += 1;
  }
}
