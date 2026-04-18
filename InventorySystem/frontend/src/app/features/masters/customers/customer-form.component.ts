import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MasterDataService } from '../../../../core/services/master-data.service';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <section class="master-page">
      <div class="form-header">
        <a routerLink="/masters/customers" class="back-link">← Back to Customers</a>
        <h2>{{ isEditMode ? 'Edit Customer' : 'Register New Customer' }}</h2>
      </div>

      <div class="message-card error" *ngIf="error">{{ error }}</div>

      <div class="glass-card form-container animate-fade-in">
        <form [formGroup]="customerForm" (ngSubmit)="onSubmit()">
          
          <div class="form-section">
            <h3 class="section-title">Business Details</h3>
            <div class="form-grid">
              <div class="form-group">
                <label>Company / Full Name *</label>
                <input formControlName="name" placeholder="Enter business name" />
              </div>
              <div class="form-group">
                <label>Contact Person</label>
                <input formControlName="contactPerson" placeholder="Name of primary contact" />
              </div>
              <div class="form-group">
                <label>Phone Number</label>
                <input formControlName="phone" placeholder="+91" />
              </div>
              <div class="form-group">
                <label>Email Address</label>
                <input type="email" formControlName="email" placeholder="client@example.com" />
              </div>
            </div>
          </div>

          <div class="form-section">
            <h3 class="section-title">KYC & Compliance (India)</h3>
            <div class="form-grid">
              <div class="form-group">
                <label>GSTIN</label>
                <input formControlName="gstin" placeholder="15-digit GST Number" />
              </div>
              <div class="form-group">
                <label>PAN or Aadhar</label>
                <input formControlName="pan" placeholder="Optional for compliance" />
              </div>
              <div class="form-group">
                <label>UPI ID (VPA)</label>
                <input formControlName="upiId" placeholder="e.g. mobile@upi" />
              </div>
            </div>
          </div>

          <div class="form-section">
            <h3 class="section-title">Address & Accounting</h3>
            <div class="form-grid" style="grid-template-columns: 1fr;">
              <div class="form-group">
                <label>Billing Address</label>
                <textarea formControlName="billingAddress" rows="2" placeholder="Full address for tax invoices"></textarea>
              </div>
            </div>
            <div class="form-grid" style="margin-top: 20px;">
              <div class="form-group">
                <label>Payment Terms (Days)</label>
                <input type="number" formControlName="paymentTermsDays" placeholder="30" />
              </div>
              <div class="form-group">
                <label>Credit Limit (INR)</label>
                <input type="number" formControlName="creditLimit" placeholder="0.00" />
              </div>
            </div>
          </div>

          <div class="form-actions">
            <button type="button" class="btn btn-secondary" routerLink="/masters/customers">Cancel</button>
            <button type="submit" class="btn btn-primary" [disabled]="saving || customerForm.invalid">
              {{ saving ? 'Saving...' : (isEditMode ? 'Update Customer' : 'Save Customer') }}
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
    
    .form-actions { display: flex; justify-content: flex-end; gap: 16px; margin-top: 32px; padding-top: 24px; border-top: 1px solid var(--border-subtle); }
  `]
})
export class CustomerFormComponent implements OnInit {
  customerForm!: FormGroup;
  isEditMode = false;
  customerId: number | null = null;
  saving = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private masterData: MasterDataService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.initForm();
    
    this.route.paramMap.subscribe(params => {
      const idStr = params.get('id');
      if (idStr) {
        this.isEditMode = true;
        this.customerId = +idStr;
        this.loadCustomer(this.customerId);
      }
    });
  }

  initForm(): void {
    this.customerForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(180)]],
      contactPerson: [''],
      phone: [''],
      email: [''],
      gstin: [''],
      pan: [''],
      upiId: [''],
      billingAddress: [''],
      paymentTermsDays: [0],
      creditLimit: [0]
    });
  }

  loadCustomer(id: number): void {
    this.masterData.getBootstrap().subscribe({
      next: (data) => {
        const cust = (data.customers || []).find(c => c.id === id);
        if (cust) {
          this.customerForm.patchValue(cust);
        } else {
          this.error = 'Customer not found.';
        }
      }
    });
  }

  onSubmit(): void {
    if (this.customerForm.invalid) return;
    
    this.saving = true;
    this.error = '';
    const payload = this.customerForm.value;

    const request$ = this.isEditMode 
      ? this.masterData.updateCustomer(this.customerId!, payload)
      : this.masterData.createCustomer(payload);

    request$.subscribe({
      next: () => {
        this.router.navigate(['/masters/customers']);
      },
      error: (err) => {
        this.error = err.error?.message || 'An error occurred while saving.';
        this.saving = false;
      }
    });
  }
}
