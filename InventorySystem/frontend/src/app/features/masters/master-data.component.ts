import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MasterBootstrap,
  MasterCategory,
  MasterDataService,
  MasterUnit,
  MasterProduct,
  MasterCustomer,
  MasterVendor,
  MasterWarehouse,
  MasterBin
} from '../../core/services/master-data.service';

type MasterTab = 'categories' | 'units' | 'products' | 'customers' | 'vendors' | 'warehouses' | 'bins';

@Component({
  selector: 'app-master-data',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './master-data.component.html',
  styleUrls: ['./master-data.component.css']
})
export class MasterDataComponent implements OnInit {
  loading = true;
  saving = false;
  error = '';
  validationMessage = '';
  successMessage = '';
  activeTab: MasterTab = 'products';
  pageSize = 25;
  pages: Record<MasterTab, number> = {
    categories: 1,
    units: 1,
    products: 1,
    customers: 1,
    vendors: 1,
    warehouses: 1,
    bins: 1
  };

  editIds: Record<MasterTab, number | null> = {
    categories: null,
    units: null,
    products: null,
    customers: null,
    vendors: null,
    warehouses: null,
    bins: null
  };

  data: MasterBootstrap = {
    categories: [],
    units: [],
    products: [],
    customers: [],
    vendors: [],
    warehouses: [],
    bins: []
  };

  categoryForm = this.fb.group({
    name: ['', Validators.required],
    description: ['']
  });

  unitForm = this.fb.group({
    code: ['', Validators.required],
    name: ['', Validators.required],
    symbol: ['']
  });

  productForm = this.fb.group({
    sku: ['', Validators.required],
    name: ['', Validators.required],
    barcode: [''],
    hsnCode: [''],
    brand: [''],
    categoryId: [0, Validators.min(1)],
    uomId: [0, Validators.min(1)],
    reorderLevel: [0, Validators.required],
    purchasePrice: [0, Validators.required],
    salesPrice: [0, Validators.required],
    gstRate: [0, Validators.required],
    trackBatch: [false],
    trackSerial: [false],
    trackExpiry: [false]
  });

  customerForm = this.fb.group({
    name: ['', Validators.required],
    gstin: [''],
    contactPerson: [''],
    phone: [''],
    email: [''],
    billingAddress: [''],
    shippingAddress: [''],
    paymentTermsDays: [0, Validators.required],
    creditLimit: [0, Validators.required]
  });

  vendorForm = this.fb.group({
    name: ['', Validators.required],
    gstin: [''],
    contactPerson: [''],
    phone: [''],
    email: [''],
    address: [''],
    paymentTermsDays: [0, Validators.required]
  });

  warehouseForm = this.fb.group({
    name: ['', Validators.required],
    code: [''],
    addressLine1: [''],
    addressLine2: [''],
    city: [''],
    state: [''],
    postalCode: ['']
  });

  binForm = this.fb.group({
    warehouseId: [0, Validators.min(1)],
    binCode: ['', Validators.required],
    zone: [''],
    aisle: [''],
    shelf: ['']
  });

  constructor(private fb: FormBuilder, private masterDataService: MasterDataService) {}

  ngOnInit(): void {
    this.loadData();
  }

  setTab(tab: MasterTab): void {
    this.activeTab = tab;
    this.error = '';
  }

  loadData(): void {
    this.loading = true;
    this.masterDataService.getBootstrap().subscribe({
      next: (data) => {
        this.data = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load master data.';
        this.loading = false;
      }
    });
  }

  submitCategory(): void {
    this.validationMessage = '';
    if (this.categoryForm.invalid) { this.validationMessage = 'Category name is required.'; return; }
    const raw = this.categoryForm.getRawValue();
    const payload = { name: raw.name ?? '', description: raw.description ?? '' };
    const id = this.editIds.categories;
    this.save(
      () => id ? this.masterDataService.updateCategory(id, payload) : this.masterDataService.createCategory(payload),
      () => this.resetCategory()
    );
  }

  submitUnit(): void {
    this.validationMessage = '';
    if (this.unitForm.invalid) { this.validationMessage = 'Unit code and name are required.'; return; }
    const raw = this.unitForm.getRawValue();
    const payload = { code: raw.code ?? '', name: raw.name ?? '', symbol: raw.symbol ?? '' };
    const id = this.editIds.units;
    this.save(
      () => id ? this.masterDataService.updateUnit(id, payload) : this.masterDataService.createUnit(payload),
      () => this.resetUnit()
    );
  }

