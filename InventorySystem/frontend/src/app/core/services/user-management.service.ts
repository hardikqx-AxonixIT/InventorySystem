import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private readonly apiUrl = 'http://localhost:5157/api/UserManagement';
  private readonly advancedUrl = 'http://localhost:5157/api/AdvancedOperations';

  constructor(private http: HttpClient) {}

  getRoles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/roles`);
  }

  getUsers(page = 1, pageSize = 25, search = ''): Observable<any> {
    const q = `?page=${page}&pageSize=${pageSize}&search=${encodeURIComponent(search)}`;
    return this.http.get<any>(`${this.apiUrl}/users${q}`);
  }

  createUser(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/users`, payload);
  }

  getPermissions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.advancedUrl}/users/permissions`);
  }

  savePermission(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/users/permissions`, payload);
  }

  getAuditLogs(payload: any): Observable<any> {
    return this.http.post(`${this.advancedUrl}/users/audit-logs`, payload);
  }
}
