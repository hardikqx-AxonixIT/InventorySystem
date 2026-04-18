import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MasterDataService, MasterProduct } from '../../../../core/services/master-data.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <section class="master-page">
      <div class="hero-card glass-card" style="margin-bottom: 24px;">
        <div class="hero-content">
          <p class="eyebrow" style="color: var(--accent-primary)">Master Data</p>
          <h2>Products Directory</h2>
          <p>Manage your inventory assortment, pricing, and tracking rules.</p>
        </div>
        <div class="hero-actions">
          <button class="btn btn-primary" routerLink="/masters/products/new">
            <span>+</span> Add Product
          </button>
        </div>
      </div>

      <div class="message-card" *ngIf="loading">Loading products...</div>
      <div class="message-card error" *ngIf="error">{{ error }}</div>

      <div class="table-container" *ngIf="!loading && !error">
        <table class="custom-table animate-fade-in">
          <thead>
            <tr>
              <th>SKU</th>
              <th>Product Name</th>
              <th>GST %</th>
              <th>Status</th>
              <th style="text-align: right;">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of products">
              <td data-label="SKU">{{ item.sku }}</td>
              <td data-label="Name"><strong>{{ item.name }}</strong></td>
              <td data-label="GST">{{ item.gstRate }}%</td>
              <td data-label="Status">
                <span class="status-badge" [class.success]="item.isActive" [class.danger]="!item.isActive">
                  {{ item.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td data-label="Actions" style="text-align: right;">
                <button class="btn btn-secondary" style="padding: 6px 12px; margin-right: 8px;" routerLink="/masters/products/{{item.id}}/edit">Edit</button>
                <button class="btn btn-secondary" style="padding: 6px 12px;" (click)="toggleStatus(item)">
                  {{ item.isActive ? 'Deactivate' : 'Activate' }}
                </button>
              </td>
            </tr>
            <tr *ngIf="products.length === 0">
              <td colspan="5" style="text-align: center; padding: 32px;">No products found. Create one.</td>
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
export class ProductListComponent implements OnInit {
  products: MasterProduct[] = [];
  loading = true;
  error = '';

  constructor(private masterData: MasterDataService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.masterData.getBootstrap().subscribe({
      next: (data) => {
        this.products = data.products || [];
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load products';
        this.loading = false;
      }
    });
  }

  toggleStatus(item: MasterProduct): void {
    const newState = !item.isActive;
    this.masterData.setProductStatus(item.id, newState).subscribe({
      next: () => { item.isActive = newState; },
      error: () => alert('Failed to update status')
    });
  }
}