  submitProduct(): void {
    this.validationMessage = '';
    if (this.productForm.invalid) { this.validationMessage = 'Complete required product fields before save.'; return; }
    const payload = this.productForm.getRawValue();
    const id = this.editIds.products;
    this.save(
      () => id ? this.masterDataService.updateProduct(id, payload) : this.masterDataService.createProduct(payload),
      () => this.resetProduct()
    );
  }

  submitCustomer(): void {
    this.validationMessage = '';
    if (this.customerForm.invalid) { this.validationMessage = 'Customer name and payment settings are required.'; return; }
    const payload = this.customerForm.getRawValue();
    const id = this.editIds.customers;
    this.save(
      () => id ? this.masterDataService.updateCustomer(id, payload) : this.masterDataService.createCustomer(payload),
      () => this.resetCustomer()
    );
  }

  submitVendor(): void {
    this.validationMessage = '';
    if (this.vendorForm.invalid) { this.validationMessage = 'Vendor name and payment terms are required.'; return; }
    const payload = this.vendorForm.getRawValue();
    const id = this.editIds.vendors;
    this.save(
      () => id ? this.masterDataService.updateVendor(id, payload) : this.masterDataService.createVendor(payload),
      () => this.resetVendor()
    );
  }

  submitWarehouse(): void {
    this.validationMessage = '';
    if (this.warehouseForm.invalid) { this.validationMessage = 'Warehouse name is required.'; return; }
    const payload = this.warehouseForm.getRawValue();
    const id = this.editIds.warehouses;
    this.save(
      () => id ? this.masterDataService.updateWarehouse(id, payload) : this.masterDataService.createWarehouse(payload),
      () => this.resetWarehouse()
    );
  }

  submitBin(): void {
    this.validationMessage = '';
    if (this.binForm.invalid) { this.validationMessage = 'Warehouse and bin code are required.'; return; }
    const payload = this.binForm.getRawValue();
    const id = this.editIds.bins;
    this.save(
      () => id ? this.masterDataService.updateBin(id, payload) : this.masterDataService.createBin(payload),
      () => this.resetBin()
    );
  }

  startEditCategory(item: MasterCategory): void {
    this.editIds.categories = item.id;
    this.categoryForm.patchValue({ name: item.name, description: item.description ?? '' });
    this.setTab('categories');
  }

  startEditUnit(item: MasterUnit): void {
    this.editIds.units = item.id;
    this.unitForm.patchValue({ code: item.code, name: item.name, symbol: item.symbol ?? '' });
    this.setTab('units');
  }

  startEditProduct(item: MasterProduct): void {
    this.editIds.products = item.id;
    this.productForm.patchValue({
      sku: item.sku,
      name: item.name,
      barcode: item.barcode ?? '',
      hsnCode: item.hsnCode ?? '',
      brand: item.brand ?? '',
      categoryId: item.categoryId,
      uomId: item.uomId,
      reorderLevel: item.reorderLevel,
      purchasePrice: item.purchasePrice,
      salesPrice: item.salesPrice,
      gstRate: item.gstRate,
      trackBatch: item.trackBatch,
      trackSerial: item.trackSerial,
      trackExpiry: item.trackExpiry
    });
    this.setTab('products');
  }

  startEditCustomer(item: MasterCustomer): void {
    this.editIds.customers = item.id;
    this.customerForm.patchValue({
      name: item.name,
      gstin: item.gstin ?? '',
      contactPerson: item.contactPerson ?? '',
      phone: item.phone ?? '',
      email: item.email ?? '',
      billingAddress: item.billingAddress ?? '',
      shippingAddress: item.shippingAddress ?? '',
      paymentTermsDays: item.paymentTermsDays,
      creditLimit: item.creditLimit
    });
    this.setTab('customers');
  }

  startEditVendor(item: MasterVendor): void {
    this.editIds.vendors = item.id;
    this.vendorForm.patchValue({
      name: item.name,
      gstin: item.gstin ?? '',
      contactPerson: item.contactPerson ?? '',
      phone: item.phone ?? '',
      email: item.email ?? '',
      address: item.address ?? '',
      paymentTermsDays: item.paymentTermsDays
    });
    this.setTab('vendors');
  }

