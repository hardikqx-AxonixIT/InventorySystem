import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = localStorage.getItem('token');
    const authReq = token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = this.resolveErrorMessage(error);

        if (error.status === 401) {
          localStorage.removeItem('token');
          this.router.navigate(['/login']);
          this.showToast('Session expired. Please login again.', 'error');
        } else if (error.status === 403) {
          this.showToast(message || 'You do not have permission to perform this action.', 'error');
        } else if (error.status === 0) {
          this.showToast('Cannot connect to server. Please check network.', 'error');
        } else {
          this.showToast(message || 'Request failed. Please verify data and retry.', 'error');
        }

        return throwError(() => error);
      })
    );
  }

  private resolveErrorMessage(error: HttpErrorResponse): string {
    const payload = error?.error;
    if (!payload) return '';
    if (typeof payload === 'string') return payload;
    if (typeof payload?.message === 'string') return payload.message;
    if (typeof payload?.title === 'string') return payload.title;

    if (Array.isArray(payload)) {
      return payload.map((x: any) => x?.description || x?.message || String(x)).join(', ');
    }

    if (payload?.errors && typeof payload.errors === 'object') {
      const values = Object.values(payload.errors) as any[];
      return values.flat().map(x => String(x)).join(', ');
    }

    return '';
  }

  private showToast(message: string, type: 'error' | 'success' | 'info' = 'info'): void {
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
      border-radius: 8px;
      font-family: "DM Sans", sans-serif;
      font-size: 14px;
      font-weight: 500;
      color: #fff;
      background: ${type === 'error' ? '#e53e3e' : type === 'success' ? '#38a169' : '#3182ce'};
      box-shadow: 0 4px 24px rgba(0,0,0,0.2);
      animation: slideIn 0.3s ease;
      max-width: 420px;
    `;

    const style = document.createElement('style');
    style.textContent = '@keyframes slideIn { from { transform: translateY(20px); opacity:0; } to { transform: translateY(0); opacity:1; } }';
    document.head.appendChild(style);
    document.body.appendChild(toast);

    setTimeout(() => toast.remove(), 4500);
  }
}
