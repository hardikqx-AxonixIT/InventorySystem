import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardOverview {
  kpis: DashboardKpis;
  modules: DashboardModule[];
  integrations: string[];
  nextMilestones: string[];
  remainingGaps: string[];
  recommendations: string[];
}

export interface DashboardKpis {
  totalProducts: number;
  activeWarehouses: number;
  pendingAdjustments: number;
  lowStockItems: number;
  inventoryValueEstimate: number;
  totalReceivables: number;
  totalPayables: number;
  cashBalance: number;
  todaySales: number;
  totalRevenue: number;
  pendingSalesOrders: number;
  lowStockAlerts: number;
}

export interface DashboardModule {
  name: string;
  status: string;
  description: string;
  features: string[];
}

export interface ProductListItem {
  id: number;
  sku: string;
  name: string;
  categoryId: number;
  uomId: number;
  barcode?: string;
  hsnCode?: string;
  brand?: string;
  reorderLevel: number;
  purchasePrice: number;
  salesPrice: number;
  gstRate: number;
  trackBatch: boolean;
  trackSerial: boolean;
  trackExpiry: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl = `${environment.apiUrl}/Inventory`;
  private dashboardApiUrl = `${environment.apiUrl}/Dashboard`;

  constructor(private http: HttpClient) { }

  private getHeaders() {
    const token = localStorage.getItem('token');
    if (!token) {
      return new HttpHeaders();
    }
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  requestAdjustment(productId: number, amount: number, reason: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/adjustments/request`, { productId, amount, reason }, { headers: this.getHeaders() });
  }

  getPendingAdjustments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/adjustments/pending`, { headers: this.getHeaders() });
  }

  approveAdjustment(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/adjustments/${id}/approve`, {}, { headers: this.getHeaders() });
  }

  getDashboardOverview(): Observable<DashboardOverview> {
    return this.http.get<DashboardOverview>(`${this.dashboardApiUrl}/overview`);
  }

  getProducts(): Observable<ProductListItem[]> {
    return this.http.get<ProductListItem[]>(`${environment.apiUrl}/Products`);
  }
}
