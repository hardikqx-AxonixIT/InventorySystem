using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InventorySystem.Application.Interfaces;

namespace InventorySystem.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ItemCategory> ItemCategories { get; set; } = null!;
        public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; } = null!;
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<WarehouseBin> WarehouseBins { get; set; } = null!;
        public DbSet<StockLevel> StockLevels => Set<StockLevel>();
        public DbSet<StockLedger> StockLedgers => Set<StockLedger>();
        public DbSet<StockBatchDetail> StockBatchDetails => Set<StockBatchDetail>();
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<AdjustmentRequest> AdjustmentRequests { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Vendor> Vendors { get; set; } = null!;
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } = null!;
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; } = null!;
        public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; } = null!;
        public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; } = null!;
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = null!;
        public DbSet<SupplierPayment> SupplierPayments { get; set; } = null!;
        public DbSet<SalesOrder> SalesOrders { get; set; } = null!;
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; } = null!;
        public DbSet<SalesInvoice> SalesInvoices { get; set; } = null!;
        public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; } = null!;
        public DbSet<SalesQuotation> SalesQuotations { get; set; } = null!;
        public DbSet<SalesQuotationItem> SalesQuotationItems { get; set; } = null!;
        public DbSet<DeliveryChallan> DeliveryChallans { get; set; } = null!;
        public DbSet<DeliveryChallanItem> DeliveryChallanItems { get; set; } = null!;
        public DbSet<SalesReturn> SalesReturns { get; set; } = null!;
        public DbSet<SalesReturnItem> SalesReturnItems { get; set; } = null!;
        public DbSet<AccountLedger> AccountLedgers { get; set; } = null!;
        public DbSet<JournalVoucher> JournalVouchers { get; set; } = null!;
        public DbSet<JournalVoucherLine> JournalVoucherLines { get; set; } = null!;
        public DbSet<WarehouseTransferRequest> WarehouseTransferRequests { get; set; } = null!;
        public DbSet<PickList> PickLists { get; set; } = null!;
        public DbSet<PickListItem> PickListItems { get; set; } = null!;
        public DbSet<BomTemplate> BomTemplates { get; set; } = null!;
        public DbSet<BomTemplateItem> BomTemplateItems { get; set; } = null!;
        public DbSet<ProductionWorkOrder> ProductionWorkOrders { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<GstEInvoiceRecord> GstEInvoiceRecords { get; set; } = null!;
        public DbSet<GstEWayBillRecord> GstEWayBillRecords { get; set; } = null!;
        public DbSet<PaymentGatewayCallbackLog> PaymentGatewayCallbackLogs { get; set; } = null!;
        public DbSet<TenantSubscriptionRecord> TenantSubscriptions { get; set; } = null!;
        public DbSet<CommercialLicenseRecord> CommercialLicenses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Global Query Filters
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

            // Composite Key for StockLevel
            builder.Entity<StockLevel>()
                .HasKey(sl => new { sl.ProductId, sl.BinId });

            // Ensure SKU is unique
            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<ItemCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();

            builder.Entity<UnitOfMeasure>()
                .HasIndex(u => u.Code)
                .IsUnique();

            builder.Entity<Warehouse>()
                .HasIndex(w => w.Code)
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL");

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .HasOne(p => p.UOM)
                .WithMany()
                .HasForeignKey(p => p.UOMId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseBin>()
                .HasOne(b => b.Warehouse)
                .WithMany()
                .HasForeignKey(b => b.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseOrder>()
                .HasIndex(p => p.OrderNumber)
                .IsUnique();

            builder.Entity<GoodsReceiptNote>()
                .HasIndex(g => g.GrnNumber)
                .IsUnique();

            builder.Entity<SalesOrder>()
                .HasIndex(s => s.OrderNumber)
                .IsUnique();

            builder.Entity<SalesInvoice>()
                .HasIndex(s => s.InvoiceNumber)
                .IsUnique();

            builder.Entity<PurchaseInvoice>()
                .HasIndex(p => p.InvoiceNumber)
                .IsUnique();

            builder.Entity<SalesQuotation>()
                .HasIndex(p => p.QuotationNumber)
                .IsUnique();

            builder.Entity<DeliveryChallan>()
                .HasIndex(p => p.ChallanNumber)
                .IsUnique();

            builder.Entity<SalesReturn>()
                .HasIndex(p => p.ReturnNumber)
                .IsUnique();

            builder.Entity<JournalVoucher>()
                .HasIndex(p => p.VoucherNumber)
                .IsUnique();

            builder.Entity<AccountLedger>()
                .HasIndex(p => p.Code)
                .IsUnique();

            builder.Entity<WarehouseTransferRequest>()
                .HasIndex(p => p.RequestNumber)
                .IsUnique();

            builder.Entity<PickList>()
                .HasIndex(p => p.PickNumber)
                .IsUnique();

            builder.Entity<BomTemplate>()
                .HasIndex(p => p.BomCode)
                .IsUnique();

            builder.Entity<ProductionWorkOrder>()
                .HasIndex(p => p.WorkOrderNumber)
                .IsUnique();

            builder.Entity<SupplierPayment>()
                .HasIndex(s => s.PaymentNumber)
                .IsUnique();

            builder.Entity<SalesInvoice>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesInvoice>()
                .HasOne(s => s.Warehouse)
                .WithMany()
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesInvoice>()
                .HasOne(s => s.SalesOrder)
                .WithMany()
                .HasForeignKey(s => s.SalesOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseInvoice>()
                .HasOne(p => p.GoodsReceiptNote)
                .WithMany()
                .HasForeignKey(p => p.GoodsReceiptNoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseInvoice>()
                .HasOne(p => p.PurchaseOrder)
                .WithMany()
                .HasForeignKey(p => p.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseInvoice>()
                .HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesOrderItem>()
                .HasOne(s => s.Bin)
                .WithMany()
                .HasForeignKey(s => s.BinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesInvoiceItem>()
                .HasOne(s => s.Bin)
                .WithMany()
                .HasForeignKey(s => s.BinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GoodsReceiptNoteItem>()
                .HasOne(g => g.PurchaseOrderItem)
                .WithMany()
                .HasForeignKey(g => g.PurchaseOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GoodsReceiptNoteItem>()
                .HasOne(g => g.Bin)
                .WithMany()
                .HasForeignKey(g => g.BinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseInvoiceItem>()
                .HasOne(p => p.GoodsReceiptNoteItem)
                .WithMany()
                .HasForeignKey(p => p.GoodsReceiptNoteItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPayment>()
                .HasOne(s => s.PurchaseInvoice)
                .WithMany()
                .HasForeignKey(s => s.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPayment>()
                .HasOne(s => s.Vendor)
                .WithMany()
                .HasForeignKey(s => s.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesInvoiceItem>()
                .HasOne(s => s.SalesOrderItem)
                .WithMany()
                .HasForeignKey(s => s.SalesOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesQuotation>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesQuotation>()
                .HasOne(s => s.Warehouse)
                .WithMany()
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesQuotationItem>()
                .HasOne(s => s.Bin)
                .WithMany()
                .HasForeignKey(s => s.BinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DeliveryChallan>()
                .HasOne(s => s.SalesOrder)
                .WithMany()
                .HasForeignKey(s => s.SalesOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DeliveryChallanItem>()
                .HasOne(s => s.SalesOrderItem)
                .WithMany()
                .HasForeignKey(s => s.SalesOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DeliveryChallanItem>()
                .HasOne(s => s.Bin)
                .WithMany()
                .HasForeignKey(s => s.BinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesReturn>()
                .HasOne(s => s.SalesInvoice)
                .WithMany()
                .HasForeignKey(s => s.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesReturnItem>()
                .HasOne(s => s.SalesInvoiceItem)
                .WithMany()
                .HasForeignKey(s => s.SalesInvoiceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesReturnItem>()
                .HasOne(s => s.Bin)
                .WithMany()
                .HasForeignKey(s => s.BinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JournalVoucherLine>()
                .HasOne(s => s.Ledger)
                .WithMany()
                .HasForeignKey(s => s.LedgerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseTransferRequest>()
                .HasOne(s => s.FromBin)
                .WithMany()
                .HasForeignKey(s => s.FromBinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseTransferRequest>()
                .HasOne(s => s.ToBin)
                .WithMany()
                .HasForeignKey(s => s.ToBinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PickList>()
                .HasOne(s => s.SalesOrder)
                .WithMany()
                .HasForeignKey(s => s.SalesOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PickListItem>()
                .HasOne(s => s.Bin)
                .WithMany()
                .HasForeignKey(s => s.BinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PickListItem>()
                .HasOne(s => s.SalesOrderItem)
                .WithMany()
                .HasForeignKey(s => s.SalesOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BomTemplate>()
                .HasOne(s => s.FinishedProduct)
                .WithMany()
                .HasForeignKey(s => s.FinishedProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BomTemplateItem>()
                .HasOne(s => s.ComponentProduct)
                .WithMany()
                .HasForeignKey(s => s.ComponentProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionWorkOrder>()
                .HasOne(s => s.BomTemplate)
                .WithMany()
                .HasForeignKey(s => s.BomTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionWorkOrder>()
                .HasOne(s => s.OutputBin)
                .WithMany()
                .HasForeignKey(s => s.OutputBinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionWorkOrder>()
                .HasOne(s => s.InputBin)
                .WithMany()
                .HasForeignKey(s => s.InputBinId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>().Property(p => p.ReorderLevel).HasPrecision(18, 2);
            builder.Entity<Product>().Property(p => p.PurchasePrice).HasPrecision(18, 2);
            builder.Entity<Product>().Property(p => p.SalesPrice).HasPrecision(18, 2);
            builder.Entity<Product>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<Customer>().Property(c => c.CreditLimit).HasPrecision(18, 2);
            builder.Entity<AdjustmentRequest>().Property(a => a.RequestedAmount).HasPrecision(18, 2);
            builder.Entity<StockLevel>().Property(s => s.QuantityOnHand).HasPrecision(18, 2);
            builder.Entity<StockLevel>().Property(s => s.ReservedQuantity).HasPrecision(18, 2);
            builder.Entity<StockLedger>().Property(s => s.ChangeAmount).HasPrecision(18, 2);
            builder.Entity<StockLedger>().Property(s => s.PostChangeBalance).HasPrecision(18, 2);
            builder.Entity<PurchaseOrder>().Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Entity<PurchaseOrder>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrder>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrder>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrder>().Property(p => p.TaxTotal).HasPrecision(18, 2);
            builder.Entity<PurchaseOrder>().Property(p => p.GrandTotal).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.ReceivedQuantity).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.TaxableAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNote>().Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNote>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNote>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNote>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNote>().Property(p => p.TaxTotal).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNote>().Property(p => p.GrandTotal).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.QuantityReceived).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.TaxableAmount).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<GoodsReceiptNoteItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.TaxTotal).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.GrandTotal).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.PaidAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoice>().Property(p => p.BalanceAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.TaxableAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseInvoiceItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
            builder.Entity<SupplierPayment>().Property(p => p.Amount).HasPrecision(18, 2);
            builder.Entity<SalesOrder>().Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Entity<SalesOrder>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesOrder>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesOrder>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesOrder>().Property(p => p.TaxTotal).HasPrecision(18, 2);
            builder.Entity<SalesOrder>().Property(p => p.GrandTotal).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.InvoicedQuantity).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.TaxableAmount).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesOrderItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
            builder.Entity<SalesInvoice>().Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Entity<SalesInvoice>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesInvoice>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesInvoice>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesInvoice>().Property(p => p.TaxTotal).HasPrecision(18, 2);
            builder.Entity<SalesInvoice>().Property(p => p.GrandTotal).HasPrecision(18, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.TaxableAmount).HasPrecision(18, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesInvoiceItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
            builder.Entity<SalesQuotation>().Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Entity<SalesQuotation>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesQuotation>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesQuotation>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesQuotation>().Property(p => p.TaxTotal).HasPrecision(18, 2);
            builder.Entity<SalesQuotation>().Property(p => p.GrandTotal).HasPrecision(18, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.TaxableAmount).HasPrecision(18, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesQuotationItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
            builder.Entity<SalesReturn>().Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Entity<SalesReturn>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesReturn>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesReturn>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesReturn>().Property(p => p.TaxTotal).HasPrecision(18, 2);
            builder.Entity<SalesReturn>().Property(p => p.GrandTotal).HasPrecision(18, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.GstRate).HasPrecision(5, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.TaxableAmount).HasPrecision(18, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.CgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.SgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.IgstAmount).HasPrecision(18, 2);
            builder.Entity<SalesReturnItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
            builder.Entity<DeliveryChallanItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<WarehouseTransferRequest>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<PickListItem>().Property(p => p.Quantity).HasPrecision(18, 2);
            builder.Entity<PickListItem>().Property(p => p.PickedQuantity).HasPrecision(18, 2);
            builder.Entity<BomTemplate>().Property(p => p.StandardOutputQty).HasPrecision(18, 2);
            builder.Entity<BomTemplateItem>().Property(p => p.QuantityPerOutput).HasPrecision(18, 4);
            builder.Entity<ProductionWorkOrder>().Property(p => p.PlannedOutputQty).HasPrecision(18, 2);
            builder.Entity<GstEWayBillRecord>().Property(p => p.DistanceKm).HasPrecision(18, 2);
            builder.Entity<JournalVoucherLine>().Property(p => p.Debit).HasPrecision(18, 2);
            builder.Entity<JournalVoucherLine>().Property(p => p.Credit).HasPrecision(18, 2);

            builder.Entity<TenantSubscriptionRecord>()
                .ToTable("TenantSubscriptions")
                .HasIndex(x => x.TenantId)
                .IsUnique();

            builder.Entity<CommercialLicenseRecord>()
                .ToTable("CommercialLicenses")
                .HasIndex(x => x.LicenseKey)
                .IsUnique();

            builder.Entity<StockBatchDetail>().Property(x => x.UnitCost).HasPrecision(18, 4);
            builder.Entity<StockBatchDetail>().Property(x => x.Quantity).HasPrecision(18, 4);
            builder.Entity<SalesInvoiceItem>().Property(x => x.CogsAmount).HasPrecision(18, 2);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditingAndBaseEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HandleAuditingAndBaseEntities()
        {
            var entries = ChangeTracker.Entries().ToList();
            var userId = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

            foreach (var entry in entries)
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // Handle BaseEntity timestamps
                if (entry.Entity is BaseEntity baseEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedAt = DateTime.UtcNow;
                        baseEntity.CreatedBy = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        baseEntity.LastModifiedAt = DateTime.UtcNow;
                        baseEntity.LastModifiedBy = userId;
                    }
                }

                // Handle Audit Logistics
                var auditEntry = new AuditLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                };

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) continue;
                    
                    string propertyName = property.Metadata.Name;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.EntityId = property.CurrentValue?.ToString() ?? "Unknown";
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                if (oldValues.Count > 0)
                    auditEntry.OldValues = JsonSerializer.Serialize(oldValues);

                if (newValues.Count > 0)
                    auditEntry.NewValues = JsonSerializer.Serialize(newValues);

                AuditLogs.Add(auditEntry);
            }
        }
    }
}
