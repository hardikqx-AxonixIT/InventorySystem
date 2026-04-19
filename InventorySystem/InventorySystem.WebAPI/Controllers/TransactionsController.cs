using InventorySystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("bootstrap")]
        public async Task<IActionResult> GetBootstrap(CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.GetBootstrapAsync(cancellationToken));
        }

        [HttpGet("purchase-orders")]
        public async Task<IActionResult> GetPurchaseOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _transactionService.GetPurchaseOrdersPagedAsync(page, pageSize, cancellationToken));
        }

        [HttpGet("goods-receipts")]
        public async Task<IActionResult> GetGoodsReceipts([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _transactionService.GetGoodsReceiptsPagedAsync(page, pageSize, cancellationToken));
        }

        [HttpGet("purchase-invoices")]
        public async Task<IActionResult> GetPurchaseInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _transactionService.GetPurchaseInvoicesPagedAsync(page, pageSize, cancellationToken));
        }

        [HttpGet("sales-orders")]
        public async Task<IActionResult> GetSalesOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _transactionService.GetSalesOrdersPagedAsync(page, pageSize, cancellationToken));
        }

        [HttpGet("sales-invoices")]
        public async Task<IActionResult> GetSalesInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _transactionService.GetSalesInvoicesPagedAsync(page, pageSize, cancellationToken));
        }

        [HttpPost("purchase-orders")]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreatePurchaseOrderAsync(request, cancellationToken));
        }

        [HttpPut("purchase-orders/{purchaseOrderId:int}")]
        public async Task<IActionResult> UpdatePurchaseOrder(int purchaseOrderId, [FromBody] PurchaseOrderUpdateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.UpdatePurchaseOrderAsync(purchaseOrderId, request, cancellationToken));
        }

        [HttpPost("purchase-orders/{purchaseOrderId:int}/cancel")]
        public async Task<IActionResult> CancelPurchaseOrder(int purchaseOrderId, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CancelPurchaseOrderAsync(purchaseOrderId, cancellationToken));
        }

        [HttpPost("goods-receipts")]
        public async Task<IActionResult> CreateGoodsReceipt([FromBody] GoodsReceiptCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.ReceiveGoodsAsync(request, cancellationToken));
        }

        [HttpPost("purchase-invoices")]
        public async Task<IActionResult> CreatePurchaseInvoice([FromBody] PurchaseInvoiceCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreatePurchaseInvoiceAsync(request, cancellationToken));
        }

        [HttpPost("supplier-payments")]
        public async Task<IActionResult> CreateSupplierPayment([FromBody] SupplierPaymentCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreateSupplierPaymentAsync(request, cancellationToken));
        }

        [HttpPost("inventory/stock-in")]
        public async Task<IActionResult> CreateStockIn([FromBody] StockMovementCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreateStockInAsync(request, cancellationToken));
        }

        [HttpPost("inventory/stock-out")]
        public async Task<IActionResult> CreateStockOut([FromBody] StockMovementCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreateStockOutAsync(request, cancellationToken));
        }

        [HttpPost("inventory/stock-transfer")]
        public async Task<IActionResult> CreateStockTransfer([FromBody] StockTransferCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreateStockTransferAsync(request, cancellationToken));
        }

        [HttpPost("manufacturing/production-runs")]
        public async Task<IActionResult> CreateProductionRun([FromBody] ProductionRunCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreateProductionRunAsync(request, cancellationToken));
        }

        [HttpGet("reports/snapshot")]
        public async Task<IActionResult> GetReportsSnapshot(CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.GetReportsSnapshotAsync(cancellationToken));
        }

        [HttpGet("mobile/dashboard")]
        public async Task<IActionResult> GetMobileDashboard(CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.GetMobileDashboardAsync(cancellationToken));
        }

        [HttpGet("ai/demand-prediction")]
        public async Task<IActionResult> GetDemandPrediction([FromQuery] int lookbackDays = 30, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _transactionService.GetDemandPredictionAsync(lookbackDays, cancellationToken));
        }

        [HttpGet("auto-reorder/suggestions")]
        public async Task<IActionResult> GetAutoReorderSuggestions([FromQuery] int lookbackDays = 30, [FromQuery] int horizonDays = 15, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _transactionService.GetAutoReorderSuggestionsAsync(lookbackDays, horizonDays, cancellationToken));
        }

        [HttpGet("accounting-summary")]
        public async Task<IActionResult> GetAccountingSummary(CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.GetAccountingSummaryAsync(cancellationToken));
        }

        [HttpPost("sales-orders")]
        public async Task<IActionResult> CreateSalesOrder([FromBody] SalesOrderCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreateSalesOrderAsync(request, cancellationToken));
        }

        [HttpPut("sales-orders/{salesOrderId:int}")]
        public async Task<IActionResult> UpdateSalesOrder(int salesOrderId, [FromBody] SalesOrderUpdateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.UpdateSalesOrderAsync(salesOrderId, request, cancellationToken));
        }

        [HttpPost("sales-orders/{salesOrderId:int}/cancel")]
        public async Task<IActionResult> CancelSalesOrder(int salesOrderId, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CancelSalesOrderAsync(salesOrderId, cancellationToken));
        }

        [HttpPost("sales-invoices")]
        public async Task<IActionResult> CreateSalesInvoice([FromBody] SalesInvoiceCreateDto request, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.CreateSalesInvoiceAsync(request, cancellationToken));
        }

        [HttpGet("stock-history/{productId:int}")]
        public async Task<IActionResult> GetStockHistory(int productId, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _transactionService.GetStockHistoryAsync(productId, cancellationToken));
        }

        [HttpGet("inventory/expiry-alerts")]
        public Task<IActionResult> GetExpiryAlerts([FromQuery] int days = 30, CancellationToken ct = default) => ExecuteAsync(() => _transactionService.GetExpiryAlertsAsync(days, ct));

        [HttpPost("sales/orders/{orderId:int}/scan/{batchNumber}")]
        public Task<IActionResult> ScanBatchToOrder(int orderId, string batchNumber, CancellationToken ct = default) => ExecuteAsync(() => _transactionService.ScanBatchToOrderAsync(orderId, batchNumber, ct));

        private async Task<IActionResult> ExecuteAsync(Func<Task<object>> operation)
        {
            try
            {
                return Ok(await operation());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}


