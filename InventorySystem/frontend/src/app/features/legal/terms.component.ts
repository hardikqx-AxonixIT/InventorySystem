import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-terms',
  standalone: true,
  imports: [CommonModule],
  template: `
  <section class="legal-page">
    <h1>Terms of use</h1>
    <p>Axonix Inventory Suite is provided as business software. You are responsible for the accuracy of masters, tax rates, and statutory filings prepared from this system.</p>
    <p>Provider liability is limited to the fees paid for the software in the prior billing period, except where prohibited by law.</p>
  </section>`,
  styles: [`.legal-page { max-width: 720px; margin: 0 auto; padding: 1.5rem; line-height: 1.6; }`]
})
export class TermsComponent {}
