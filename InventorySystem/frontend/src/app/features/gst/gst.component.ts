import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-gst',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './gst.component.html',
  styleUrls: ['./gst.component.css']
})
export class GstComponent implements OnInit {
  data: TransactionBootstrap | null = null;
  advanced: any = null;
  error = '';
  filter = { fromDate: '', toDate: '', downloadCsv: false };
  gstr1: any = null;
  gstr3b: any = null;
  eWayForm = { salesInvoiceId: 0, vehicleNumber: '', distanceKm: 120 };
  eInvoiceResult: any = null;
  eWayResult: any = null;
  automationMessage = '';

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.transactions.getBootstrap().subscribe({
      next: (data) => this.data = data,
      error: () => this.error = 'Unable to load GST summary.'
    });
    this.transactions.getAdvancedSnapshot().subscribe({
      next: (data) => this.advanced = data,
      error: () => {}
    });
  }

  runGstr1(): void {
    this.transactions.getGstr1(this.filter).subscribe({
      next: (data) => this.gstr1 = data,
      error: (err) => this.error = err?.error ?? 'Unable to generate GSTR-1.'
    });
  }

  runGstr3b(): void {
    this.transactions.getGstr3b(this.filter).subscribe({
      next: (data) => this.gstr3b = data,
      error: (err) => this.error = err?.error ?? 'Unable to generate GSTR-3B.'
    });
  }

  generateIrn(invoiceId: number): void {
    this.transactions.generateEInvoice({ salesInvoiceId: invoiceId }).subscribe({
      next: (data) => this.eInvoiceResult = data,
      error: (err) => this.error = err?.error ?? 'E-invoice generation failed.'
    });
  }

  generateEWayBill(): void {
    if (!this.eWayForm.salesInvoiceId) return;
    this.transactions.generateEWayBill(this.eWayForm).subscribe({
      next: (data) => this.eWayResult = data,
      error: (err) => this.error = err?.error ?? 'E-way bill generation failed.'
    });
  }

  fileGstViaGsp(): void {
    this.transactions.fileGstViaGsp(this.filter).subscribe({
      next: (res) => this.automationMessage = res?.note ?? 'GST filing request sent.',
      error: (err) => this.error = err?.error ?? 'GST filing via GSP failed.'
    });
  }
}
