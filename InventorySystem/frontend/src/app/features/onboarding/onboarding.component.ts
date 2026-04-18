import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
  <section class="onboarding">
    <h1>Welcome</h1>
    <ol class="steps">
      <li>Complete <a routerLink="/masters">master data</a> (categories, units, warehouses, products).</li>
      <li>Add <a routerLink="/purchase">vendors</a> and <a routerLink="/sales">customers</a> with correct GST details.</li>
      <li>Review <a routerLink="/gst-disclaimer">GST disclaimer</a> before enabling integrations.</li>
      <li>Go to <a routerLink="/dashboard">dashboard</a> for daily KPIs.</li>
    </ol>
  </section>`,
  styles: [`
    .onboarding { max-width: 640px; margin: 0 auto; padding: 1.5rem; line-height: 1.6; }
    .steps { padding-left: 1.25rem; }
    .steps li { margin-bottom: 0.5rem; }
  `]
})
export class OnboardingComponent {}
