export interface ErpModuleRecord {
  label: string;
  value: string;
}

export interface ErpModuleDefinition {
  key: string;
  title: string;
  subtitle: string;
  status: string;
  summary: string;
  metrics: ErpModuleRecord[];
  workflows: string[];
  sampleRecords: ErpModuleRecord[];
}

export const ERP_MODULES: ErpModuleDefinition[] = [
  {
    key: 'masters',
    title: 'Master Module',
    subtitle: 'Foundation data for products, tax, parties, and warehouse structure.',
    status: 'Active',
    summary: 'Manage item catalog, categories, brands, units, warehouses, customers, and vendors before transactional modules go live.',
    metrics: [
      { label: 'Entities', value: 'Items, units, warehouses, customers, vendors' },
      { label: 'India fields', value: 'GSTIN, HSN, barcode, credit limit' },
      { label: 'Goal', value: 'Clean base data for every transaction' }
    ],
    workflows: [
      'Create SKU, barcode, HSN, brand, and unit mappings.',
      'Maintain customer/vendor GST and payment terms.',
      'Define warehouses and future rack/bin structure.'
    ],
    sampleRecords: [
      { label: 'Sample customer', value: 'Shree Medical Stores' },
      { label: 'Sample vendor', value: 'Sunrise Pharma Distributors' },
      { label: 'Primary warehouse', value: 'Main Warehouse' }
    ]
  },
  {
    key: 'purchase',
    title: 'Purchase Module',
    subtitle: 'Procure stock from suppliers with GRN-driven inventory updates.',
    status: 'Active',
    summary: 'Covers purchase order, goods receipt note, purchase invoice, and supplier payments with stock impact.',
    metrics: [
      { label: 'Flow', value: 'PO -> GRN -> Invoice -> Payment' },
      { label: 'Stock impact', value: 'Auto update on receipt' },
      { label: 'Finance link', value: 'Supplier payable posting' }
    ],
    workflows: [
      'Raise and approve purchase orders.',
      'Receive partial or full GRN against PO.',
      'Book supplier invoice and due date.'
    ],
    sampleRecords: [
      { label: 'PO number', value: 'PO-2026-0012' },
      { label: 'Supplier', value: 'Prime Electronics Supply' },
      { label: 'Expected receipt', value: '22 Mar 2026' }
    ]
  },
  {
    key: 'sales',
    title: 'Sales Module',
    subtitle: 'Quotation to invoice workflow for trading and distribution teams.',
    status: 'Active',
    summary: 'Handles quotation, sales order, delivery challan, GST invoice, payment follow-up, and returns.',
    metrics: [
      { label: 'Flow', value: 'Quotation -> Order -> Delivery -> Invoice' },
      { label: 'Dispatch mode', value: 'Pick, pack, dispatch ready' },
      { label: 'Retail mode', value: 'Quick invoice and POS ready' }
    ],
    workflows: [
      'Prepare quotation with tax and pricing.',
      'Convert approved quote into a sales order.',
      'Generate delivery challan and invoice.',
      'Use quick invoice mode to reduce sales flow steps for counter billing.'
    ],
    sampleRecords: [
      { label: 'Quotation', value: 'QT-2026-0044' },
      { label: 'Customer', value: 'Metro Retail LLP' },
      { label: 'Invoice draft', value: 'INV-2026-0091' }
    ]
  },
  {
    key: 'gst',
    title: 'GST & Billing',
    subtitle: 'India-specific tax, invoice, and compliance features.',
    status: 'Active',
    summary: 'Supports CGST, SGST, IGST, HSN/SAC, direct GSP filing readiness, NIC e-invoice flow, e-way bill generation, and return-oriented reporting.',
    metrics: [
      { label: 'Tax logic', value: 'CGST / SGST / IGST' },
      { label: 'Compliance', value: 'HSN and GST reports' },
      { label: 'Target', value: 'Invoice-ready for Indian businesses' }
    ],
    workflows: [
      'Apply intra-state or inter-state GST automatically.',
      'Print GST-compliant sales invoices.',
      'Prepare data for GSTR-1 and GSTR-3B.',
      'Use direct GST filing and NIC integration mode when provider credentials are configured.'
    ],
    sampleRecords: [
      { label: 'HSN sample', value: '3004 / 8504' },
      { label: 'Tax slab', value: '12% and 18%' },
      { label: 'Place of supply', value: 'Gujarat' }
    ]
  },
  {
    key: 'accounting',
    title: 'Accounting Module',
    subtitle: 'Ledgers, journals, receivables, payables, and final statements.',
    status: 'Active',
    summary: 'The accounting layer will automatically reflect purchase, sales, stock adjustments, receipts, and payments.',
    metrics: [
      { label: 'Books', value: 'Cash, bank, ledger, journal' },
      { label: 'Statements', value: 'P&L and balance sheet' },
      { label: 'Integration', value: 'Inventory and billing linked' }
    ],
    workflows: [
      'Post sales and purchase vouchers automatically.',
      'Track receivable and payable ageing.',
      'Run customer outstanding aging with overdue interest computation.',
      'Trigger WhatsApp payment reminders and export ledger summaries to Tally.'
    ],
    sampleRecords: [
      { label: 'Receivable ageing', value: 'INR 2.45L pending' },
      { label: 'Payable ageing', value: 'INR 1.32L pending' },
      { label: 'Bank account', value: 'HDFC Current Account' }
    ]
  },
  {
    key: 'warehouse',
    title: 'Warehouse Module',
    subtitle: 'Operational control for bins, transfers, picks, packs, and dispatch.',
    status: 'Active',
    summary: 'Focused on multi-location stock control, barcode handling, rack/bin tracking, and warehouse transfers.',
    metrics: [
      { label: 'Coverage', value: 'Multi-warehouse and bin-aware' },
      { label: 'Execution', value: 'Pick / pack / dispatch' },
      { label: 'Tools', value: 'Barcode scanning ready' }
    ],
    workflows: [
      'Map stock to zone, aisle, shelf, and bin.',
      'Transfer stock between warehouses.',
      'Track dispatch preparation and execution.'
    ],
    sampleRecords: [
      { label: 'Bin code', value: 'A-01-T' },
      { label: 'Transfer no.', value: 'TRF-2026-0007' },
      { label: 'Dispatch hub', value: 'Retail Dispatch Hub' }
    ]
  },
  {
    key: 'manufacturing',
    title: 'Manufacturing Module',
    subtitle: 'Optional but high-value extension for factories and assemblers.',
    status: 'Active',
    summary: 'Adds BOM, production planning, raw material issue, WIP tracking, and finished goods receipts.',
    metrics: [
      { label: 'Core', value: 'BOM and production order' },
      { label: 'Stock link', value: 'Raw material consumption' },
      { label: 'Fit', value: 'Factories and assembly units' }
    ],
    workflows: [
      'Define BOM for each finished item.',
      'Issue raw materials to production.',
      'Receive finished goods back into stock.'
    ],
    sampleRecords: [
      { label: 'BOM code', value: 'BOM-USB65W-01' },
      { label: 'Production order', value: 'MO-2026-0003' },
      { label: 'FG output', value: '120 units planned' }
    ]
  },
  {
    key: 'reports',
    title: 'Reports & Analytics',
    subtitle: 'Operational and management reporting for faster decisions.',
    status: 'Active',
    summary: 'Designed for low stock alerts, sales/purchase insights, ageing, valuation, profitability, and forecasting.',
    metrics: [
      { label: 'Reports', value: 'Stock, sales, purchase, valuation' },
      { label: 'Alerts', value: 'Low stock, fast/slow, dead stock' },
      { label: 'Future', value: 'Demand forecast and AI reorder' }
    ],
    workflows: [
      'Monitor low stock and reorder candidates.',
      'Review fast-moving vs slow-moving products.',
      'Track profit by item, profit by customer, and inventory value.'
    ],
    sampleRecords: [
      { label: 'Low stock alert', value: '3 items below threshold' },
      { label: 'Top seller', value: 'Paracetamol 500mg' },
      { label: 'Forecast horizon', value: '30 days' }
    ]
  },
  {
    key: 'users',
    title: 'User & Role Management',
    subtitle: 'Permissions, roles, and audit trail across the ERP.',
    status: 'Active',
    summary: 'Supports admins, managers, warehouse staff, finance users, and approval policies with full audit logs.',
    metrics: [
      { label: 'Roles', value: 'Admin, manager, staff' },
      { label: 'Security', value: 'JWT auth and policies' },
      { label: 'Traceability', value: 'Audit log enabled' }
    ],
    workflows: [
      'Create users and assign role bundles.',
      'Restrict sensitive inventory actions.',
      'Review audit history for master and stock changes.'
    ],
    sampleRecords: [
      { label: 'Demo admin', value: 'admin@axonix.local' },
      { label: 'Approval policy', value: 'CanApproveAdjustments' },
      { label: 'Audit scope', value: 'Master and transaction logs' }
    ]
  },
  {
    key: 'mobile',
    title: 'Mobile / Owner Dashboard',
    subtitle: 'Quick business visibility for owners and managers on the go.',
    status: 'Active',
    summary: 'Provides today sales, stock alerts, receivables, and payment follow-up in a mobile-first experience.',
    metrics: [
      { label: 'Owner KPIs', value: 'Sales, stock alerts, outstanding' },
      { label: 'Reach', value: 'Mobile-friendly from day one' },
      { label: 'Future', value: 'Native app and offline support' }
    ],
    workflows: [
      'Open a daily business summary on mobile.',
      'Review high-priority alerts quickly.',
      'Approve operational exceptions remotely.'
    ],
    sampleRecords: [
      { label: 'Today sales', value: 'INR 84,250' },
      { label: 'Outstanding', value: 'INR 2.45L' },
      { label: 'Alerts', value: '4 urgent actions' }
    ]
  },
  {
    key: 'integrations',
    title: 'Integrations',
    subtitle: 'Provider APIs for communication and payments.',
    status: 'Active',
    summary: 'Centralized integration workspace for Tally sync, WhatsApp invoice delivery, Razorpay/UPI lifecycle, backup safety controls, and SaaS monetization setup.',
    metrics: [
      { label: 'Channels', value: 'Tally, WhatsApp, Razorpay' },
      { label: 'Security', value: 'Callback signature verification' },
      { label: 'Use case', value: 'Collections, sync, and monetization' }
    ],
    workflows: [
      'Export sales and ledger data to Tally XML.',
      'Import masters and sync ledger/vouchers with Tally connector.',
      'Send invoice to customer on WhatsApp.',
      'Create payment orders with Razorpay API.',
      'Verify payment callback signatures.',
      'Configure subscription plans, trial period, and license keys.'
    ],
    sampleRecords: [
      { label: 'WhatsApp flow', value: 'Invoice -> send confirmation' },
      { label: 'Payment mode', value: 'UPI / card / netbanking' },
      { label: 'Callback state', value: 'Verified / Failed signature' }
    ]
  },
  {
    key: 'audit-logs',
    title: 'Audit Trail',
    subtitle: 'Dedicated log explorer for compliance and traceability.',
    status: 'Active',
    summary: 'Separate module to inspect entity-level create/update/delete changes with filters and paging.',
    metrics: [
      { label: 'Coverage', value: 'Master and transaction changes' },
      { label: 'Search', value: 'Entity + action filters' },
      { label: 'Scale', value: 'Paginated for large volume' }
    ],
    workflows: [
      'Filter logs by entity and action.',
      'Review who changed what and when.',
      'Use pagination for high-volume records.'
    ],
    sampleRecords: [
      { label: 'Action', value: 'Modified' },
      { label: 'Entity', value: 'SalesOrder' },
      { label: 'User', value: 'System / application user' }
    ]
  }
];

export function getModuleDefinition(key: string): ErpModuleDefinition | undefined {
  return ERP_MODULES.find(module => module.key === key);
}
