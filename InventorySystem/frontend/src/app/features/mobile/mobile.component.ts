import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-mobile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './mobile.component.html',
  styleUrls: ['./mobile.component.css']
})
export class MobileComponent implements OnInit {
  loading = true;
  error = '';
  data: any = null;

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.transactions.getMobileDashboard().subscribe({
      next: (data) => {
        this.data = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load mobile dashboard.';
        this.loading = false;
      }
    });
  }
}
