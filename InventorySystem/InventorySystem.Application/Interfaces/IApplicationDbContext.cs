using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; }
        DbSet<ItemCategory> ItemCategories { get; }
        DbSet<UnitOfMeasure> UnitsOfMeasure { get; }
        DbSet<Warehouse> Warehouses { get; }
        DbSet<WarehouseBin> WarehouseBins { get; }
        DbSet<StockLevel> StockLevels { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<StockLedger> StockLedgers { get; }
        DbSet<StockBatchDetail> StockBatchDetails { get; }
        DbSet<AdjustmentRequest> AdjustmentRequests { get; }
        DbSet<Customer> Customers { get; }
        DbSet<Vendor> Vendors { get; }
        DbSet<PurchaseOrder> PurchaseOrders { get; }
        DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
        DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; }
        DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; }
        DbSet<PurchaseInvoice> PurchaseInvoices { get; }
        DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }
        DbSet<SupplierPayment> SupplierPayments { get; }
        DbSet<SalesOrder> SalesOrders { get; }
        DbSet<SalesOrderItem> SalesOrderItems { get; }
        DbSet<SalesInvoice> SalesInvoices { get; }
        DbSet<SalesInvoiceItem> SalesInvoiceItems { get; }
        DbSet<SalesQuotation> SalesQuotations { get; }
        DbSet<SalesQuotationItem> SalesQuotationItems { get; }
        DbSet<DeliveryChallan> DeliveryChallans { get; }
        DbSet<DeliveryChallanItem> DeliveryChallanItems { get; }
        DbSet<SalesReturn> SalesReturns { get; }
        DbSet<SalesReturnItem> SalesReturnItems { get; }
        DbSet<AccountLedger> AccountLedgers { get; }
        DbSet<JournalVoucher> JournalVouchers { get; }
        DbSet<JournalVoucherLine> JournalVoucherLines { get; }
        DbSet<WarehouseTransferRequest> WarehouseTransferRequests { get; }
        DbSet<PickList> PickLists { get; }
        DbSet<PickListItem> PickListItems { get; }
        DbSet<BomTemplate> BomTemplates { get; }
        DbSet<BomTemplateItem> BomTemplateItems { get; }
        DbSet<ProductionWorkOrder> ProductionWorkOrders { get; }
        DbSet<RolePermission> RolePermissions { get; }
        DbSet<GstEInvoiceRecord> GstEInvoiceRecords { get; }
        DbSet<GstEWayBillRecord> GstEWayBillRecords { get; }
        DbSet<PaymentGatewayCallbackLog> PaymentGatewayCallbackLogs { get; }

        DatabaseFacade Database { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
