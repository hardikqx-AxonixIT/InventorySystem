import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { InventoryListComponent } from './features/inventory/inventory-list/inventory-list.component';
import { InventoryAdjustmentsComponent } from './features/inventory/adjustments/inventory-adjustments.component';
import { MasterDataComponent } from './features/masters/master-data.component';
import { PurchaseComponent } from './features/purchase/purchase.component';
import { SalesComponent } from './features/sales/sales.component';
import { GstComponent } from './features/gst/gst.component';
import { AccountingComponent } from './features/accounting/accounting.component';
import { WarehouseComponent } from './features/warehouse/warehouse.component';
import { ManufacturingComponent } from './features/manufacturing/manufacturing.component';
import { ReportsComponent } from './features/reports/reports.component';
import { UsersComponent } from './features/users/users.component';
import { AuditLogComponent } from './features/users/audit-log.component';
import { MobileComponent } from './features/mobile/mobile.component';
import { PosComponent } from './features/pos/pos.component';
import { IntegrationsComponent } from './features/integrations/integrations.component';
import { AuditLogsComponent } from './features/audit-logs/audit-logs.component';
import { StockHistoryComponent } from './features/inventory/stock-history/stock-history.component';
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
      { path: 'dashboard', component: DashboardComponent },
      { path: 'inventory', component: InventoryListComponent },
      { path: 'inventory/adjustments', component: InventoryAdjustmentsComponent },
      { path: 'inventory/history/:id', component: StockHistoryComponent },
      { path: 'masters', component: MasterDataComponent },
      { path: 'purchase', component: PurchaseComponent },
      { path: 'sales', component: SalesComponent },
      { path: 'gst', component: GstComponent },
      { path: 'accounting', component: AccountingComponent },
      { path: 'warehouse', component: WarehouseComponent },
      { path: 'manufacturing', component: ManufacturingComponent },
      { path: 'reports', component: ReportsComponent },
      { path: 'users', component: UsersComponent },
      { path: 'audit-log', component: AuditLogComponent },
      { path: 'mobile', component: MobileComponent },
      { path: 'pos', component: PosComponent },
      { path: 'integrations', component: IntegrationsComponent },
      { path: 'audit-logs', component: AuditLogsComponent }
    ]
  },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
