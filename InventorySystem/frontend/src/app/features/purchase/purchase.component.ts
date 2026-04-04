import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-purchase',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './purchase.component.html',
  styleUrls: ['./purchase.component.css']
})
export class PurchaseComponent implements OnInit {
  data: TransactionBootstrap | null = null;
  loading = true;
  error = '';
  validationMessage = '';
  successMessage = '';
  activeTab: 'po' | 'grn' | 'invoice' | 'payment' = 'po';

  purchaseOrders: any[] = [];
  poPage = 1;
  poTotal = 0;

  grns: any[] = [];
  grnPage = 1;
  grnTotal = 0;

  invoices: any[] = [];
  invoicePage = 1;
  invoiceTotal = 0;
  paymentPage = 1;
  readonly pageSize = 25;

  form = {
    editingOrderId: 0,
    vendorId: 0,
    warehouseId: 0,
    supplyState: '',
    current: { productId: 0, quantity: 1, unitPrice: 0, gstRate: 0 },
    items: [] as any[]
  };

  grn = { purchaseOrderId: 0, items: [] as any[] };
  invoice = { goodsReceiptNoteId: 0, dueDate: '' };
  payment = { purchaseInvoiceId: 0, amount: 0, paymentMode: 'BANK', referenceNo: '', notes: '' };

  constructor(private transactions: TransactionDataService, private toast: ToastService) {}

  private clearMessages(): void {
    this.error = '';
    this.validationMessage = '';
    this.successMessage = '';
  }

  private setError(err: any, fallback: string): void {
    if (typeof err?.error === 'string') {
      this.error = err.error;
      this.toast.error(this.error);
      return;
    }
    if (typeof err?.message === 'string') {
      this.error = err.message;
      this.toast.error(this.error);
      return;
    }
    this.error = fallback;
    this.toast.error(this.error);
  }

  private setValidation(message: string): void {
    this.validationMessage = message;
    this.toast.warning(message);
  }

  private setSuccess(message: string): void {
    this.successMessage = message;
    this.toast.success(message);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.validationMessage = '';
    this.transactions.getBootstrap().subscribe({
      next: (data) => {
        this.data = data;
        this.loadPagedData();
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load purchase module data.';
        this.toast.error(this.error);
        this.loading = false;
      }
    });
  }

  loadPagedData(): void {
    this.transactions.getPurchaseOrders(this.poPage, this.pageSize).subscribe({
      next: (res) => { this.purchaseOrders = res?.records ?? []; this.poTotal = +(res?.total ?? 0); },
      error: () => {}
    });
    this.transactions.getGoodsReceipts(this.grnPage, this.pageSize).subscribe({
      next: (res) => { this.grns = res?.records ?? []; this.grnTotal = +(res?.total ?? 0); },
      error: () => {}
    });
    this.transactions.getPurchaseInvoices(this.invoicePage, this.pageSize).subscribe({
      next: (res) => { this.invoices = res?.records ?? []; this.invoiceTotal = +(res?.total ?? 0); },
      error: () => {}
    });
  }

  selectPurchaseProduct(): void {
    const product = this.data?.products.find(x => x.id === +this.form.current.productId);
    if (!product) return;
    this.form.current.unitPrice = product.purchasePrice ?? 0;
    this.form.current.gstRate = product.gstRate ?? 0;
  }

  addLine(): void {
    this.clearMessages();
    if (!this.form.current.productId) {
      this.setValidation('Select a product before adding line.');
      return;
    }
    if (+this.form.current.quantity <= 0) {
      this.setValidation('Quantity must be greater than 0.');
      return;
    }
    if (+this.form.current.unitPrice < 0 || +this.form.current.gstRate < 0) {
      this.setValidation('Price and GST cannot be negative.');
      return;
    }
    const product = this.data?.products.find(x => x.id === +this.form.current.productId);
    if (!product) {
      this.setValidation('Selected product is invalid.');
      return;
    }
    this.form.items.push({
      productId: this.form.current.productId,
      productName: product?.name,
      quantity: this.form.current.quantity,
      unitPrice: this.form.current.unitPrice,
      gstRate: this.form.current.gstRate
    });
    this.form.current = { productId: 0, quantity: 1, unitPrice: 0, gstRate: 0 };
  }

  removeLine(index: number): void {
    this.form.items.splice(index, 1);
  }

  editPurchaseOrder(order: any): void {
    this.form.editingOrderId = +order.id;
    this.form.vendorId = +order.vendorId;
    this.form.warehouseId = +order.warehouseId;
    this.form.supplyState = order.supplyState ?? '';
    this.form.items = (order.items ?? []).map((x: any) => ({
      productId: x.productId,
      productName: this.data?.products.find(p => p.id === x.productId)?.name,
      quantity: x.quantity,
      unitPrice: x.unitPrice,
      gstRate: x.gstRate
    }));
    this.activeTab = 'po';
  }

