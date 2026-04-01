import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TransactionBootstrap, TransactionDataService } from '../../core/services/transaction-data.service';

@Component({
  selector: 'app-manufacturing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manufacturing.component.html',
  styleUrls: ['./manufacturing.component.css']
})
export class ManufacturingComponent implements OnInit {
  data: TransactionBootstrap | null = null;
  advanced: any = null;
  loading = true;
  error = '';
  validationMessage = '';
  successMessage = '';

  runForm = {
    finishedProductId: 0,
    outputBinId: 0,
    outputQuantity: 1,
    referenceNo: '',
    current: { productId: 0, binId: 0, quantity: 1 },
    components: [] as any[]
  };

  bomForm = {
    editingBomId: 0,
    name: '',
    bomCode: '',
    finishedProductId: 0,
    standardOutputQty: 1,
    current: { componentProductId: 0, quantityPerOutput: 0 },
    items: [] as any[]
  };

  workOrderForm = {
    bomTemplateId: 0,
    plannedDate: '',
    plannedOutputQty: 1,
    inputBinId: 0,
    outputBinId: 0
  };

  constructor(private transactions: TransactionDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.transactions.getBootstrap().subscribe({
      next: (data) => {
        this.data = data;
        this.loadAdvanced();
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error ?? 'Unable to load manufacturing data.';
        this.loading = false;
      }
    });
  }

  loadAdvanced(): void {
    this.transactions.getAdvancedSnapshot().subscribe({
      next: (data) => this.advanced = data,
      error: () => {}
    });
  }

  addComponent(): void {
    const c = this.runForm.current;
    this.validationMessage = '';
    if (!c.productId || !c.binId || c.quantity <= 0) {
      this.validationMessage = 'Select component product, bin and quantity.';
      return;
    }
    const product = this.data?.products.find(x => x.id === +c.productId);
    const bin = this.data?.bins.find(x => x.id === +c.binId);
    this.runForm.components.push({
      productId: +c.productId,
      binId: +c.binId,
      quantity: +c.quantity,
      productName: product?.name,
      binCode: bin?.binCode
    });
    this.runForm.current = { productId: 0, binId: 0, quantity: 1 };
  }

  createRun(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.runForm.finishedProductId || !this.runForm.outputBinId || this.runForm.outputQuantity <= 0 || this.runForm.components.length === 0) {
      this.validationMessage = 'Finished product, output bin and component lines are required.';
      return;
    }
    const payload = {
      finishedProductId: +this.runForm.finishedProductId,
      outputBinId: +this.runForm.outputBinId,
      outputQuantity: +this.runForm.outputQuantity,
      referenceNo: this.runForm.referenceNo,
      components: this.runForm.components.map(x => ({
        productId: +x.productId,
        binId: +x.binId,
        quantity: +x.quantity
      }))
    };

    this.transactions.createProductionRun(payload).subscribe({
      next: () => {
        this.runForm = {
          finishedProductId: 0,
          outputBinId: 0,
          outputQuantity: 1,
          referenceNo: '',
          current: { productId: 0, binId: 0, quantity: 1 },
          components: []
        };
        this.successMessage = 'Production run posted.';
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Production run failed.'
    });
  }

  addBomItem(): void {
    const curr = this.bomForm.current;
    this.validationMessage = '';
    if (!curr.componentProductId || curr.quantityPerOutput <= 0) {
      this.validationMessage = 'Select BOM component and quantity per output.';
      return;
    }
    const product = this.data?.products.find(x => x.id === +curr.componentProductId);
    this.bomForm.items.push({
      componentProductId: +curr.componentProductId,
      componentName: product?.name,
      quantityPerOutput: +curr.quantityPerOutput
    });
    this.bomForm.current = { componentProductId: 0, quantityPerOutput: 0 };
  }

