import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LookupItem {
  id: number;
  name?: string;
  code?: string;
}

export interface MasterCategory {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface MasterUnit {
  id: number;
  code: string;
  name: string;
  symbol?: string;
  isActive: boolean;
}

export interface MasterProduct {
  id: number;
  sku: string;
  name: string;
  barcode?: string;
  hsnCode?: string;
  brand?: string;
  categoryId: number;
  uomId: number;
  reorderLevel: number;
  purchasePrice: number;
  salesPrice: number;
  gstRate: number;
  trackBatch: boolean;
  trackSerial: boolean;
  trackExpiry: boolean;
  isActive: boolean;
  isDeleted: boolean;
}

export interface MasterCustomer {
  id: number;
  name: string;
  gstin?: string;
  contactPerson?: string;
  phone?: string;
  email?: string;
  billingAddress?: string;
  shippingAddress?: string;
  paymentTermsDays: number;
  creditLimit: number;
  isActive: boolean;
}

export interface MasterVendor {
  id: number;
  name: string;
  gstin?: string;
  contactPerson?: string;
  phone?: string;
  email?: string;
  address?: string;
  paymentTermsDays: number;
  isActive: boolean;
}

export interface MasterWarehouse {
  id: number;
  name: string;
  code?: string;
  city?: string;
  state?: string;
  isActive: boolean;
}

export interface MasterBin {
  id: number;
  warehouseId: number;
  warehouseName: string;
  zone?: string;
  aisle?: string;
  shelf?: string;
  binCode: string;
  isActive: boolean;
}

export interface MasterBootstrap {
  categories: MasterCategory[];
  units: MasterUnit[];
  products: MasterProduct[];
  customers: MasterCustomer[];
  vendors: MasterVendor[];
  warehouses: MasterWarehouse[];
  bins: MasterBin[];
}

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {
  private readonly apiUrl = `${environment.apiUrl}/Masters`;

  constructor(private http: HttpClient) {}

  getBootstrap(): Observable<MasterBootstrap> {
    return this.http.get<MasterBootstrap>(`${this.apiUrl}/bootstrap`);
  }

  createCategory(payload: { name: string; description?: string }): Observable<MasterCategory> {
    return this.http.post<MasterCategory>(`${this.apiUrl}/categories`, payload);
  }

  updateCategory(id: number, payload: { name: string; description?: string }): Observable<MasterCategory> {
    return this.http.put<MasterCategory>(`${this.apiUrl}/categories/${id}`, payload);
  }

  setCategoryStatus(id: number, isActive: boolean): Observable<MasterCategory> {
    return this.http.post<MasterCategory>(`${this.apiUrl}/categories/${id}/status`, { isActive });
  }

  createUnit(payload: { code: string; name: string; symbol?: string }): Observable<MasterUnit> {
    return this.http.post<MasterUnit>(`${this.apiUrl}/units`, payload);
  }

  updateUnit(id: number, payload: { code: string; name: string; symbol?: string }): Observable<MasterUnit> {
    return this.http.put<MasterUnit>(`${this.apiUrl}/units/${id}`, payload);
  }

  setUnitStatus(id: number, isActive: boolean): Observable<MasterUnit> {
    return this.http.post<MasterUnit>(`${this.apiUrl}/units/${id}/status`, { isActive });
  }

  createProduct(payload: any): Observable<MasterProduct> {
    return this.http.post<MasterProduct>(`${this.apiUrl}/products`, payload);
  }

  updateProduct(id: number, payload: any): Observable<MasterProduct> {
    return this.http.put<MasterProduct>(`${this.apiUrl}/products/${id}`, payload);
  }

  setProductStatus(id: number, isActive: boolean): Observable<MasterProduct> {
    return this.http.post<MasterProduct>(`${this.apiUrl}/products/${id}/status`, { isActive });
  }

  createCustomer(payload: any): Observable<MasterCustomer> {
    return this.http.post<MasterCustomer>(`${this.apiUrl}/customers`, payload);
  }

  updateCustomer(id: number, payload: any): Observable<MasterCustomer> {
    return this.http.put<MasterCustomer>(`${this.apiUrl}/customers/${id}`, payload);
  }

  setCustomerStatus(id: number, isActive: boolean): Observable<MasterCustomer> {
    return this.http.post<MasterCustomer>(`${this.apiUrl}/customers/${id}/status`, { isActive });
  }

  createVendor(payload: any): Observable<MasterVendor> {
    return this.http.post<MasterVendor>(`${this.apiUrl}/vendors`, payload);
  }

  updateVendor(id: number, payload: any): Observable<MasterVendor> {
    return this.http.put<MasterVendor>(`${this.apiUrl}/vendors/${id}`, payload);
  }

  setVendorStatus(id: number, isActive: boolean): Observable<MasterVendor> {
    return this.http.post<MasterVendor>(`${this.apiUrl}/vendors/${id}/status`, { isActive });
  }

  createWarehouse(payload: any): Observable<MasterWarehouse> {
    return this.http.post<MasterWarehouse>(`${this.apiUrl}/warehouses`, payload);
  }

  updateWarehouse(id: number, payload: any): Observable<MasterWarehouse> {
    return this.http.put<MasterWarehouse>(`${this.apiUrl}/warehouses/${id}`, payload);
  }

  setWarehouseStatus(id: number, isActive: boolean): Observable<MasterWarehouse> {
    return this.http.post<MasterWarehouse>(`${this.apiUrl}/warehouses/${id}/status`, { isActive });
  }

  createBin(payload: any): Observable<MasterBin> {
    return this.http.post<MasterBin>(`${this.apiUrl}/bins`, payload);
  }

  updateBin(id: number, payload: any): Observable<MasterBin> {
    return this.http.put<MasterBin>(`${this.apiUrl}/bins/${id}`, payload);
  }

  setBinStatus(id: number, isActive: boolean): Observable<MasterBin> {
    return this.http.post<MasterBin>(`${this.apiUrl}/bins/${id}/status`, { isActive });
  }
}
