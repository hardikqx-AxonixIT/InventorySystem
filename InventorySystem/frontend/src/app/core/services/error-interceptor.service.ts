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
import { ToastService } from './toast.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router, private toast: ToastService) {}

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
          this.toast.error('Session expired. Please login again.');
        } else if (error.status === 403) {
          this.toast.error(message || 'You do not have permission to perform this action.');
        } else if (error.status === 0) {
          this.toast.error('Cannot connect to server. Please check network.');
        } else {
          this.toast.error(message || 'Request failed. Please verify data and retry.');
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
}
