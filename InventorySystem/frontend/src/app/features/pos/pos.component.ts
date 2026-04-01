import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pos.component.html',
  styleUrls: ['./pos.component.css']
})
export class PosComponent implements OnInit {
  data: TransactionBootstrap | null = null;
  loading = true;
  error = '';

  barcode = '';
  customerId = 0;
  warehouseId = 0;
  defaultBinId = 0;
  scanQty = 1;
  cart: any[] = [];
  lastInvoice: any = null;
  upiLink = '';
  whatsappLink = '';

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getBootstrap().subscribe({
      next: (data) => {
        this.data = data;
        this.customerId = data.customers[0]?.id ?? 0;
        this.warehouseId = data.warehouses[0]?.id ?? 0;
        this.defaultBinId = data.bins[0]?.id ?? 0;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load POS.';
        this.loading = false;
      }
    });
  }

  scanBarcode(): void {
    const code = (this.barcode ?? '').trim();
    if (!code || !this.data) return;
    const product = this.data.products.find(p => (p.barcode ?? '').toLowerCase() === code.toLowerCase());
    if (!product) {
      this.error = 'Barcode not found.';
      return;
    }

    const existing = this.cart.find(x => x.productId === product.id);
    if (existing) {
      existing.quantity += this.scanQty;
      existing.lineTotal = existing.quantity * existing.unitPrice;
    } else {
      this.cart.push({
        productId: product.id,
        productName: product.name,
        quantity: this.scanQty,
        unitPrice: +(product.salesPrice ?? 0),
        gstRate: +(product.gstRate ?? 0),
        lineTotal: this.scanQty * +(product.salesPrice ?? 0)
      });
    }

    this.barcode = '';
    this.scanQty = 1;
    this.error = '';
  }

  removeLine(index: number): void {
    this.cart.splice(index, 1);
  }

  subtotal(): number {
    return this.cart.reduce((sum, x) => sum + (+x.lineTotal || 0), 0);
  }

  checkout(): void {
    if (!this.customerId || !this.warehouseId || !this.defaultBinId || this.cart.length === 0) return;
    const orderPayload = {
      customerId: +this.customerId,
      warehouseId: +this.warehouseId,
      placeOfSupplyState: '',
      items: this.cart.map(x => ({
        productId: +x.productId,
        binId: +this.defaultBinId,
        quantity: +x.quantity,
        unitPrice: +x.unitPrice,
        gstRate: +x.gstRate
      }))
    };

    this.transactions.createSalesOrder(orderPayload).subscribe({
      next: (so) => {
        this.transactions.createSalesInvoice({ salesOrderId: so.id }).subscribe({
          next: (invoice) => {
            this.lastInvoice = invoice;
            const amount = +(invoice.grandTotal ?? this.subtotal());
            this.upiLink = `upi://pay?pa=merchant@upi&pn=Axonix%20ERP&am=${amount.toFixed(2)}&cu=INR&tn=${encodeURIComponent(invoice.invoiceNumber ?? 'POS Invoice')}`;
            const message = `Invoice ${invoice.invoiceNumber} amount INR ${amount.toFixed(2)} generated from POS.`;
            this.whatsappLink = `https://wa.me/?text=${encodeURIComponent(message)}`;
            this.cart = [];
            this.load();
          },
          error: (err) => this.error = err?.error ?? 'POS invoice failed.'
        });
      },
      error: (err) => this.error = err?.error ?? 'POS checkout failed.'
    });
  }
}
