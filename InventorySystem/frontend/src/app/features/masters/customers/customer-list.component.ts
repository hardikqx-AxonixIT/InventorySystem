import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MasterDataService, MasterCustomer } from '../../../../core/services/master-data.service';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <section class="master-page">
      <div class="hero-card glass-card" style="margin-bottom: 24px;">
        <div class="hero-content">
          <p class="eyebrow" style="color: var(--accent-primary)">Master Data</p>
          <h2>Customer Ledger</h2>
          <p>Track your clients, their GST compliance, and credit limits.</p>
        </div>
        <div class="hero-actions">
          <button class="btn btn-primary" routerLink="/masters/customers/new">
            <span>+</span> Add Customer
          </button>
        </div>
      </div>

      <div class="message-card" *ngIf="loading">Loading customers...</div>
      <div class="message-card error" *ngIf="error">{{ error }}</div>

      <div class="table-container" *ngIf="!loading && !error">
        <table class="custom-table animate-fade-in">
          <thead>
            <tr>
              <th>Customer Name</th>
              <th>GSTIN</th>
              <th>Credit Limit</th>
              <th>Status</th>
              <th style="text-align: right;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of customers">
              <td data-label="Name"><strong>{{ item.name }}</strong></td>
              <td data-label="GSTIN">{{ item.gstin || 'Unregistered' }}</td>
              <td data-label="Credit Limit">{{ item.creditLimit | currency:'INR' }}</td>
              <td data-label="Status">
                <span class="status-badge" [class.success]="item.isActive" [class.danger]="!item.isActive">
                  {{ item.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td data-label="Actions" style="text-align: right;">
                <button class="btn btn-secondary" style="padding: 6px 12px; margin-right: 8px;" routerLink="/masters/customers/{{item.id}}/edit">Edit</button>
                <button class="btn btn-secondary" style="padding: 6px 12px;" (click)="toggleStatus(item)">
                  {{ item.isActive ? 'Deactivate' : 'Activate' }}
                </button>
              </td>
            </tr>
            <tr *ngIf="customers.length === 0">
              <td colspan="5" style="text-align: center; padding: 32px;">No customers found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  `,
  styles: [`
    .master-page { padding: 32px; max-width: 1400px; margin: 0 auto; }
    .hero-card { padding: 32px; display: flex; justify-content: space-between; align-items: center; }
  `]
})
export class CustomerListComponent implements OnInit {
  customers: MasterCustomer[] = [];
  loading = true;
  error = '';

  constructor(private masterData: MasterDataService) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.loading = true;
    this.masterData.getBootstrap().subscribe({
      next: (data) => {
        this.customers = data.customers || [];
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load customers';
        this.loading = false;
      }
    });
  }

  toggleStatus(item: MasterCustomer): void {
    const newState = !item.isActive;
    this.masterData.setCustomerStatus(item.id, newState).subscribe({
      next: () => { item.isActive = newState; },
      error: () => alert('Failed to update status')
    });
  }
}
