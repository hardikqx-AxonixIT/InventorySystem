import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserManagementService } from '../../core/services/user-management.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.css']
})
export class AuditLogsComponent implements OnInit {
  loading = true;
  error = '';
  logs: any[] = [];
  total = 0;

  filter = {
    entityName: '',
    action: '',
    page: 1,
    pageSize: 50
  };

  constructor(private usersApi: UserManagementService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.usersApi.getAuditLogs(this.filter).subscribe({
      next: (res) => {
        this.logs = res?.records ?? [];
        this.total = +(res?.total ?? 0);
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load audit logs.';
        this.loading = false;
      }
    });
  }

  search(): void {
    this.filter.page = 1;
    this.load();
  }

  nextPage(): void {
    if ((this.filter.page * this.filter.pageSize) >= this.total) return;
    this.filter.page += 1;
    this.load();
  }

  prevPage(): void {
    if (this.filter.page <= 1) return;
    this.filter.page -= 1;
    this.load();
  }
}
