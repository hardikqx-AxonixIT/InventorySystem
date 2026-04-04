import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sales.component.html',
  styleUrls: ['./sales.component.css']
})
export class SalesComponent implements OnInit {
  data: TransactionBootstrap | null = null;
  advanced: any = null;
  loading = true;
  error = '';
  validationMessage = '';
  successMessage = '';
  integrationMessage = '';
  activeTab: 'orders' | 'invoices' | 'quotations' | 'challans' | 'returns' | 'integrations' = 'orders';
  readonly pageSize = 25;

  salesOrders: any[] = [];
  ordersPage = 1;
  ordersTotal = 0;

  salesInvoices: any[] = [];
  invoicesPage = 1;
  invoicesTotal = 0;

  quotationPage = 1;
  challanPage = 1;
  returnPage = 1;

  form = {
    editingOrderId: 0,
    customerId: 0,
    warehouseId: 0,
    placeOfSupplyState: '',
    current: { productId: 0, binId: 0, quantity: 1, unitPrice: 0, gstRate: 0 },
    items: [] as any[]
  };

  quotationForm = {
    editingQuotationId: 0,
    customerId: 0,
    warehouseId: 0,
    placeOfSupplyState: '',
    validUntil: '',
    current: { productId: 0, binId: 0, quantity: 1, unitPrice: 0, gstRate: 0 },
    items: [] as any[]
  };

  challanForm = {
    salesOrderId: 0,
    notes: ''
  };

  returnForm = {
    salesInvoiceId: 0,
    reason: '',
    items: [] as any[]
  };

  whatsappForm = {
    salesInvoiceId: 0,
    phoneNumber: ''
  };

