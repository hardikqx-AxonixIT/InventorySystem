import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TransactionLookup {
  id: number;
  name?: string;
  code?: string;
  state?: string;
  warehouseName?: string;
  binCode?: string;
  purchasePrice?: number;
  salesPrice?: number;
  gstRate?: number;
}

export interface TransactionBootstrap {
  products: any[];
  customers: any[];
  vendors: any[];
  warehouses: any[];
  bins: any[];
  stockLevels: any[];
  purchaseOrders: any[];
  goodsReceiptNotes: any[];
  purchaseInvoices: any[];
  supplierPayments: any[];
  salesOrders: any[];
  salesInvoices: any[];
  gstSummary: {
    purchaseCgst: number;
    purchaseSgst: number;
    purchaseIgst: number;
    salesCgst: number;
    salesSgst: number;
    salesIgst: number;
    netGstPayable: number;
  };
  accountingSummary: any;
}

@Injectable({
  providedIn: 'root'
})
export class TransactionDataService {
  private readonly apiUrl = `${environment.apiUrl}/Transactions`;
  private readonly advancedUrl = `${environment.apiUrl}/AdvancedOperations`;

  constructor(private http: HttpClient) {}

  getBootstrap(): Observable<TransactionBootstrap> {
    return this.http.get<TransactionBootstrap>(`${this.apiUrl}/bootstrap`);
  }

  getPurchaseOrders(page = 1, pageSize = 25): Observable<any> {
    return this.http.get(`${this.apiUrl}/purchase-orders?page=${page}&pageSize=${pageSize}`);
  }

  getGoodsReceipts(page = 1, pageSize = 25): Observable<any> {
    return this.http.get(`${this.apiUrl}/goods-receipts?page=${page}&pageSize=${pageSize}`);
  }

  getPurchaseInvoices(page = 1, pageSize = 25): Observable<any> {
    return this.http.get(`${this.apiUrl}/purchase-invoices?page=${page}&pageSize=${pageSize}`);
  }

  getSalesOrders(page = 1, pageSize = 25): Observable<any> {
    return this.http.get(`${this.apiUrl}/sales-orders?page=${page}&pageSize=${pageSize}`);
  }

  getSalesInvoices(page = 1, pageSize = 25): Observable<any> {
    return this.http.get(`${this.apiUrl}/sales-invoices?page=${page}&pageSize=${pageSize}`);
  }

