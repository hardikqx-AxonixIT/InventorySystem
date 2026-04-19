import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserManagementService } from '../../core/services/user-management.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {
  loading = true;
  error = '';
  users: any[] = [];
  totalUsers = 0;
  page = 1;
  pageSize = 25;
  search = '';
  roles: string[] = [];
  permissions: any[] = [];
  editingUserId = '';
  editForm = {
    email: '',
    fullName: '',
    role: ''
  };

  form = {
    email: '',
    password: 'Admin@123',
    fullName: '',
    role: ''
  };

  permissionForm = {
    roleName: '',
    moduleKey: 'sales',
    canView: true,
    canCreate: false,
    canUpdate: false,
    canDelete: false,
    canApprove: false,
    canExport: false
  };

  constructor(private usersApi: UserManagementService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.usersApi.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
        if (!this.form.role && roles.length) this.form.role = roles[0];
      },
      error: () => {}
    });

    this.usersApi.getUsers(this.page, this.pageSize, this.search).subscribe({
      next: (res) => {
        this.users = res?.records ?? [];
        this.totalUsers = +(res?.total ?? 0);
        this.loadAdvanced();
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load users.';
        this.loading = false;
      }
    });
  }

  loadAdvanced(): void {
    this.usersApi.getPermissions().subscribe({
      next: (rows) => this.permissions = rows ?? [],
      error: () => {}
    });
  }

  createUser(): void {
    if (!this.form.email || !this.form.password) return;
    this.usersApi.createUser(this.form).subscribe({
      next: () => {
        this.form = { email: '', password: 'Admin@123', fullName: '', role: this.roles[0] ?? '' };
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'User creation failed.'
    });
  }

  savePermission(): void {
    if (!this.permissionForm.roleName || !this.permissionForm.moduleKey) return;
    this.usersApi.savePermission(this.permissionForm).subscribe({
      next: () => this.loadAdvanced(),
      error: (err) => this.error = err?.error ?? 'Permission update failed.'
    });
  }

  beginEdit(user: any): void {
    this.editingUserId = user.id;
    this.editForm = {
      email: user.email ?? '',
      fullName: user.fullName ?? '',
      role: (user.roles ?? [])[0] ?? (this.roles[0] ?? '')
    };
  }

  cancelEdit(): void {
    this.editingUserId = '';
    this.editForm = { email: '', fullName: '', role: '' };
  }

  saveEdit(user: any): void {
    if (!user?.id || !this.editForm.email) return;
    this.usersApi.updateUser(user.id, this.editForm).subscribe({
      next: () => {
        this.cancelEdit();
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'User update failed.'
    });
  }

  toggleActive(user: any): void {
    if (!user?.id) return;
    const action$ = user.isActive ? this.usersApi.deactivateUser(user.id) : this.usersApi.activateUser(user.id);
    action$.subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error ?? 'Status update failed.'
    });
  }

  searchUsers(): void {
    this.page = 1;
    this.load();
  }

  nextPage(): void {
    if ((this.page * this.pageSize) >= this.totalUsers) return;
    this.page += 1;
    this.load();
  }

  prevPage(): void {
    if (this.page <= 1) return;
    this.page -= 1;
    this.load();
  }
}