  razorpayForm = {
    amount: 0,
    currency: 'INR',
    receipt: ''
  };

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
    this.transactions.getBootstrap().subscribe({
      next: (data) => {
        this.data = data;
        this.loadPagedData();
        this.refreshAdvanced();
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load sales module data.';
        this.toast.error(this.error);
        this.loading = false;
      }
    });
  }

  refreshAdvanced(): void {
    this.transactions.getAdvancedSnapshot().subscribe({
      next: (data) => this.advanced = data,
      error: () => {}
    });
  }

  loadPagedData(): void {
    this.transactions.getSalesOrders(this.ordersPage, this.pageSize).subscribe({
      next: (res) => {
        this.salesOrders = res?.records ?? [];
        this.ordersTotal = +(res?.total ?? 0);
      },
      error: () => {}
    });
    this.transactions.getSalesInvoices(this.invoicesPage, this.pageSize).subscribe({
      next: (res) => {
        this.salesInvoices = res?.records ?? [];
        this.invoicesTotal = +(res?.total ?? 0);
      },
      error: () => {}
    });
  }

  selectSalesProduct(): void {
    const product = this.data?.products.find(x => x.id === +this.form.current.productId);
    if (!product) return;
    this.form.current.unitPrice = product.salesPrice ?? 0;
    this.form.current.gstRate = product.gstRate ?? 0;
  }

  addLine(): void {
    this.clearMessages();
    if (!this.form.current.productId || !this.form.current.binId || !this.form.current.quantity) {
      this.setValidation('Select product, bin and quantity before adding line.');
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
    const bin = this.data?.bins.find(x => x.id === +this.form.current.binId);
    this.form.items.push({
      productId: this.form.current.productId,
      productName: product?.name,
      binId: this.form.current.binId,
      binCode: bin?.binCode,
      quantity: this.form.current.quantity,
      unitPrice: this.form.current.unitPrice,
      gstRate: this.form.current.gstRate
    });
    this.form.current = { productId: 0, binId: 0, quantity: 1, unitPrice: 0, gstRate: 0 };
  }

  editSalesOrder(order: any): void {
    this.form.editingOrderId = +order.id;
    this.form.customerId = +order.customerId;
    this.form.warehouseId = +order.warehouseId;
    this.form.placeOfSupplyState = order.placeOfSupplyState ?? '';
    this.form.items = (order.items ?? []).map((x: any) => ({
      productId: +x.productId,
      productName: this.data?.products.find(p => p.id === x.productId)?.name,
      binId: +x.binId,
      binCode: this.data?.bins.find(b => b.id === x.binId)?.binCode,
      quantity: +x.quantity,
      unitPrice: +x.unitPrice,
      gstRate: +x.gstRate
    }));
    this.activeTab = 'orders';
  }

  resetSalesOrderForm(): void {
    this.form = { editingOrderId: 0, customerId: 0, warehouseId: 0, placeOfSupplyState: '', current: { productId: 0, binId: 0, quantity: 1, unitPrice: 0, gstRate: 0 }, items: [] };
  }

  editQuotation(quote: any): void {
    this.quotationForm.editingQuotationId = +quote.id;
    this.quotationForm.customerId = +quote.customerId;
    this.quotationForm.warehouseId = +quote.warehouseId;
    this.quotationForm.validUntil = quote.validUntil ? new Date(quote.validUntil).toISOString().slice(0, 10) : '';
    this.quotationForm.items = (quote.items ?? []).map((x: any) => ({
      productId: +x.productId,
      productName: this.data?.products.find(p => p.id === x.productId)?.name,
      binId: +x.binId,
      binCode: this.data?.bins.find(b => b.id === x.binId)?.binCode,
      quantity: +x.quantity,
      unitPrice: +x.unitPrice,
      gstRate: +x.gstRate
    }));
  }

  resetQuotationForm(): void {
    this.quotationForm = { editingQuotationId: 0, customerId: 0, warehouseId: 0, placeOfSupplyState: '', validUntil: '', current: { productId: 0, binId: 0, quantity: 1, unitPrice: 0, gstRate: 0 }, items: [] };
  }

  addQuoteLine(): void {
    this.validationMessage = '';
    if (!this.quotationForm.current.productId || !this.quotationForm.current.binId || !this.quotationForm.current.quantity) {
      this.validationMessage = 'Select product, bin and quantity before adding quotation line.';
      return;
    }
    const product = this.data?.products.find(x => x.id === +this.quotationForm.current.productId);
    const bin = this.data?.bins.find(x => x.id === +this.quotationForm.current.binId);
    this.quotationForm.items.push({
      productId: this.quotationForm.current.productId,
      productName: product?.name,
      binId: this.quotationForm.current.binId,
      binCode: bin?.binCode,
      quantity: this.quotationForm.current.quantity,
      unitPrice: this.quotationForm.current.unitPrice,
      gstRate: this.quotationForm.current.gstRate
    });
    this.quotationForm.current = { productId: 0, binId: 0, quantity: 1, unitPrice: 0, gstRate: 0 };
  }

  createSalesOrder(): void {
    this.clearMessages();
    if (!this.form.customerId || !this.form.warehouseId || this.form.items.length === 0) {
      this.setValidation('Customer, warehouse and at least one line item are required.');
      return;
    }
    const payload = {
      customerId: +this.form.customerId,
      warehouseId: +this.form.warehouseId,
      placeOfSupplyState: this.form.placeOfSupplyState,
      items: this.form.items.map(x => ({
        productId: +x.productId,
        binId: +x.binId,
        quantity: +x.quantity,
        unitPrice: +x.unitPrice,
        gstRate: +x.gstRate
      }))
    };
    const save$ = this.form.editingOrderId
      ? this.transactions.updateSalesOrder(this.form.editingOrderId, payload)
      : this.transactions.createSalesOrder(payload);
    const wasEdit = !!this.form.editingOrderId;
    save$.subscribe({
      next: () => {
        this.resetSalesOrderForm();
        this.setSuccess(wasEdit ? 'Sales order updated successfully.' : 'Sales order saved successfully.');
        this.loadPagedData();
        this.refreshAdvanced();
      },
      error: (err) => this.setError(err, wasEdit ? 'Sales order update failed.' : 'Sales order save failed.')
    });
  }

  rapidScan(serial: string): void {
    if (!serial || !this.form.editingOrderId) {
      alert('Open a Sales Order first to use Rapid Scan.');
      return;
    }
    this.transactions.scanBatchToOrder(this.form.editingOrderId, serial).subscribe({
      next: (res) => {
        this.setSuccess(`Item ${res.scannedProduct} added!`);
        this.loadPagedData();
      },
      error: (err) => alert(err.error || 'Serial/IMEI not found.')
    });
  }

  createInvoice(orderId: number): void {
    this.clearMessages();
    this.transactions.createSalesInvoice({ salesOrderId: orderId }).subscribe({
      next: () => {
        this.setSuccess('Sales invoice created.');
        this.loadPagedData();
        this.refreshAdvanced();
      },
      error: (err) => this.setError(err, 'Invoice creation failed.')
    });
  }

  createQuotation(): void {
    this.clearMessages();
    if (!this.quotationForm.customerId || !this.quotationForm.warehouseId || this.quotationForm.items.length === 0) {
      this.setValidation('Customer, warehouse and quotation lines are required.');
      return;
    }
    const payload = {
      customerId: +this.quotationForm.customerId,
      warehouseId: +this.quotationForm.warehouseId,
      placeOfSupplyState: this.quotationForm.placeOfSupplyState,
      validUntil: this.quotationForm.validUntil || null,
      items: this.quotationForm.items.map(x => ({
        productId: +x.productId, binId: +x.binId, quantity: +x.quantity, unitPrice: +x.unitPrice, gstRate: +x.gstRate
      }))
    };
    const save$ = this.quotationForm.editingQuotationId
      ? this.transactions.updateQuotation(this.quotationForm.editingQuotationId, payload)
      : this.transactions.createQuotation(payload);
    const wasEdit = !!this.quotationForm.editingQuotationId;
    save$.subscribe({
      next: () => {
        this.resetQuotationForm();
        this.setSuccess(wasEdit ? 'Quotation updated successfully.' : 'Quotation saved successfully.');
        this.refreshAdvanced();
      },
      error: (err) => this.setError(err, wasEdit ? 'Quotation update failed.' : 'Quotation save failed.')
    });
  }

  convertQuotation(quotationId: number): void {
    if (!confirm('Convert this quotation to sales order?')) return;
    this.clearMessages();
    this.transactions.convertQuotation(quotationId).subscribe({
      next: () => {
        this.setSuccess('Quotation converted to sales order.');
        this.loadPagedData();
        this.refreshAdvanced();
      },
      error: (err) => this.setError(err, 'Quotation conversion failed.')
    });
  }

  cancelQuotation(quotationId: number): void {
    if (!confirm('Soft delete this quotation?')) return;
    this.clearMessages();
    this.transactions.cancelQuotation(quotationId).subscribe({
      next: () => {
        this.setSuccess('Quotation soft deleted.');
        this.refreshAdvanced();
      },
      error: (err) => this.setError(err, 'Quotation cancel failed.')
    });
  }

  cancelSalesOrder(orderId: number): void {
    if (!confirm('Soft delete this sales order?')) return;
    this.clearMessages();
    this.transactions.cancelSalesOrder(orderId).subscribe({
      next: () => {
        this.setSuccess('Sales order cancelled successfully.');
        this.loadPagedData();
      },
      error: (err) => this.setError(err, 'Sales order cancel failed.')
    });
  }

  createDeliveryChallan(): void {
    this.clearMessages();
    if (!this.challanForm.salesOrderId) {
      this.setValidation('Select a sales order for delivery challan.');
      return;
    }
    this.transactions.createDeliveryChallan(this.challanForm).subscribe({
      next: () => {
        this.challanForm = { salesOrderId: 0, notes: '' };
        this.setSuccess('Delivery challan created.');
        this.refreshAdvanced();
      },
      error: (err) => this.setError(err, 'Delivery challan creation failed.')
    });
  }

  loadReturnItems(): void {
    const invoice = [...(this.salesInvoices ?? []), ...(this.data?.salesInvoices ?? [])].find(x => x.id === +this.returnForm.salesInvoiceId);
    this.returnForm.items = (invoice?.items ?? []).map((x: any) => ({
      salesInvoiceItemId: x.id, binId: x.binId, quantity: x.quantity, productId: x.productId
    }));
  }

  createSalesReturn(): void {
    this.clearMessages();
    if (!this.returnForm.salesInvoiceId || this.returnForm.items.length === 0) {
      this.setValidation('Select invoice and return lines to post return.');
      return;
    }
    const payload = {
      salesInvoiceId: +this.returnForm.salesInvoiceId,
      reason: this.returnForm.reason,
      items: this.returnForm.items.map((x: any) => ({ salesInvoiceItemId: +x.salesInvoiceItemId, binId: +x.binId, quantity: +x.quantity }))
    };
    this.transactions.createSalesReturn(payload).subscribe({
      next: () => {
        this.returnForm = { salesInvoiceId: 0, reason: '', items: [] };
        this.setSuccess('Sales return posted.');
        this.refreshAdvanced();
      },
      error: (err) => this.setError(err, 'Sales return failed.')
    });
  }

  sendWhatsAppInvoice(): void {
    this.clearMessages();
    if (!this.whatsappForm.salesInvoiceId || !this.whatsappForm.phoneNumber) {
      this.setValidation('Invoice and phone number are required for WhatsApp.');
      return;
    }
    this.transactions.sendWhatsAppInvoice(this.whatsappForm).subscribe({
      next: (res) => this.integrationMessage = res?.note ?? (res?.success ? 'WhatsApp API request sent.' : 'WhatsApp not configured. Preview link generated.'),
      error: (err) => this.setError(err, 'WhatsApp send failed.')
    });
  }

  createRazorpayOrder(): void {
    this.clearMessages();
    if (!this.razorpayForm.amount) {
      this.setValidation('Amount is required for Razorpay order.');
      return;
    }
    this.transactions.createRazorpayOrder(this.razorpayForm).subscribe({
      next: (res) => this.integrationMessage = res?.success ? 'Razorpay order created.' : (res?.note ?? 'Razorpay not configured.'),
      error: (err) => this.setError(err, 'Razorpay order failed.')
    });
  }

  customerName(customerId: number): string {
    return this.data?.customers.find((x) => +x.id === +customerId)?.name ?? `Customer #${customerId}`;
  }

  warehouseName(warehouseId: number): string {
    return this.data?.warehouses.find((x) => +x.id === +warehouseId)?.name ?? `Warehouse #${warehouseId}`;
  }

  exportSalesOrdersCsv(): void {
    const headers = ['Order Number', 'Customer', 'Warehouse', 'Status', 'Grand Total'];
    const rows = this.salesOrders.map((order) => [
      order.orderNumber,
      this.customerName(order.customerId),
      this.warehouseName(order.warehouseId),
      order.status === 1 ? 'OPEN' : order.status === 2 ? 'LOCKED' : 'COMPLETED',
      (+order.grandTotal || 0).toFixed(2)
    ]);
    const csv = [headers, ...rows]
      .map((line) => line.map((value) => `"${String(value ?? '').replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `sales-orders-page-${this.ordersPage}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  exportSalesOrdersPdf(): void {
    const rows = this.salesOrders
      .map((order) => `
        <tr>
          <td>${order.orderNumber ?? ''}</td>
          <td>${this.customerName(order.customerId)}</td>
          <td>${this.warehouseName(order.warehouseId)}</td>
          <td>${order.status === 1 ? 'OPEN' : order.status === 2 ? 'LOCKED' : 'COMPLETED'}</td>
          <td>${(+order.grandTotal || 0).toFixed(2)}</td>
        </tr>
      `)
      .join('');
    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write(`
      <html><head><title>Sales Orders</title>
      <style>body{font-family:Arial,sans-serif;padding:16px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #ddd;padding:8px;text-align:left}th{background:#f5f5f5}</style>
      </head><body>
      <h2>Sales Orders (Page ${this.ordersPage})</h2>
      <p>Total Records: ${this.ordersTotal}</p>
      <table><thead><tr><th>Order</th><th>Customer</th><th>Warehouse</th><th>Status</th><th>Total</th></tr></thead><tbody>${rows}</tbody></table>
      </body></html>
    `);
    win.document.close();
    win.focus();
    win.print();
  }

  canCreateInvoice(order: any): boolean {
    return +order?.status !== 3;
  }

  downloadInvoice(invoice: any): void {
    this.transactions.downloadSalesInvoicePdf(invoice.id);
  }

  shareOnWhatsApp(invoice: any): void {
    const customer = this.data?.customers.find(x => x.id === invoice.customerId);
    const phone = customer?.phone || customer?.mobile || '';
    if (!phone) {
      alert('No phone number found for this customer. Please update customer master.');
      return;
    }
    const cleanPhone = phone.replace(/\D/g, '');
    const mobileWithCountry = cleanPhone.length === 10 ? '91' + cleanPhone : cleanPhone;
    
    this.transactions.sendWhatsAppInvoice({ salesInvoiceId: invoice.id, phoneNumber: mobileWithCountry }).subscribe({
      next: (res) => {
        if (res.previewUrl) {
          window.open(res.previewUrl, '_blank');
        } else {
          this.setSuccess('WhatsApp message sent via API.');
        }
      },
      error: () => alert('WhatsApp integration failed.')
    });
  }

  pagedRows(rows: any[] | null | undefined, page: number): any[] {
    const list = rows ?? [];
    const start = (page - 1) * this.pageSize;
    return list.slice(start, start + this.pageSize);
  }

  totalPages(total: number): number {
    return Math.max(1, Math.ceil((total || 0) / this.pageSize));
  }

  prevOrdersPage(): void {
    if (this.ordersPage <= 1) return;
    this.ordersPage -= 1;
    this.loadPagedData();
  }

  nextOrdersPage(): void {
    if ((this.ordersPage * this.pageSize) >= this.ordersTotal) return;
    this.ordersPage += 1;
    this.loadPagedData();
  }

  prevInvoicesPage(): void {
    if (this.invoicesPage <= 1) return;
    this.invoicesPage -= 1;
    this.loadPagedData();
  }

  nextInvoicesPage(): void {
    if ((this.invoicesPage * this.pageSize) >= this.invoicesTotal) return;
    this.invoicesPage += 1;
    this.loadPagedData();
  }

  prevQuotationPage(): void {
    if (this.quotationPage <= 1) return;
    this.quotationPage -= 1;
  }

  nextQuotationPage(): void {
    const total = (this.advanced?.quotations ?? []).length;
    if ((this.quotationPage * this.pageSize) >= total) return;
    this.quotationPage += 1;
  }

  prevChallanPage(): void {
    if (this.challanPage <= 1) return;
    this.challanPage -= 1;
  }

  nextChallanPage(): void {
    const total = (this.advanced?.deliveryChallans ?? []).length;
    if ((this.challanPage * this.pageSize) >= total) return;
    this.challanPage += 1;
  }

  prevReturnPage(): void {
    if (this.returnPage <= 1) return;
    this.returnPage -= 1;
  }

  nextReturnPage(): void {
    const total = (this.advanced?.salesReturns ?? []).length;
    if ((this.returnPage * this.pageSize) >= total) return;
    this.returnPage += 1;
  }
}