  createBom(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.bomForm.name || !this.bomForm.finishedProductId || this.bomForm.items.length === 0) {
      this.validationMessage = 'BOM name, finished product and item lines are required.';
      return;
    }
    const payload = {
      name: this.bomForm.name,
      bomCode: this.bomForm.bomCode,
      finishedProductId: +this.bomForm.finishedProductId,
      standardOutputQty: +this.bomForm.standardOutputQty,
      isActive: true,
      items: this.bomForm.items.map(x => ({ componentProductId: +x.componentProductId, quantityPerOutput: +x.quantityPerOutput }))
    };
    const save$ = this.bomForm.editingBomId
      ? this.transactions.updateBomTemplate(this.bomForm.editingBomId, payload)
      : this.transactions.createBomTemplate(payload);
    const wasEdit = !!this.bomForm.editingBomId;

    save$.subscribe({
      next: () => {
        this.bomForm = { editingBomId: 0, name: '', bomCode: '', finishedProductId: 0, standardOutputQty: 1, current: { componentProductId: 0, quantityPerOutput: 0 }, items: [] };
        this.successMessage = wasEdit ? 'BOM updated.' : 'BOM created.';
        this.loadAdvanced();
      },
      error: (err) => this.error = err?.error ?? (this.bomForm.editingBomId ? 'BOM update failed.' : 'BOM creation failed.')
    });
  }

  editBom(bom: any): void {
    this.bomForm.editingBomId = +bom.id;
    this.bomForm.name = bom.name ?? '';
    this.bomForm.bomCode = bom.bomCode ?? '';
    this.bomForm.finishedProductId = +bom.finishedProductId;
    this.bomForm.standardOutputQty = +(bom.standardOutputQty ?? 1);
    this.bomForm.items = (bom.items ?? []).map((x: any) => ({
      componentProductId: +x.componentProductId,
      componentName: this.data?.products.find(p => p.id === x.componentProductId)?.name,
      quantityPerOutput: +x.quantityPerOutput
    }));
  }

  deactivateBom(bom: any): void {
    if (!confirm('Soft delete this BOM template?')) return;
    this.transactions.updateBomTemplate(+bom.id, {
      name: bom.name,
      standardOutputQty: +bom.standardOutputQty,
      isActive: false,
      items: (bom.items ?? []).map((x: any) => ({ componentProductId: +x.componentProductId, quantityPerOutput: +x.quantityPerOutput }))
    }).subscribe({
      next: () => {
        this.successMessage = 'BOM soft deleted.';
        this.loadAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'BOM soft delete failed.'
    });
  }

  createWorkOrder(): void {
    this.validationMessage = '';
    this.successMessage = '';
    if (!this.workOrderForm.bomTemplateId || !this.workOrderForm.inputBinId || !this.workOrderForm.outputBinId) {
      this.validationMessage = 'BOM template, input bin and output bin are required.';
      return;
    }
    this.transactions.createWorkOrder({
      bomTemplateId: +this.workOrderForm.bomTemplateId,
      plannedDate: this.workOrderForm.plannedDate || new Date().toISOString(),
      plannedOutputQty: +this.workOrderForm.plannedOutputQty,
      inputBinId: +this.workOrderForm.inputBinId,
      outputBinId: +this.workOrderForm.outputBinId
    }).subscribe({
      next: () => {
        this.workOrderForm = { bomTemplateId: 0, plannedDate: '', plannedOutputQty: 1, inputBinId: 0, outputBinId: 0 };
        this.successMessage = 'Work order created.';
        this.loadAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Work order creation failed.'
    });
  }

  releaseWorkOrder(workOrderId: number): void {
    if (!confirm('Release this work order?')) return;
    this.transactions.releaseWorkOrder(workOrderId).subscribe({
      next: () => {
        this.successMessage = 'Work order released.';
        this.load();
      },
      error: (err) => this.error = err?.error ?? 'Work order release failed.'
    });
  }

  cancelWorkOrder(workOrderId: number): void {
    if (!confirm('Cancel this work order?')) return;
    this.transactions.cancelWorkOrder(workOrderId).subscribe({
      next: () => {
        this.successMessage = 'Work order cancelled.';
        this.loadAdvanced();
      },
      error: (err) => this.error = err?.error ?? 'Work order cancel failed.'
    });
  }
}
