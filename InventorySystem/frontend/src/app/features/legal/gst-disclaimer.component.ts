import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-gst-disclaimer',
  standalone: true,
  imports: [CommonModule],
  template: `
  <section class="legal-page">
    <h1>GST and e-invoice disclaimer</h1>
    <p>GSTR exports, e-invoice IRN generation, and e-way bill features depend on correct master data, valid GSTIN, and configured integration endpoints. Validate all filings on the government portal before submission.</p>
    <p>Direct GSP filing requires a certified provider and active credentials. This software does not replace professional tax advice.</p>
  </section>`,
  styles: [`.legal-page { max-width: 720px; margin: 0 auto; padding: 1.5rem; line-height: 1.6; }`]
})
export class GstDisclaimerComponent {}