  startEditWarehouse(item: MasterWarehouse): void {
    this.editIds.warehouses = item.id;
    this.warehouseForm.patchValue({
      name: item.name,
      code: item.code ?? '',
      city: item.city ?? '',
      state: item.state ?? '',
      postalCode: '',
      addressLine1: '',
      addressLine2: ''
    });
    this.setTab('warehouses');
  }

  startEditBin(item: MasterBin): void {
    this.editIds.bins = item.id;
    this.binForm.patchValue({
      warehouseId: item.warehouseId,
      binCode: item.binCode,
      zone: item.zone ?? '',
      aisle: item.aisle ?? '',
      shelf: item.shelf ?? ''
    });
    this.setTab('bins');
  }

  toggleCategory(item: MasterCategory): void { this.confirmToggle(item.isActive, 'category') && this.save(() => this.masterDataService.setCategoryStatus(item.id, !item.isActive)); }
  toggleUnit(item: MasterUnit): void { this.confirmToggle(item.isActive, 'unit') && this.save(() => this.masterDataService.setUnitStatus(item.id, !item.isActive)); }
  toggleProduct(item: MasterProduct): void { this.confirmToggle(item.isActive, 'product') && this.save(() => this.masterDataService.setProductStatus(item.id, !item.isActive)); }
  toggleCustomer(item: MasterCustomer): void { this.confirmToggle(item.isActive, 'customer') && this.save(() => this.masterDataService.setCustomerStatus(item.id, !item.isActive)); }
  toggleVendor(item: MasterVendor): void { this.confirmToggle(item.isActive, 'vendor') && this.save(() => this.masterDataService.setVendorStatus(item.id, !item.isActive)); }
  toggleWarehouse(item: MasterWarehouse): void { this.confirmToggle(item.isActive, 'warehouse') && this.save(() => this.masterDataService.setWarehouseStatus(item.id, !item.isActive)); }
  toggleBin(item: MasterBin): void { this.confirmToggle(item.isActive, 'bin') && this.save(() => this.masterDataService.setBinStatus(item.id, !item.isActive)); }

  exportProductsCsv(): void {
    const header = ['sku', 'name', 'barcode', 'hsnCode', 'brand', 'categoryId', 'uomId', 'reorderLevel', 'purchasePrice', 'salesPrice', 'gstRate', 'trackBatch', 'trackSerial', 'trackExpiry'];
    const rows = this.data.products.map(p => [
      p.sku, p.name, p.barcode ?? '', p.hsnCode ?? '', p.brand ?? '', p.categoryId, p.uomId, p.reorderLevel, p.purchasePrice, p.salesPrice, p.gstRate, p.trackBatch, p.trackSerial, p.trackExpiry
    ]);
    const csv = [header.join(','), ...rows.map(r => r.map(v => `"${String(v).replace(/"/g, '""')}"`).join(','))].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'products_export.csv';
    a.click();
    URL.revokeObjectURL(url);
  }

  onProductsCsvSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
      const content = String(reader.result ?? '');
      const lines = content.split(/\r?\n/).filter(x => x.trim().length > 0);
      if (lines.length < 2) return;
      const header = lines[0].split(',').map(x => x.trim().replace(/^"|"$/g, ''));
      const idx = (name: string) => header.findIndex(h => h.toLowerCase() === name.toLowerCase());

      const parseLine = (line: string): string[] => {
        const out: string[] = [];
        let cur = '';
        let quoted = false;
        for (let i = 0; i < line.length; i++) {
          const ch = line[i];
          if (ch === '"' && line[i + 1] === '"') { cur += '"'; i++; continue; }
          if (ch === '"') { quoted = !quoted; continue; }
          if (ch === ',' && !quoted) { out.push(cur); cur = ''; continue; }
          cur += ch;
        }
        out.push(cur);
        return out;
      };

