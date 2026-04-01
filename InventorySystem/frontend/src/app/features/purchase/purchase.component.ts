import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

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

  constructor(private transactions: TransactionDataService) {}

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
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.form.current.productId) {
      this.validationMessage = 'Select a product before adding line.';
      return;
    }
    if (+this.form.current.quantity <= 0) {
      this.validationMessage = 'Quantity must be greater than 0.';
      return;
    }
    const product = this.data?.products.find(x => x.id === +this.form.current.productId);
    if (!product) {
      this.validationMessage = 'Selected product is invalid.';
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
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.form.vendorId) {
      this.validationMessage = 'Vendor is required.';
      return;
    }
    if (!this.form.warehouseId) {
      this.validationMessage = 'Warehouse is required.';
      return;
    }
    if (this.form.items.length === 0) {
      this.validationMessage = 'Add at least one line item.';
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
        this.successMessage = wasEdit ? 'Purchase order updated.' : 'Purchase order created.';
        this.loadPagedData();
      },
      error: (err) => this.error = err?.error ?? 'Purchase order save failed.'
    });
  }

  cancelPurchaseOrder(orderId: number): void {
    this.transactions.cancelPurchaseOrder(orderId).subscribe({
      next: () => this.loadPagedData(),
      error: (err) => this.error = err?.error ?? 'Purchase order cancel failed.'
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
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.grn.purchaseOrderId) {
      this.validationMessage = 'Select a purchase order first.';
      return;
    }
    if (!this.grn.items.length) {
      this.validationMessage = 'No GRN lines available for receipt.';
      return;
    }
    if (this.grn.items.some(x => +x.binId <= 0 || +x.quantityReceived <= 0)) {
      this.validationMessage = 'Each GRN line needs valid bin and received quantity.';
      return;
    }
    const payload = {
      purchaseOrderId: this.grn.purchaseOrderId,
      items: this.grn.items.map(x => ({ purchaseOrderItemId: x.purchaseOrderItemId, productId: x.productId, binId: +x.binId, quantityReceived: +x.quantityReceived }))
    };
    this.transactions.createGoodsReceipt(payload).subscribe({
      next: () => {
        this.grn = { purchaseOrderId: 0, items: [] };
        this.successMessage = 'GRN posted successfully.';
        this.loadPagedData();
      },
      error: (err) => this.error = err?.error ?? 'GRN creation failed.'
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
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.invoice.goodsReceiptNoteId) {
      this.validationMessage = 'Select GRN before creating invoice.';
      return;
    }
    const payload = { goodsReceiptNoteId: +this.invoice.goodsReceiptNoteId, dueDate: this.invoice.dueDate || null };
    this.transactions.createPurchaseInvoice(payload).subscribe({
      next: () => {
        this.invoice = { goodsReceiptNoteId: 0, dueDate: '' };
        this.successMessage = 'Purchase invoice created.';
        this.loadPagedData();
      },
      error: (err) => this.error = err?.error ?? 'Purchase invoice creation failed.'
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
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.payment.purchaseInvoiceId) {
      this.validationMessage = 'Select invoice before payment.';
      return;
    }
    if (this.payment.amount <= 0) {
      this.validationMessage = 'Payment amount must be greater than 0.';
      return;
    }
    const payload = { purchaseInvoiceId: +this.payment.purchaseInvoiceId, amount: +this.payment.amount, paymentMode: this.payment.paymentMode, referenceNo: this.payment.referenceNo, notes: this.payment.notes };
    this.transactions.createSupplierPayment(payload).subscribe({
      next: () => {
        this.payment = { purchaseInvoiceId: 0, amount: 0, paymentMode: 'BANK', referenceNo: '', notes: '' };
        this.successMessage = 'Supplier payment recorded.';
        this.loadPagedData();
      },
      error: (err) => this.error = err?.error ?? 'Supplier payment failed.'
    });
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
