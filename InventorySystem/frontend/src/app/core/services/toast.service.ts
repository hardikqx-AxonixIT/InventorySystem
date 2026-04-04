import { Injectable } from '@angular/core';

export type ToastType = 'error' | 'success' | 'info' | 'warning';

@Injectable({ providedIn: 'root' })
export class ToastService {
  show(message: string, type: ToastType = 'info'): void {
    const existing = document.getElementById('global-toast');
    if (existing) existing.remove();

    const toast = document.createElement('div');
    toast.id = 'global-toast';
    toast.textContent = message;
    toast.style.cssText = `
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 99999;
      padding: 14px 20px;
      border-radius: 10px;
      font-family: "Outfit", sans-serif;
      font-size: 14px;
      font-weight: 600;
      color: #fff;
      background: ${this.colorForType(type)};
      box-shadow: 0 8px 26px rgba(0,0,0,0.28);
      animation: slideInToast 0.24s ease;
      max-width: 440px;
    `;

    if (!document.getElementById('global-toast-style')) {
      const style = document.createElement('style');
      style.id = 'global-toast-style';
      style.textContent = '@keyframes slideInToast { from { transform: translateY(16px); opacity:0; } to { transform: translateY(0); opacity:1; } }';
      document.head.appendChild(style);
    }

    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 4200);
  }

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error');
  }

  warning(message: string): void {
    this.show(message, 'warning');
  }

  info(message: string): void {
    this.show(message, 'info');
  }

  private colorForType(type: ToastType): string {
    if (type === 'error') return '#dc2626';
    if (type === 'success') return '#16a34a';
    if (type === 'warning') return '#d97706';
    return '#2563eb';
  }
}

