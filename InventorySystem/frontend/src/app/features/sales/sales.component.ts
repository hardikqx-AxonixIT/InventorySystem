import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

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

  constructor(private transactions: TransactionDataService) {}

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
    this.validationMessage = '';
    if (!this.form.current.productId || !this.form.current.binId || !this.form.current.quantity) {
      this.validationMessage = 'Select product, bin and quantity before adding line.';
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
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.form.customerId || !this.form.warehouseId || this.form.items.length === 0) {
      this.validationMessage = 'Customer, warehouse and at least one line item are required.';
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
    save$.subscribe({
      next: () => {
        this.resetSalesOrderForm();
        this.successMessage = this.form.editingOrderId ? 'Sales order updated.' : 'Sales order created.';
        this.loadPagedData();
        this.refreshAdvanced();
      },
      error: (err) => this.error = err?.error ?? (this.form.editingOrderId ? 'Sales order update failed.' : 'Sales order creation failed.')
    });
  }

  rapidScan(serial: string): void {
    if (!serial || !this.form.editingOrderId) {
      alert('Open a Sales Order first to use Rapid Scan.');
      return;
    }
    this.transactions.scanBatchToOrder(this.form.editingOrderId, serial).subscribe({
      next: (res) => {
        this.successMessage = `Item ${res.scannedProduct} added!`;
        this.loadPagedData();
      },
      error: (err) => alert(err.error || 'Serial/IMEI not found.')
    });
  }

  createInvoice(orderId: number): void {
    this.successMessage = '';
    this.transactions.createSalesInvoice({ salesOrderId: orderId }).subscribe({
      next: () => {
        this.successMessage = 'Sales invoice created.';
        this.loadPagedData();
        this.refreshAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Invoice creation failed.'
    });
  }

  createQuotation(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.quotationForm.customerId || !this.quotationForm.warehouseId || this.quotationForm.items.length === 0) {
      this.validationMessage = 'Customer, warehouse and quotation lines are required.';
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
    save$.subscribe({
      next: () => {
        this.resetQuotationForm();
        this.successMessage = this.quotationForm.editingQuotationId ? 'Quotation updated.' : 'Quotation created.';
        this.refreshAdvanced();
      },
      error: (err) => this.error = err?.error ?? (this.quotationForm.editingQuotationId ? 'Quotation update failed.' : 'Quotation creation failed.')
    });
  }

  convertQuotation(quotationId: number): void {
    if (!confirm('Convert this quotation to sales order?')) return;
    this.successMessage = '';
    this.transactions.convertQuotation(quotationId).subscribe({
      next: () => {
        this.successMessage = 'Quotation converted to sales order.';
        this.loadPagedData();
        this.refreshAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Quotation conversion failed.'
    });
  }

  cancelQuotation(quotationId: number): void {
    if (!confirm('Soft delete this quotation?')) return;
    this.successMessage = '';
    this.transactions.cancelQuotation(quotationId).subscribe({
      next: () => {
        this.successMessage = 'Quotation soft deleted.';
        this.refreshAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Quotation cancel failed.'
    });
  }

  cancelSalesOrder(orderId: number): void {
    if (!confirm('Soft delete this sales order?')) return;
    this.successMessage = '';
    this.transactions.cancelSalesOrder(orderId).subscribe({
      next: () => {
        this.successMessage = 'Sales order soft deleted.';
        this.loadPagedData();
      },
      error: (err) => this.error = err?.error ?? 'Sales order cancel failed.'
    });
  }

  createDeliveryChallan(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.challanForm.salesOrderId) {
      this.validationMessage = 'Select a sales order for delivery challan.';
      return;
    }
    this.transactions.createDeliveryChallan(this.challanForm).subscribe({
      next: () => {
        this.challanForm = { salesOrderId: 0, notes: '' };
        this.successMessage = 'Delivery challan created.';
        this.refreshAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Delivery challan creation failed.'
    });
  }

  loadReturnItems(): void {
    const invoice = [...(this.salesInvoices ?? []), ...(this.data?.salesInvoices ?? [])].find(x => x.id === +this.returnForm.salesInvoiceId);
    this.returnForm.items = (invoice?.items ?? []).map((x: any) => ({
      salesInvoiceItemId: x.id, binId: x.binId, quantity: x.quantity, productId: x.productId
    }));
  }

  createSalesReturn(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.returnForm.salesInvoiceId || this.returnForm.items.length === 0) {
      this.validationMessage = 'Select invoice and return lines to post return.';
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
        this.successMessage = 'Sales return posted.';
        this.refreshAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Sales return failed.'
    });
  }

  sendWhatsAppInvoice(): void {
    this.validationMessage = '';
    if (!this.whatsappForm.salesInvoiceId || !this.whatsappForm.phoneNumber) {
      this.validationMessage = 'Invoice and phone number are required for WhatsApp.';
      return;
    }
    this.transactions.sendWhatsAppInvoice(this.whatsappForm).subscribe({
      next: (res) => this.integrationMessage = res?.note ?? (res?.success ? 'WhatsApp API request sent.' : 'WhatsApp not configured. Preview link generated.'),
      error: (err) => this.error = err?.error ?? 'WhatsApp send failed.'
    });
  }

  createRazorpayOrder(): void {
    this.validationMessage = '';
    if (!this.razorpayForm.amount) {
      this.validationMessage = 'Amount is required for Razorpay order.';
      return;
    }
    this.transactions.createRazorpayOrder(this.razorpayForm).subscribe({
      next: (res) => this.integrationMessage = res?.success ? 'Razorpay order created.' : (res?.note ?? 'Razorpay not configured.'),
      error: (err) => this.error = err?.error ?? 'Razorpay order failed.'
    });
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
          this.successMessage = 'WhatsApp message sent via API.';
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
