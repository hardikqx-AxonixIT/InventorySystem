import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [CommonModule],
  template: `
  <section class="legal-page">
    <h1>Privacy policy</h1>
    <p>This application processes business and inventory data you enter. Use only on secure networks; restrict access by role. Do not store payment card data in free-text fields.</p>
    <p>Contact your administrator for data export, correction, or deletion requests.</p>
  </section>`,
  styles: [`.legal-page { max-width: 720px; margin: 0 auto; padding: 1.5rem; line-height: 1.6; }`]
})
export class PrivacyComponent {}
