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
    public class AdvancedOperationsController : ControllerBase
    {
        private readonly IAdvancedErpService _service;

        public AdvancedOperationsController(IAdvancedErpService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet("snapshot")]
        public Task<IActionResult> GetSnapshot(CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetAdvancedSnapshotAsync(cancellationToken));

        [AllowAnonymous]
        [HttpPost("gst/gstr1")]
        public Task<IActionResult> GetGstr1([FromBody] GstrExportFilterDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetGstr1Async(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("gst/gstr3b")]
        public Task<IActionResult> GetGstr3b([FromBody] GstrExportFilterDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetGstr3bAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("gst/einvoice")]
        public Task<IActionResult> GenerateEInvoice([FromBody] EInvoiceGenerateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GenerateEInvoiceAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("gst/eway-bill")]
        public Task<IActionResult> GenerateEWayBill([FromBody] EWayBillGenerateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GenerateEWayBillAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpGet("accounting/ledgers")]
        public Task<IActionResult> GetLedgers(CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetAccountingLedgersAsync(cancellationToken));

        [AllowAnonymous]
        [HttpPost("accounting/journal-vouchers")]
        public Task<IActionResult> PostVoucher([FromBody] JournalVoucherCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.PostJournalVoucherAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("accounting/ledger/{ledgerId:int}")]
        public Task<IActionResult> GetLedger(int ledgerId, [FromBody] DateRangeFilterDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetLedgerDrilldownAsync(ledgerId, request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("accounting/profit-loss")]
        public Task<IActionResult> GetProfitLoss([FromBody] DateRangeFilterDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetProfitAndLossAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpGet("accounting/balance-sheet")]
        public Task<IActionResult> GetBalanceSheet([FromQuery] DateTime? asOfDate, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetBalanceSheetAsync(asOfDate, cancellationToken));

        [AllowAnonymous]
        [HttpPost("sales/quotations")]
        public Task<IActionResult> CreateQuotation([FromBody] QuotationCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreateQuotationAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPut("sales/quotations/{quotationId:int}")]
        public Task<IActionResult> UpdateQuotation(int quotationId, [FromBody] QuotationUpdateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.UpdateQuotationAsync(quotationId, request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("sales/quotations/{quotationId:int}/cancel")]
        public Task<IActionResult> CancelQuotation(int quotationId, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CancelQuotationAsync(quotationId, cancellationToken));

        [AllowAnonymous]
        [HttpPost("sales/quotations/{quotationId:int}/convert")]
        public Task<IActionResult> ConvertQuotation(int quotationId, CancellationToken cancellationToken) => ExecuteAsync(() => _service.ConvertQuotationToOrderAsync(quotationId, cancellationToken));

        [AllowAnonymous]
        [HttpPost("sales/delivery-challans")]
        public Task<IActionResult> CreateDeliveryChallan([FromBody] DeliveryChallanCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreateDeliveryChallanAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("sales/returns")]
        public Task<IActionResult> CreateSalesReturn([FromBody] SalesReturnCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreateSalesReturnAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("warehouse/transfer-requests")]
        public Task<IActionResult> CreateTransferRequest([FromBody] TransferRequestCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreateTransferRequestAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("warehouse/transfer-requests/approve")]
        public Task<IActionResult> ApproveTransferRequest([FromBody] TransferRequestApprovalDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.ApproveTransferRequestAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("warehouse/pick-lists")]
        public Task<IActionResult> CreatePickList([FromBody] PickListCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreatePickListAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("warehouse/pick-lists/scan")]
        public Task<IActionResult> ScanPick([FromBody] PickScanDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.ScanPickItemAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("warehouse/pick-lists/{pickListId:int}/pack")]
        public Task<IActionResult> PackPickList(int pickListId, CancellationToken cancellationToken) => ExecuteAsync(() => _service.MarkPickListPackedAsync(pickListId, cancellationToken));

        [AllowAnonymous]
        [HttpPost("manufacturing/boms")]
        public Task<IActionResult> CreateBom([FromBody] BomTemplateCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreateBomTemplateAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPut("manufacturing/boms/{bomTemplateId:int}")]
        public Task<IActionResult> UpdateBom(int bomTemplateId, [FromBody] BomTemplateUpdateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.UpdateBomTemplateAsync(bomTemplateId, request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("manufacturing/work-orders")]
        public Task<IActionResult> CreateWorkOrder([FromBody] WorkOrderCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreateWorkOrderAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("manufacturing/work-orders/{workOrderId:int}/cancel")]
        public Task<IActionResult> CancelWorkOrder(int workOrderId, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CancelWorkOrderAsync(workOrderId, cancellationToken));

        [AllowAnonymous]
        [HttpPost("manufacturing/work-orders/{workOrderId:int}/release")]
        public Task<IActionResult> ReleaseWorkOrder(int workOrderId, CancellationToken cancellationToken) => ExecuteAsync(() => _service.ReleaseWorkOrderAsync(workOrderId, cancellationToken));

        [AllowAnonymous]
        [HttpGet("users/permissions")]
        public Task<IActionResult> GetPermissions(CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetRolePermissionMatrixAsync(cancellationToken));

        [AllowAnonymous]
        [HttpPost("users/permissions")]
        public Task<IActionResult> UpsertPermission([FromBody] PermissionUpsertDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.UpsertRolePermissionAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("users/audit-logs")]
        public Task<IActionResult> GetAuditLogs([FromBody] AuditLogFilterDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.GetAuditLogsAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("integrations/whatsapp/invoice")]
        public Task<IActionResult> SendWhatsApp([FromBody] WhatsAppInvoiceDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.SendWhatsAppInvoiceAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("integrations/razorpay/orders")]
        public Task<IActionResult> CreateRazorpayOrder([FromBody] RazorpayOrderCreateDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.CreateRazorpayOrderAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("integrations/razorpay/callback")]
        public Task<IActionResult> VerifyRazorpayCallback([FromBody] RazorpayCallbackDto request, CancellationToken cancellationToken) => ExecuteAsync(() => _service.VerifyRazorpayCallbackAsync(request, cancellationToken));

        private async Task<IActionResult> ExecuteAsync(Func<Task<object>> operation)
        {
            try { return Ok(await operation()); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
