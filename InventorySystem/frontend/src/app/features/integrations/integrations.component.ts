import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-integrations',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './integrations.component.html',
  styleUrls: ['./integrations.component.css']
})
export class IntegrationsComponent {
  loading = false;
  error = '';
  message = '';

  whatsappForm = {
    salesInvoiceId: 0,
    phoneNumber: ''
  };

  razorpayOrderForm = {
    amount: 0,
    currency: 'INR',
    receipt: ''
  };

  razorpayVerifyForm = {
    razorpayOrderId: '',
    razorpayPaymentId: '',
    razorpaySignature: '',
    rawPayload: ''
  };
  backupLabel = 'manual';
  tenantForm = {
    tenantId: 'demo-tenant',
    email: 'owner@demo.local',
    planCode: 'STARTER',
    months: 1,
    licenseKey: ''
  };

  constructor(private transactions: TransactionDataService) {}

  exportTallyXml(): void {
    this.transactions.downloadTallySalesXml();
    this.message = 'Tally XML export started in a new tab.';
    this.error = '';
  }

  importTallyMasters(): void {
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.importTallyMasters().subscribe({
      next: (res) => {
        this.loading = false;
        this.message = res?.note ?? 'Tally import completed.';
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Tally import failed.';
      }
    });
  }

  syncLedgersAndVouchers(): void {
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.syncTallyLedgersVouchers().subscribe({
      next: (res) => {
        this.loading = false;
        this.message = res?.note ?? 'Tally ledger sync completed.';
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Tally sync failed.';
      }
    });
  }

  exportDatabase(): void {
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.createSystemBackup(this.backupLabel).subscribe({
      next: (res) => {
        this.loading = false;
        this.message = `Backup created: ${res?.fileName ?? 'success'}`;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Backup creation failed.';
      }
    });
  }

  setupSubscriptionPlans(): void {
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.getCommercialPlans().subscribe({
      next: (res) => {
        this.loading = false;
        this.message = `Plans loaded: ${(res ?? []).map((x: any) => x.code).join(', ')}`;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Unable to load plans.';
      }
    });
  }

  configureTrialPeriod(): void {
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.startTrial({
      tenantId: this.tenantForm.tenantId,
      email: this.tenantForm.email,
      days: 14
    }).subscribe({
      next: (res) => {
        this.loading = false;
        this.message = `Trial started till ${res?.expiresAtUtc ?? ''}`;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Trial setup failed.';
      }
    });
  }

  generateLicenseKeys(): void {
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.generateLicense({
      tenantId: this.tenantForm.tenantId,
      months: this.tenantForm.months
    }).subscribe({
      next: (res) => {
        this.loading = false;
        this.tenantForm.licenseKey = res?.key ?? '';
        this.message = `License generated: ${res?.key ?? ''}`;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'License generation failed.';
      }
    });
  }

  sendWhatsAppInvoice(): void {
    if (!this.whatsappForm.salesInvoiceId || !this.whatsappForm.phoneNumber) return;
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.sendWhatsAppInvoice(this.whatsappForm).subscribe({
      next: (res) => {
        this.loading = false;
        if (res?.success) {
          this.message = 'WhatsApp API request sent successfully.';
          return;
        }
        this.message = res?.note ?? 'Provider not configured. Please set integration keys.';
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'WhatsApp send failed.';
      }
    });
  }

  createRazorpayOrder(): void {
    if (!this.razorpayOrderForm.amount) return;
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.createRazorpayOrder(this.razorpayOrderForm).subscribe({
      next: (res) => {
        this.loading = false;
        if (res?.success) {
          this.message = 'Razorpay order created.';
          return;
        }
        this.message = res?.note ?? 'Razorpay provider not configured.';
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Razorpay order creation failed.';
      }
    });
  }

  verifyRazorpayCallback(): void {
    if (!this.razorpayVerifyForm.razorpayOrderId || !this.razorpayVerifyForm.razorpayPaymentId || !this.razorpayVerifyForm.razorpaySignature) return;
    this.loading = true;
    this.error = '';
    this.message = '';
    this.transactions.verifyRazorpayCallback(this.razorpayVerifyForm).subscribe({
      next: (res) => {
        this.loading = false;
        this.message = res?.verified ? 'Callback signature verified.' : 'Callback signature did not match.';
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Callback verification failed.';
      }
    });
  }
}