  resetPoForm(): void {
    this.form = { editingOrderId: 0, vendorId: 0, warehouseId: 0, supplyState: '', current: { productId: 0, quantity: 1, unitPrice: 0, gstRate: 0 }, items: [] };
  }

  createPurchaseOrder(): void {
    this.clearMessages();
    if (!this.form.vendorId) {
      this.setValidation('Vendor is required.');
      return;
    }
    if (!this.form.warehouseId) {
      this.setValidation('Warehouse is required.');
      return;
    }
    if (this.form.items.length === 0) {
      this.setValidation('Add at least one line item.');
      return;
    }
    const payload = {
      vendorId: +this.form.vendorId,
      warehouseId: +this.form.warehouseId,
      supplyState: this.form.supplyState,
      items: this.form.items.map(x => ({ productId: +x.productId, quantity: +x.quantity, unitPrice: +x.unitPrice, gstRate: +x.gstRate }))
    };
    const save$ = this.form.editingOrderId
      ? this.transactions.updatePurchaseOrder(this.form.editingOrderId, payload)
      : this.transactions.createPurchaseOrder(payload);
    const wasEdit = !!this.form.editingOrderId;
    save$.subscribe({
      next: () => {
        this.resetPoForm();
        this.setSuccess(wasEdit ? 'Purchase order updated.' : 'Purchase order created.');
        this.loadPagedData();
      },
      error: (err) => this.setError(err, 'Purchase order save failed.')
    });
  }

  cancelPurchaseOrder(orderId: number): void {
    this.clearMessages();
    this.transactions.cancelPurchaseOrder(orderId).subscribe({
      next: () => {
        this.setSuccess('Purchase order cancelled successfully.');
        this.loadPagedData();
      },
      error: (err) => this.setError(err, 'Purchase order cancel failed.')
    });
  }

  prepareGrn(orderId: number): void {
    this.grn.purchaseOrderId = orderId;
    const order = this.purchaseOrders.find(x => x.id === orderId);
    this.grn.items = (order?.items ?? []).map((item: any) => ({
      purchaseOrderItemId: item.id,
      productId: item.productId,
      productName: this.data?.products.find(p => p.id === item.productId)?.name,
      binId: this.data?.bins[0]?.id ?? 0,
      quantityReceived: Math.max(0, (item.quantity ?? 0) - (item.receivedQuantity ?? 0))
    })).filter((x: any) => x.quantityReceived > 0);
    this.activeTab = 'grn';
  }

  submitGrn(): void {
    this.clearMessages();
    if (!this.grn.purchaseOrderId) {
      this.setValidation('Select a purchase order first.');
      return;
    }
    if (!this.grn.items.length) {
      this.setValidation('No GRN lines available for receipt.');
      return;
    }
    if (this.grn.items.some(x => +x.binId <= 0 || +x.quantityReceived <= 0)) {
      this.setValidation('Each GRN line needs valid bin and received quantity.');
      return;
    }
    const payload = {
      purchaseOrderId: this.grn.purchaseOrderId,
      items: this.grn.items.map(x => ({ purchaseOrderItemId: x.purchaseOrderItemId, productId: x.productId, binId: +x.binId, quantityReceived: +x.quantityReceived }))
    };
    this.transactions.createGoodsReceipt(payload).subscribe({
      next: () => {
        this.grn = { purchaseOrderId: 0, items: [] };
        this.setSuccess('GRN posted successfully.');
        this.loadPagedData();
      },
      error: (err) => this.setError(err, 'GRN creation failed.')
    });
  }

  canCreateInvoice(grnId: number): boolean {
    return !(this.invoices ?? []).some(x => x.goodsReceiptNoteId === grnId);
  }

  prepareInvoice(grnId: number): void {
    this.invoice.goodsReceiptNoteId = grnId;
    const due = new Date();
    due.setDate(due.getDate() + 30);
    this.invoice.dueDate = due.toISOString().slice(0, 10);
    this.activeTab = 'invoice';
  }

  createPurchaseInvoice(): void {
    this.clearMessages();
    if (!this.invoice.goodsReceiptNoteId) {
      this.setValidation('Select GRN before creating invoice.');
      return;
    }
    const payload = { goodsReceiptNoteId: +this.invoice.goodsReceiptNoteId, dueDate: this.invoice.dueDate || null };
    this.transactions.createPurchaseInvoice(payload).subscribe({
      next: () => {
        this.invoice = { goodsReceiptNoteId: 0, dueDate: '' };
        this.setSuccess('Purchase invoice created.');
        this.loadPagedData();
      },
      error: (err) => this.setError(err, 'Purchase invoice creation failed.')
    });
  }

  hasPendingBalance(invoice: any): boolean {
    return +(invoice?.balanceAmount ?? 0) > 0;
  }