  createPurchaseOrder(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/purchase-orders`, payload);
  }

  updatePurchaseOrder(purchaseOrderId: number, payload: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/purchase-orders/${purchaseOrderId}`, payload);
  }

  cancelPurchaseOrder(purchaseOrderId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/purchase-orders/${purchaseOrderId}/cancel`, {});
  }

  createGoodsReceipt(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/goods-receipts`, payload);
  }

  createPurchaseInvoice(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/purchase-invoices`, payload);
  }

  createSupplierPayment(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/supplier-payments`, payload);
  }

  createStockIn(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/inventory/stock-in`, payload);
  }

  createStockOut(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/inventory/stock-out`, payload);
  }

  createStockTransfer(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/inventory/stock-transfer`, payload);
  }

  getAccountingSummary(): Observable<any> {
    return this.http.get(`${this.apiUrl}/accounting-summary`);
  }

  createProductionRun(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/manufacturing/production-runs`, payload);
  }

  getReportsSnapshot(): Observable<any> {
    return this.http.get(`${this.apiUrl}/reports/snapshot`);
  }

  getMobileDashboard(): Observable<any> {
    return this.http.get(`${this.apiUrl}/mobile/dashboard`);
  }

  getDemandPrediction(lookbackDays = 30): Observable<any> {
    return this.http.get(`${this.apiUrl}/ai/demand-prediction?lookbackDays=${lookbackDays}`);
  }

  getAutoReorderSuggestions(lookbackDays = 30, horizonDays = 15): Observable<any> {
    return this.http.get(`${this.apiUrl}/auto-reorder/suggestions?lookbackDays=${lookbackDays}&horizonDays=${horizonDays}`);
  }

  createSalesOrder(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/sales-orders`, payload);
  }

  updateSalesOrder(salesOrderId: number, payload: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/sales-orders/${salesOrderId}`, payload);
  }

  cancelSalesOrder(salesOrderId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/sales-orders/${salesOrderId}/cancel`, {});
  }

  createSalesInvoice(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/sales-invoices`, payload);
  }

  getAdvancedSnapshot(): Observable<any> {
    return this.http.get(`${this.advancedUrl}/snapshot`);
  }

  getGstr1(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/gst/gstr1`, payload);
  }

  getGstr3b(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/gst/gstr3b`, payload);
  }

  generateEInvoice(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/gst/einvoice`, payload);
  }

  generateEWayBill(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/gst/eway-bill`, payload);
  }

  getAccountingLedgers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.advancedUrl}/accounting/ledgers`);
  }

  postJournalVoucher(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/accounting/journal-vouchers`, payload);
  }

  getLedgerDrilldown(ledgerId: number, payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/accounting/ledger/${ledgerId}`, payload);
  }

  getProfitLoss(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/accounting/profit-loss`, payload);
  }

  getBalanceSheet(asOfDate?: string): Observable<any> {
    return this.http.get(`${this.advancedUrl}/accounting/balance-sheet${asOfDate ? `?asOfDate=${encodeURIComponent(asOfDate)}` : ''}`);
  }

  createQuotation(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/sales/quotations`, payload);
  }

  convertQuotation(quotationId: number): Observable<any> {
    return this.http.post(`${this.advancedUrl}/sales/quotations/${quotationId}/convert`, {});
  }

  updateQuotation(quotationId: number, payload: any): Observable<any> {
    return this.http.put(`${this.advancedUrl}/sales/quotations/${quotationId}`, payload);
  }

  cancelQuotation(quotationId: number): Observable<any> {
    return this.http.post(`${this.advancedUrl}/sales/quotations/${quotationId}/cancel`, {});
  }

  createDeliveryChallan(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/sales/delivery-challans`, payload);
  }

  createSalesReturn(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/sales/returns`, payload);
  }

  createTransferRequest(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/warehouse/transfer-requests`, payload);
  }

  approveTransferRequest(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/warehouse/transfer-requests/approve`, payload);
  }

  createPickList(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/warehouse/pick-lists`, payload);
  }

  scanPick(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/warehouse/pick-lists/scan`, payload);
  }

  packPickList(pickListId: number): Observable<any> {
    return this.http.post(`${this.advancedUrl}/warehouse/pick-lists/${pickListId}/pack`, {});
  }

  createBomTemplate(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/manufacturing/boms`, payload);
  }

  createWorkOrder(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/manufacturing/work-orders`, payload);
  }

  updateBomTemplate(bomTemplateId: number, payload: any): Observable<any> {
    return this.http.put(`${this.advancedUrl}/manufacturing/boms/${bomTemplateId}`, payload);
  }

  releaseWorkOrder(workOrderId: number): Observable<any> {
    return this.http.post(`${this.advancedUrl}/manufacturing/work-orders/${workOrderId}/release`, {});
  }

  cancelWorkOrder(workOrderId: number): Observable<any> {
    return this.http.post(`${this.advancedUrl}/manufacturing/work-orders/${workOrderId}/cancel`, {});
  }

  getPermissions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.advancedUrl}/users/permissions`);
  }

  savePermission(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/users/permissions`, payload);
  }

  getAuditLogs(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/users/audit-logs`, payload);
  }

  sendWhatsAppInvoice(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/integrations/whatsapp/invoice`, payload);
  }

  createRazorpayOrder(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/integrations/razorpay/orders`, payload);
  }

  verifyRazorpayCallback(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/integrations/razorpay/callback`, payload);
  }

  getStockHistory(productId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/stock-history/${productId}`);
  }

  downloadInventoryExcel(): void {
    window.open(`${environment.apiUrl}/Export/inventory/excel`, '_blank');
  }

  downloadSalesInvoicePdf(invoiceId: number): void {
    window.open(`${environment.apiUrl}/Export/sales-invoice/${invoiceId}/pdf`, '_blank');
  }

  downloadTallySalesXml(fromDate?: string, toDate?: string): void {
    let url = `${environment.apiUrl}/Export/tally-sales/xml`;
    if (fromDate && toDate) {
      url += `?fromDate=${encodeURIComponent(fromDate)}&toDate=${encodeURIComponent(toDate)}`;
    }
    window.open(url, '_blank');
  }

  scanBatchToOrder(orderId: number, batchNumber: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/sales/orders/${orderId}/scan/${batchNumber}`, {});
  }
}
