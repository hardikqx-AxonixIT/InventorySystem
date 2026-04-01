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

  constructor(private transactions: TransactionDataService) {}

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