  preparePayment(invoice: any): void {
    this.payment.purchaseInvoiceId = invoice.id;
    this.payment.amount = +(invoice.balanceAmount ?? 0);
    this.payment.paymentMode = 'BANK';
    this.payment.referenceNo = '';
    this.payment.notes = '';
    this.activeTab = 'payment';
  }

  createSupplierPayment(): void {
    this.clearMessages();
    if (!this.payment.purchaseInvoiceId) {
      this.setValidation('Select invoice before payment.');
      return;
    }
    if (this.payment.amount <= 0) {
      this.setValidation('Payment amount must be greater than 0.');
      return;
    }
    const payload = { purchaseInvoiceId: +this.payment.purchaseInvoiceId, amount: +this.payment.amount, paymentMode: this.payment.paymentMode, referenceNo: this.payment.referenceNo, notes: this.payment.notes };
    this.transactions.createSupplierPayment(payload).subscribe({
      next: () => {
        this.payment = { purchaseInvoiceId: 0, amount: 0, paymentMode: 'BANK', referenceNo: '', notes: '' };
        this.setSuccess('Supplier payment recorded.');
        this.loadPagedData();
      },
      error: (err) => this.setError(err, 'Supplier payment failed.')
    });
  }

  vendorName(vendorId: number): string {
    return this.data?.vendors.find((x) => +x.id === +vendorId)?.name ?? `Vendor #${vendorId}`;
  }

  warehouseName(warehouseId: number): string {
    return this.data?.warehouses.find((x) => +x.id === +warehouseId)?.name ?? `Warehouse #${warehouseId}`;
  }

  exportPurchaseOrdersCsv(): void {
    const headers = ['PO Number', 'Vendor', 'Warehouse', 'Status', 'Grand Total'];
    const rows = this.purchaseOrders.map((po) => [
      po.orderNumber,
      this.vendorName(po.vendorId),
      this.warehouseName(po.warehouseId),
      po.status === 1 ? 'OPEN' : po.status === 2 ? 'LOCKED' : 'CLOSED',
      (+po.grandTotal || 0).toFixed(2)
    ]);
    const csv = [headers, ...rows]
      .map((line) => line.map((value) => `"${String(value ?? '').replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `purchase-orders-page-${this.poPage}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  exportPurchaseOrdersPdf(): void {
    const rows = this.purchaseOrders
      .map((po) => `
        <tr>
          <td>${po.orderNumber ?? ''}</td>
          <td>${this.vendorName(po.vendorId)}</td>
          <td>${this.warehouseName(po.warehouseId)}</td>
          <td>${po.status === 1 ? 'OPEN' : po.status === 2 ? 'LOCKED' : 'CLOSED'}</td>
          <td>${(+po.grandTotal || 0).toFixed(2)}</td>
        </tr>
      `)
      .join('');
    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write(`
      <html><head><title>Purchase Orders</title>
      <style>body{font-family:Arial,sans-serif;padding:16px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #ddd;padding:8px;text-align:left}th{background:#f5f5f5}</style>
      </head><body>
      <h2>Purchase Orders (Page ${this.poPage})</h2>
      <p>Total Records: ${this.poTotal}</p>
      <table><thead><tr><th>PO</th><th>Vendor</th><th>Warehouse</th><th>Status</th><th>Total</th></tr></thead><tbody>${rows}</tbody></table>
      </body></html>
    `);
    win.document.close();
    win.focus();
    win.print();
  }

  nextPoPage(): void { if ((this.poPage * this.pageSize) >= this.poTotal) return; this.poPage += 1; this.loadPagedData(); }
  prevPoPage(): void { if (this.poPage <= 1) return; this.poPage -= 1; this.loadPagedData(); }
  nextGrnPage(): void { if ((this.grnPage * this.pageSize) >= this.grnTotal) return; this.grnPage += 1; this.loadPagedData(); }
  prevGrnPage(): void { if (this.grnPage <= 1) return; this.grnPage -= 1; this.loadPagedData(); }
  nextInvoicePage(): void { if ((this.invoicePage * this.pageSize) >= this.invoiceTotal) return; this.invoicePage += 1; this.loadPagedData(); }
  prevInvoicePage(): void { if (this.invoicePage <= 1) return; this.invoicePage -= 1; this.loadPagedData(); }
  nextPaymentPage(): void {
    const total = this.data?.supplierPayments?.length ?? 0;
    if ((this.paymentPage * this.pageSize) >= total) return;
    this.paymentPage += 1;
  }
  prevPaymentPage(): void { if (this.paymentPage <= 1) return; this.paymentPage -= 1; }
  pagedRows(rows: any[] | null | undefined, page: number): any[] {
    const list = rows ?? [];
    const start = (page - 1) * this.pageSize;
    return list.slice(start, start + this.pageSize);
  }
}
