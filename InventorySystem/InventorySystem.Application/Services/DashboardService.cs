using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IApplicationDbContext _context;

        public DashboardService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
        {
            var totalProducts = await _context.Products.CountAsync(cancellationToken);
            var activeWarehouses = await _context.Warehouses.CountAsync(w => w.IsActive, cancellationToken);
            var pendingAdjustments = await _context.AdjustmentRequests.CountAsync(a => a.Status == AdjustmentStatus.Pending, cancellationToken);
            var lowStockItems = await _context.Products
                .CountAsync(p => _context.StockLevels
                    .Where(s => s.ProductId == p.Id)
                    .Sum(s => (decimal?)s.QuantityOnHand) <= p.ReorderLevel, cancellationToken);

            var inventoryValueEstimate = await _context.StockLevels
                .Join(_context.Products,
                    stock => stock.ProductId,
                    product => product.Id,
                    (stock, product) => stock.QuantityOnHand * product.PurchasePrice)
                .SumAsync(cancellationToken);

            var today = System.DateTime.UtcNow.Date;
            
            var totalReceivables = await _context.SalesInvoices
                .Where(x => x.Status != DocumentStatus.Cancelled && x.BalanceAmount > 0)
                .SumAsync(x => x.BalanceAmount, cancellationToken);
                
            var totalPayables = await _context.PurchaseInvoices
                .Where(x => x.Status != DocumentStatus.Cancelled && x.BalanceAmount > 0)
                .SumAsync(x => x.BalanceAmount, cancellationToken);
                
            var todaySales = await _context.SalesInvoices
                .Where(x => x.Status != DocumentStatus.Cancelled && x.InvoiceDate.Date == today)
                .SumAsync(x => x.GrandTotal, cancellationToken);

            var cashBalance = 150000m; // Mocked Cash Balance for MVP Dashboard

            return new DashboardOverviewDto
            {
                Kpis = new DashboardKpiDto
                {
                    TotalProducts = totalProducts,
                    ActiveWarehouses = activeWarehouses,
                    PendingAdjustments = pendingAdjustments,
                    LowStockItems = lowStockItems,
                    InventoryValueEstimate = inventoryValueEstimate,
                    TotalReceivables = totalReceivables,
                    TotalPayables = totalPayables,
                    TodaySales = todaySales,
                    CashBalance = cashBalance
                },
                Modules = BuildModules(),
                Integrations = new List<string>
                {
                    "Barcode scanner workflows",
                    "WhatsApp invoice sharing",
                    "Razorpay payment collection",
                    "Tally export/import",
                    "Excel import/export"
                },
                NextMilestones = new List<string>
                {
                    "Complete product, category, unit, warehouse, customer, and vendor masters",
                    "Add purchase order and GRN workflows with stock auto-update",
                    "Add quotation, sales order, delivery challan, and GST invoice flow",
                    "Introduce reorder alerts, valuation reports, and owner dashboard cards"
                },
                RemainingGaps = new List<string>
                {
                    "Sales module still needs quotation, delivery challan, and sales return flow.",
                    "GST compliance exports pending: GSTR-1, GSTR-3B, e-invoice, e-way bill.",
                    "Accounting needs full journal, ledger posting engine, P&L and balance sheet statements.",
                    "Warehouse requires scan-based pick/pack workflow and transfer approval matrix.",
                    "Manufacturing needs reusable BOM templates and production planning calendar.",
                    "Users module needs granular permission matrix and audit log explorer.",
                    "Integrations pending: WhatsApp invoice send, Razorpay collection capture, Tally sync."
                },
                Recommendations = new List<string>
                {
                    "Introduce role-based page visibility to reduce clutter for end users.",
                    "Add global search + filters + pagination for all master/transaction grids.",
                    "Add action-confirmation modals for deactivate/activate and posting actions.",
                    "Enable document print/export format for PO, GRN, invoice, and GST summaries.",
                    "Add centralized notification center for low stock, due payments, and failed postings."
                }
            };
        }

        private static List<DashboardModuleDto> BuildModules()
        {
            return new List<DashboardModuleDto>
            {
                new DashboardModuleDto
                {
                    Name = "Master Module",
                    Status = "In Progress",
                    Description = "Foundation records for products, units, categories, warehouses, customers, and vendors.",
                    Features = new List<string> { "SKU, barcode, HSN", "GSTIN and credit control", "Warehouse and location setup" }
                },
                new DashboardModuleDto
                {
                    Name = "Inventory Management",
                    Status = "In Progress",
                    Description = "Core stock tracking with adjustments, ledger, and low-stock visibility.",
                    Features = new List<string> { "Available and reserved stock", "Batch and expiry ready design", "Real-time stock ledger" }
                },
                new DashboardModuleDto
                {
                    Name = "Purchase",
                    Status = "Active",
                    Description = "Purchase ordering, GRN, invoicing, and supplier settlement.",
                    Features = new List<string> { "PO and GRN", "Auto stock update", "Vendor payable tracking" }
                },
                new DashboardModuleDto
                {
                    Name = "Sales + GST Billing",
                    Status = "Active",
                    Description = "Quotation-to-invoice flow with India GST requirements.",
                    Features = new List<string> { "Quotation to invoice", "CGST/SGST/IGST", "E-invoice and e-way bill hooks" }
                },
                new DashboardModuleDto
                {
                    Name = "Reports and Analytics",
                    Status = "Active",
                    Description = "Decision-ready operational and financial insights.",
                    Features = new List<string> { "Low stock and aging", "Sales and purchase analytics", "Profit and forecast reports" }
                }
            };
        }
    }
}
