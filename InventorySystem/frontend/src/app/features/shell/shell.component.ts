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
  sidebarCollapsed = false;

  readonly quickLinks = [
    { label: 'Dashboard', route: '/dashboard', icon: '&#128200;' },
    { label: 'Sales', route: '/sales', icon: '&#128722;' },
    { label: 'Purchase', route: '/purchase', icon: '&#128717;' },
    { label: 'POS Billing', route: '/pos', icon: '&#128179;' }
  ];

  readonly moduleLinks = ERP_MODULES;

  constructor(private authService: AuthService, private router: Router, public i18n: I18nService) {
    this.i18n.init();
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  toggleSidebarCollapse(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
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
      masters: '&#128203;',
      inventory: '&#128230;',
      purchase: '&#128717;',
      sales: '&#128722;',
      gst: '&#128196;',
      accounting: '&#128178;',
      warehouse: '&#127970;',
      manufacturing: '&#127981;',
      reports: '&#128202;',
      users: '&#128101;',
      mobile: '&#128241;',
      integrations: '&#128279;',
      'audit-logs': '&#128221;'
    };
    return icons[key] ?? '&#128193;';
  }
}
