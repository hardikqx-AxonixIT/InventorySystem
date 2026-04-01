using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public interface IAdvancedErpService
    {
        Task<object> GetGstr1Async(GstrExportFilterDto request, CancellationToken cancellationToken = default);
        Task<object> GetGstr3bAsync(GstrExportFilterDto request, CancellationToken cancellationToken = default);
        Task<object> GenerateEInvoiceAsync(EInvoiceGenerateDto request, CancellationToken cancellationToken = default);
        Task<object> GenerateEWayBillAsync(EWayBillGenerateDto request, CancellationToken cancellationToken = default);
        Task<object> GetAccountingLedgersAsync(CancellationToken cancellationToken = default);
        Task<object> PostJournalVoucherAsync(JournalVoucherCreateDto request, CancellationToken cancellationToken = default);
        Task<object> GetLedgerDrilldownAsync(int ledgerId, DateRangeFilterDto request, CancellationToken cancellationToken = default);
        Task<object> GetProfitAndLossAsync(DateRangeFilterDto request, CancellationToken cancellationToken = default);
        Task<object> GetBalanceSheetAsync(DateTime? asOfDate, CancellationToken cancellationToken = default);
        Task<object> CreateQuotationAsync(QuotationCreateDto request, CancellationToken cancellationToken = default);
        Task<object> UpdateQuotationAsync(int quotationId, QuotationUpdateDto request, CancellationToken cancellationToken = default);
        Task<object> CancelQuotationAsync(int quotationId, CancellationToken cancellationToken = default);
        Task<object> ConvertQuotationToOrderAsync(int quotationId, CancellationToken cancellationToken = default);
        Task<object> CreateDeliveryChallanAsync(DeliveryChallanCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateSalesReturnAsync(SalesReturnCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateTransferRequestAsync(TransferRequestCreateDto request, CancellationToken cancellationToken = default);
        Task<object> ApproveTransferRequestAsync(TransferRequestApprovalDto request, CancellationToken cancellationToken = default);
        Task<object> CreatePickListAsync(PickListCreateDto request, CancellationToken cancellationToken = default);
        Task<object> ScanPickItemAsync(PickScanDto request, CancellationToken cancellationToken = default);
        Task<object> MarkPickListPackedAsync(int pickListId, CancellationToken cancellationToken = default);
        Task<object> CreateBomTemplateAsync(BomTemplateCreateDto request, CancellationToken cancellationToken = default);
        Task<object> UpdateBomTemplateAsync(int bomTemplateId, BomTemplateUpdateDto request, CancellationToken cancellationToken = default);
        Task<object> CreateWorkOrderAsync(WorkOrderCreateDto request, CancellationToken cancellationToken = default);
        Task<object> CancelWorkOrderAsync(int workOrderId, CancellationToken cancellationToken = default);
        Task<object> ReleaseWorkOrderAsync(int workOrderId, CancellationToken cancellationToken = default);
        Task<object> GetRolePermissionMatrixAsync(CancellationToken cancellationToken = default);
        Task<object> UpsertRolePermissionAsync(PermissionUpsertDto request, CancellationToken cancellationToken = default);
        Task<object> GetAuditLogsAsync(AuditLogFilterDto request, CancellationToken cancellationToken = default);
        Task<object> SendWhatsAppInvoiceAsync(WhatsAppInvoiceDto request, CancellationToken cancellationToken = default);
        Task<object> CreateRazorpayOrderAsync(RazorpayOrderCreateDto request, CancellationToken cancellationToken = default);
        Task<object> VerifyRazorpayCallbackAsync(RazorpayCallbackDto request, CancellationToken cancellationToken = default);
        Task<object> GetAdvancedSnapshotAsync(CancellationToken cancellationToken = default);
    }
}
