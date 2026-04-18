import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-accounting',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './accounting.component.html',
  styleUrls: ['./accounting.component.css']
})
export class AccountingComponent implements OnInit {
  loading = true;
  error = '';
  summary: any = null;
  ledgers: any[] = [];
  activeTab: 'overview' | 'voucher' | 'drilldown' | 'statements' | 'collections' = 'overview';
  readonly pageSize = 25;
  ledgerPage = 1;
  collectionMessage = '';
  customerAging: Array<{
    customerId: number;
    customerName: string;
    outstanding: number;
    bucket: string;
    oldestDays: number;
    invoiceId: number;
  }> = [];
  voucherForm = {
    narration: '',
    lines: [
      { ledgerId: 0, debit: 0, credit: 0, remarks: '' },
      { ledgerId: 0, debit: 0, credit: 0, remarks: '' }
    ]
  };
  selectedLedgerId = 0;
  ledgerView: any = null;
  profitLoss: any = null;
  balanceSheet: any = null;

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getAccountingSummary().subscribe({
      next: (data) => {
        this.summary = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load accounting summary.';
        this.loading = false;
      }
    });
    this.transactions.getAccountingLedgers().subscribe({
      next: (rows) => {
        this.ledgers = rows ?? [];
        if (!this.selectedLedgerId && this.ledgers.length) this.selectedLedgerId = this.ledgers[0].id;
      },
      error: () => {}
    });
    this.transactions.getBootstrap().subscribe({
      next: (data) => this.customerAging = this.buildCustomerAging(data),
      error: () => {}
    });
    this.transactions.getProfitLoss({}).subscribe({ next: (data) => this.profitLoss = data, error: () => {} });
    this.transactions.getBalanceSheet().subscribe({ next: (data) => this.balanceSheet = data, error: () => {} });
  }

  addLine(): void {
    this.voucherForm.lines.push({ ledgerId: 0, debit: 0, credit: 0, remarks: '' });
  }

  postVoucher(): void {
    const lines = this.voucherForm.lines
      .filter(x => +x.ledgerId && (+x.debit > 0 || +x.credit > 0))
      .map(x => ({ ledgerId: +x.ledgerId, debit: +x.debit, credit: +x.credit, remarks: x.remarks }));
    if (lines.length < 2) return;
    this.transactions.postJournalVoucher({ narration: this.voucherForm.narration, lines }).subscribe({
      next: () => {
        this.voucherForm = { narration: '', lines: [{ ledgerId: 0, debit: 0, credit: 0, remarks: '' }, { ledgerId: 0, debit: 0, credit: 0, remarks: '' }] };
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Voucher posting failed.'
    });
  }

  loadLedger(): void {
    if (!this.selectedLedgerId) return;
    this.transactions.getLedgerDrilldown(this.selectedLedgerId, {}).subscribe({
      next: (data) => this.ledgerView = data,
      error: (err) => this.error = err?.error ?? 'Unable to load ledger drill-down.'
    });
  }

  pagedRows(rows: any[] | null | undefined, page: number): any[] {
    const list = rows ?? [];
    const start = (page - 1) * this.pageSize;
    return list.slice(start, start + this.pageSize);
  }

  prevLedgerPage(): void {
    if (this.ledgerPage <= 1) return;
    this.ledgerPage -= 1;
  }

  nextLedgerPage(): void {
    const total = (this.summary?.ledger ?? []).length;
    if ((this.ledgerPage * this.pageSize) >= total) return;
    this.ledgerPage += 1;
  }

  sendReminder(row: { invoiceId: number; customerName: string }): void {
    if (!row?.invoiceId) {
      this.collectionMessage = 'No invoice available for reminder.';
      return;
    }

    this.transactions.sendWhatsAppInvoice({
      salesInvoiceId: row.invoiceId,
      phoneNumber: ''
    }).subscribe({
      next: () => this.collectionMessage = `Reminder request triggered for ${row.customerName}.`,
      error: () => this.collectionMessage = `Reminder flow is ready, but WhatsApp provider credentials are required.`
    });
  }

  calculateInterest(row: { outstanding: number; oldestDays: number }): number {
    const overdueDays = Math.max(0, (row?.oldestDays ?? 0) - 30);
    const annualRate = 18;
    return +(row.outstanding * (annualRate / 100) * (overdueDays / 365)).toFixed(2);
  }

  private buildCustomerAging(data: any): Array<{
    customerId: number;
    customerName: string;
    outstanding: number;
    bucket: string;
    oldestDays: number;
    invoiceId: number;
  }> {
    const customers = data?.customers ?? [];
    const invoices = data?.salesInvoices ?? [];
    const now = new Date();
    const rows = invoices
      .map((invoice: any) => {
        const balance = +(invoice.balanceAmount ?? invoice.pendingAmount ?? invoice.grandTotal ?? 0);
        if (balance <= 0) return null;

        const customerId = +(invoice.customerId ?? invoice.partyId ?? 0);
        const invoiceDateValue = invoice.invoiceDate ?? invoice.date ?? invoice.createdAt ?? invoice.createdOn;
        const invoiceDate = invoiceDateValue ? new Date(invoiceDateValue) : now;
        const ageDays = Math.max(0, Math.floor((now.getTime() - invoiceDate.getTime()) / 86400000));
        const bucket = ageDays <= 30 ? '0-30' : ageDays <= 60 ? '31-60' : ageDays <= 90 ? '61-90' : '90+';
        const customerName = customers.find((c: any) => c.id === customerId)?.name ?? `Customer #${customerId || 'N/A'}`;
        return {
          customerId,
          customerName,
          outstanding: balance,
          bucket,
          oldestDays: ageDays,
          invoiceId: +(invoice.id ?? 0)
        };
      })
      .filter((row: any) => !!row);

    return rows.sort((a: any, b: any) => b.outstanding - a.outstanding);
  }
}
