using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public interface ITransactionService
    {
        Task<object> GetBootstrapAsync(CancellationToken cancellationToken = default);
        Task<object> GetPurchaseOrdersPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
        Task<object> GetGoodsReceiptsPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
        Task<object> GetPurchaseInvoicesPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
        Task<object> GetSalesOrdersPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
        Task<object> GetSalesInvoicesPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default);
        Task<object> CreatePurchaseOrderAsync(PurchaseOrderCreateDto request, CancellationToken cancellationToken = default);
        Task<object> UpdatePurchaseOrderAsync(int purchaseOrderId, PurchaseOrderUpdateDto request, CancellationToken cancellationToken = default);
        Task<object> CancelPurchaseOrderAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
        Task<object> ReceiveGoodsAsync(GoodsReceiptCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreatePurchaseInvoiceAsync(PurchaseInvoiceCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateSupplierPaymentAsync(SupplierPaymentCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateStockInAsync(StockMovementCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateStockOutAsync(StockMovementCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateStockTransferAsync(StockTransferCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateProductionRunAsync(ProductionRunCreateDto request, CancellationToken cancellationToken = default);
        Task<object> GetReportsSnapshotAsync(CancellationToken cancellationToken = default);
        Task<object> GetMobileDashboardAsync(CancellationToken cancellationToken = default);
        Task<object> GetDemandPredictionAsync(int lookbackDays = 30, CancellationToken cancellationToken = default);
        Task<object> GetAutoReorderSuggestionsAsync(int lookbackDays = 30, int horizonDays = 15, CancellationToken cancellationToken = default);
        Task<object> GetAccountingSummaryAsync(CancellationToken cancellationToken = default);
        Task<object> CreateSalesOrderAsync(SalesOrderCreateDto request, CancellationToken cancellationToken = default);
        Task<object> UpdateSalesOrderAsync(int salesOrderId, SalesOrderUpdateDto request, CancellationToken cancellationToken = default);
        Task<object> CancelSalesOrderAsync(int salesOrderId, CancellationToken cancellationToken = default);
        Task<object> CreateSalesInvoiceAsync(SalesInvoiceCreateDto request, CancellationToken cancellationToken = default);
        Task<object> GetStockHistoryAsync(int productId, CancellationToken cancellationToken = default);
        Task<object> GetExpiryAlertsAsync(int days = 30, CancellationToken cancellationToken = default);
        Task<object> ScanBatchToOrderAsync(int salesOrderId, string batchOrSerial, CancellationToken cancellationToken = default);
    }
}
