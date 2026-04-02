import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { ERP_MODULES } from '../../core/erp-modules';
import { AuthService } from '../../core/services/auth.service';
import { AppLang, I18nService } from '../../core/services/i18n.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.css']
})
export class ShellComponent {
  menuOpen = false;

  readonly quickLinks = [
    { label: 'Dashboard', route: '/dashboard', icon: 'DS' },
    { label: 'Sales', route: '/sales', icon: 'SL' },
    { label: 'Purchase', route: '/purchase', icon: 'PU' },
    { label: 'POS Billing', route: '/pos', icon: 'PO' }
  ];

  readonly moduleLinks = ERP_MODULES;

  constructor(private authService: AuthService, private router: Router, public i18n: I18nService) {
    this.i18n.init();
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  closeMenuOnNavigate(): void {
    this.menuOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  setLang(lang: AppLang): void {
    this.i18n.setLang(lang);
  }

  navLabel(label: string): string {
    const key = label.toLowerCase();
    if (key === 'pos billing') return 'POS Billing';
    return this.i18n.t(key) || label;
  }

  moduleLabel(key: string, fallback: string): string {
    return this.i18n.t(key) || fallback;
  }

  moduleIcon(key: string): string {
    const icons: Record<string, string> = {
      masters: 'MS',
      inventory: 'IV',
      purchase: 'PU',
      sales: 'SL',
      gst: 'TX',
      accounting: 'AC',
      warehouse: 'WH',
      manufacturing: 'MF',
      reports: 'RP',
      users: 'US',
      mobile: 'MB',
      integrations: 'IN',
      'audit-logs': '📜'
    };
    return icons[key] ?? 'MD';
  }
}