      const payloads = lines.slice(1).map(line => {
        const cols = parseLine(line);
        return {
          sku: cols[idx('sku')] ?? '',
          name: cols[idx('name')] ?? '',
          barcode: cols[idx('barcode')] ?? '',
          hsnCode: cols[idx('hsnCode')] ?? '',
          brand: cols[idx('brand')] ?? '',
          categoryId: +(cols[idx('categoryId')] ?? 0),
          uomId: +(cols[idx('uomId')] ?? 0),
          reorderLevel: +(cols[idx('reorderLevel')] ?? 0),
          purchasePrice: +(cols[idx('purchasePrice')] ?? 0),
          salesPrice: +(cols[idx('salesPrice')] ?? 0),
          gstRate: +(cols[idx('gstRate')] ?? 0),
          trackBatch: String(cols[idx('trackBatch')] ?? '').toLowerCase() === 'true',
          trackSerial: String(cols[idx('trackSerial')] ?? '').toLowerCase() === 'true',
          trackExpiry: String(cols[idx('trackExpiry')] ?? '').toLowerCase() === 'true'
        };
      }).filter(x => x.sku && x.name && x.categoryId > 0 && x.uomId > 0);

      const runSequential = (index: number) => {
        if (index >= payloads.length) { this.loadData(); return; }
        this.masterDataService.createProduct(payloads[index]).subscribe({
          next: () => runSequential(index + 1),
          error: () => runSequential(index + 1)
        });
      };
      runSequential(0);
    };
    reader.readAsText(file);
  }

  resetCategory(): void { this.editIds.categories = null; this.categoryForm.reset({ name: '', description: '' }); }
  resetUnit(): void { this.editIds.units = null; this.unitForm.reset({ code: '', name: '', symbol: '' }); }
  resetProduct(): void {
    this.editIds.products = null;
    this.productForm.reset({ sku: '', name: '', barcode: '', hsnCode: '', brand: '', categoryId: 0, uomId: 0, reorderLevel: 0, purchasePrice: 0, salesPrice: 0, gstRate: 0, trackBatch: false, trackSerial: false, trackExpiry: false });
  }
  resetCustomer(): void {
    this.editIds.customers = null;
    this.customerForm.reset({ name: '', gstin: '', contactPerson: '', phone: '', email: '', billingAddress: '', shippingAddress: '', paymentTermsDays: 0, creditLimit: 0 });
  }
  resetVendor(): void {
    this.editIds.vendors = null;
    this.vendorForm.reset({ name: '', gstin: '', contactPerson: '', phone: '', email: '', address: '', paymentTermsDays: 0 });
  }
  resetWarehouse(): void {
    this.editIds.warehouses = null;
    this.warehouseForm.reset({ name: '', code: '', addressLine1: '', addressLine2: '', city: '', state: '', postalCode: '' });
  }
  resetBin(): void {
    this.editIds.bins = null;
    this.binForm.reset({ warehouseId: 0, binCode: '', zone: '', aisle: '', shelf: '' });
  }

  private save(requestFactory: () => any, onSuccess?: () => void): void {
    this.saving = true;
    this.error = '';
    this.successMessage = '';
    requestFactory().subscribe({
      next: () => {
        if (onSuccess) onSuccess();
        this.loadData();
        this.successMessage = 'Saved successfully.';
        this.saving = false;
      },
      error: (err: any) => {
        this.error = err?.error ?? 'Operation failed.';
        this.saving = false;
      }
    });
  }

  private confirmToggle(isActive: boolean, entity: string): boolean {
    return confirm(`${isActive ? 'Deactivate' : 'Activate'} this ${entity}?`);
  }

  pagedRows<T>(tab: MasterTab, rows: T[]): T[] {
    const page = this.pages[tab] || 1;
    const start = (page - 1) * this.pageSize;
    return rows.slice(start, start + this.pageSize);
  }

  totalPages(rows: any[]): number {
    return Math.max(1, Math.ceil((rows?.length ?? 0) / this.pageSize));
  }

  nextPage(tab: MasterTab, rows: any[]): void {
    const max = this.totalPages(rows);
    if (this.pages[tab] >= max) return;
    this.pages[tab] += 1;
  }

  prevPage(tab: MasterTab): void {
    if (this.pages[tab] <= 1) return;
    this.pages[tab] -= 1;
  }

  pageInfo(tab: MasterTab, rows: any[]): string {
    const total = rows?.length ?? 0;
    if (!total) return '0-0 of 0';
    const page = this.pages[tab] || 1;
    const start = ((page - 1) * this.pageSize) + 1;
    const end = Math.min(page * this.pageSize, total);
    return `${start}-${end} of ${total}`;
  }
}
