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
  activeTab: 'overview' | 'voucher' | 'drilldown' | 'statements' = 'overview';
  readonly pageSize = 25;
  ledgerPage = 1;
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
}
