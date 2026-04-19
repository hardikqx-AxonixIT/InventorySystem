import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { ShellComponent } from './features/shell/shell.component';
import { AuthGuardService } from './core/services/auth-guard.service';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'inventory', loadComponent: () => import('./features/inventory/inventory-list/inventory-list.component').then(m => m.InventoryListComponent) },
      { path: 'inventory/adjustments', loadComponent: () => import('./features/inventory/adjustments/inventory-adjustments.component').then(m => m.InventoryAdjustmentsComponent) },
      { path: 'inventory/history/:id', loadComponent: () => import('./features/inventory/stock-history/stock-history.component').then(m => m.StockHistoryComponent) },
      { path: 'masters', loadComponent: () => import('./features/masters/master-data.component').then(m => m.MasterDataComponent) },
      { path: 'masters/products', loadComponent: () => import('./features/masters/products/product-list.component').then(m => m.ProductListComponent) },
      { path: 'masters/products/new', loadComponent: () => import('./features/masters/products/product-form.component').then(m => m.ProductFormComponent) },
      { path: 'masters/products/:id/edit', loadComponent: () => import('./features/masters/products/product-form.component').then(m => m.ProductFormComponent) },
      { path: 'masters/customers', loadComponent: () => import('./features/masters/customers/customer-list.component').then(m => m.CustomerListComponent) },
      { path: 'masters/customers/new', loadComponent: () => import('./features/masters/customers/customer-form.component').then(m => m.CustomerFormComponent) },
      { path: 'masters/customers/:id/edit', loadComponent: () => import('./features/masters/customers/customer-form.component').then(m => m.CustomerFormComponent) },
      { path: 'purchase', loadComponent: () => import('./features/purchase/purchase.component').then(m => m.PurchaseComponent) },
      { path: 'sales', loadComponent: () => import('./features/sales/sales.component').then(m => m.SalesComponent) },
      { path: 'gst', loadComponent: () => import('./features/gst/gst.component').then(m => m.GstComponent) },
      { path: 'accounting', loadComponent: () => import('./features/accounting/accounting.component').then(m => m.AccountingComponent) },
      { path: 'warehouse', loadComponent: () => import('./features/warehouse/warehouse.component').then(m => m.WarehouseComponent) },
      { path: 'manufacturing', loadComponent: () => import('./features/manufacturing/manufacturing.component').then(m => m.ManufacturingComponent) },
      { path: 'reports', loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent) },
      { path: 'users', loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent) },
      { path: 'mobile', loadComponent: () => import('./features/mobile/mobile.component').then(m => m.MobileComponent) },
      { path: 'pos', loadComponent: () => import('./features/pos/pos.component').then(m => m.PosComponent) },
      { path: 'integrations', loadComponent: () => import('./features/integrations/integrations.component').then(m => m.IntegrationsComponent) },
      { path: 'audit-logs', loadComponent: () => import('./features/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent) },
      { path: 'privacy', loadComponent: () => import('./features/legal/privacy.component').then(m => m.PrivacyComponent) },
      { path: 'terms', loadComponent: () => import('./features/legal/terms.component').then(m => m.TermsComponent) },
      { path: 'gst-disclaimer', loadComponent: () => import('./features/legal/gst-disclaimer.component').then(m => m.GstDisclaimerComponent) },
      { path: 'onboarding', loadComponent: () => import('./features/onboarding/onboarding.component').then(m => m.OnboardingComponent) }
    ]
  },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
