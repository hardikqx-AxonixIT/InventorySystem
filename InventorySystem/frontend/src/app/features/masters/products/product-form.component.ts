import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MasterDataService } from '../../../../core/services/master-data.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <section class="master-page">
      <div class="form-header">
        <a routerLink="/masters/products" class="back-link">← Back to Products</a>
        <h2>{{ isEditMode ? 'Edit Product' : 'Create New Product' }}</h2>
      </div>

      <div class="message-card error" *ngIf="error">{{ error }}</div>

      <div class="glass-card form-container animate-fade-in">
        <form [formGroup]="productForm" (ngSubmit)="onSubmit()">
          
          <div class="form-section">
            <h3 class="section-title">Basic Information</h3>
            <div class="form-grid">
              <div class="form-group">
                <label>Product Name *</label>
                <input formControlName="name" placeholder="Enter product name" />
              </div>
              <div class="form-group">
                <label>SKU / Item Code *</label>
                <input formControlName="sku" placeholder="Unique identifier" />
              </div>
              <div class="form-group">
                <label>Brand</label>
                <input formControlName="brand" placeholder="e.g. Samsung" />
              </div>
              <div class="form-group">
                <label>Category</label>
                <select formControlName="categoryId">
                  <option [ngValue]="0">Select Category</option>
                  <option *ngFor="let c of categories" [ngValue]="c.id">{{ c.name }}</option>
                </select>
              </div>
            </div>
          </div>

          <div class="form-section">
            <h3 class="section-title">Pricing & Tax</h3>
            <div class="form-grid">
              <div class="form-group">
                <label>Purchase Price</label>
                <input type="number" formControlName="purchasePrice" placeholder="0.00" />
              </div>
              <div class="form-group">
                <label>Sales Price *</label>
                <input type="number" formControlName="salesPrice" placeholder="0.00" />
              </div>
              <div class="form-group">
                <label>GST Rate % *</label>
                <input type="number" formControlName="gstRate" placeholder="e.g. 18" />
              </div>
              <div class="form-group">
                <label>HSN / SAC Code</label>
                <input formControlName="hsnCode" placeholder="Enter HSN" />
              </div>
            </div>
          </div>

          <div class="form-section">
            <h3 class="section-title">Inventory & Tracking Config</h3>
            <div class="form-grid">
              <div class="form-group">
                <label>Primary Unit</label>
                <select formControlName="uomId">
                  <option [ngValue]="0">Select Unit</option>
                  <option *ngFor="let u of units" [ngValue]="u.id">{{ u.name }}</option>
                </select>
              </div>
              <div class="form-group">
                <label>Low Stock Warning Level</label>
                <input type="number" formControlName="reorderLevel" placeholder="10" />
              </div>
            </div>
            
            <div class="checkbox-group">
              <label class="check-container">
                <input type="checkbox" formControlName="trackBatch" />
                <span class="checkmark"></span>
                Require Batch Tracking
              </label>
              <label class="check-container">
                <input type="checkbox" formControlName="trackExpiry" />
                <span class="checkmark"></span>
                Require Expiry Date (FMCG/Pharma)
              </label>
            </div>
          </div>

          <div class="form-actions">
            <button type="button" class="btn btn-secondary" routerLink="/masters/products">Cancel</button>
            <button type="submit" class="btn btn-primary" [disabled]="saving || productForm.invalid">
              {{ saving ? 'Saving...' : (isEditMode ? 'Update Product' : 'Save Product') }}
            </button>
          </div>
        </form>
      </div>
    </section>
  `,
  styles: [`
    .master-page { padding: 32px; max-width: 900px; margin: 0 auto; }
    .form-header { margin-bottom: 24px; }
    .back-link { color: var(--text-secondary); text-decoration: none; font-size: 0.9rem; margin-bottom: 8px; display: inline-block; transition: color 0.2s; }
    .back-link:hover { color: var(--text-primary); }
    .form-header h2 { font-size: 1.8rem; font-weight: 600; }
    
    .form-container { padding: 32px; }
    .form-section { margin-bottom: 32px; padding-bottom: 32px; border-bottom: 1px solid var(--border-subtle); }
    .form-section:last-of-type { border-bottom: none; margin-bottom: 0; padding-bottom: 0; }
    .section-title { font-size: 1.1rem; color: var(--accent-primary); margin-bottom: 16px; font-weight: 500; }
    
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 20px; }
    .form-group { display: flex; flex-direction: column; gap: 8px; }
    .form-group label { font-size: 0.85rem; color: var(--text-secondary); font-weight: 500; }
    
    .checkbox-group { display: flex; gap: 24px; margin-top: 20px; }
    .check-container { display: flex; align-items: center; gap: 8px; color: var(--text-primary); font-size: 0.95rem; cursor: pointer; }
    .check-container input { width: auto; scale: 1.2; accent-color: var(--accent-primary); }
    
    .form-actions { display: flex; justify-content: flex-end; gap: 16px; margin-top: 32px; padding-top: 24px; border-top: 1px solid var(--border-subtle); }
  `]
})
export class ProductFormComponent implements OnInit {
  productForm!: FormGroup;
  isEditMode = false;
  productId: number | null = null;
  saving = false;
  error = '';
  
  categories: any[] = [];
  units: any[] = [];

  constructor(
    private fb: FormBuilder,
    private masterData: MasterDataService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadLookups();
    
    this.route.paramMap.subscribe(params => {
      const idStr = params.get('id');
      if (idStr) {
        this.isEditMode = true;
        this.productId = +idStr;
        this.loadProduct(this.productId);
      }
    });
  }

  initForm(): void {
    this.productForm = this.fb.group({
      sku: ['', [Validators.required, Validators.maxLength(50)]],
      name: ['', [Validators.required, Validators.maxLength(150)]],
      brand: [''],
      categoryId: [0],
      uomId: [0],
      purchasePrice: [0],
      salesPrice: [0, Validators.required],
      gstRate: [0, Validators.required],
      hsnCode: [''],
      reorderLevel: [10],
      trackBatch: [false],
      trackExpiry: [false]
    });
  }

  loadLookups(): void {
    this.masterData.getBootstrap().subscribe({
      next: (data) => {
        this.categories = data.categories || [];
        this.units = data.units || [];
      }
    });
  }

  loadProduct(id: number): void {
    // Simulating loading from bootstrap list for MVP simplicity
    this.masterData.getBootstrap().subscribe({
      next: (data) => {
        const prod = (data.products || []).find(p => p.id === id);
        if (prod) {
          this.productForm.patchValue(prod);
        } else {
          this.error = 'Product not found.';
        }
      }
    });
  }

  onSubmit(): void {
    if (this.productForm.invalid) return;
    
    this.saving = true;
    this.error = '';
    const payload = this.productForm.value;

    const request$ = this.isEditMode 
      ? this.masterData.updateProduct(this.productId!, payload)
      : this.masterData.createProduct(payload);

    request$.subscribe({
      next: () => {
        this.router.navigate(['/masters/products']);
      },
      error: (err) => {
        this.error = err.error?.message || 'An error occurred while saving.';
        this.saving = false;
      }
    });
  }
}
