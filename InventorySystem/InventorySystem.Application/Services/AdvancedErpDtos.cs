using System;
using System.Collections.Generic;

namespace InventorySystem.Application.Services
{
    public class DateRangeFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GstrExportFilterDto : DateRangeFilterDto
    {
        public bool DownloadCsv { get; set; } = false;
    }

    public class EInvoiceGenerateDto
    {
        public int SalesInvoiceId { get; set; }
    }

    public class EWayBillGenerateDto
    {
        public int SalesInvoiceId { get; set; }
        public string? VehicleNumber { get; set; }
        public decimal DistanceKm { get; set; } = 100;
    }

    public class JournalVoucherLineCreateDto
    {
        public int LedgerId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Remarks { get; set; }
    }

    public class JournalVoucherCreateDto
    {
        public DateTime? VoucherDate { get; set; }
        public string? Narration { get; set; }
        public string? SourceModule { get; set; }
        public string? SourceDocumentNo { get; set; }
        public List<JournalVoucherLineCreateDto> Lines { get; set; } = new();
    }

    public class QuotationCreateDto
    {
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public string? PlaceOfSupplyState { get; set; }
        public DateTime? ValidUntil { get; set; }
        public List<TransactionLineRequestDto> Items { get; set; } = new();
    }

    public class QuotationUpdateDto
    {
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public string? PlaceOfSupplyState { get; set; }
        public DateTime? ValidUntil { get; set; }
        public List<TransactionLineRequestDto> Items { get; set; } = new();
    }

    public class DeliveryChallanCreateDto
    {
        public int SalesOrderId { get; set; }
        public string? Notes { get; set; }
    }

    public class SalesReturnLineDto
    {
        public int SalesInvoiceItemId { get; set; }
        public int BinId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class SalesReturnCreateDto
    {
        public int SalesInvoiceId { get; set; }
        public string? Reason { get; set; }
        public List<SalesReturnLineDto> Items { get; set; } = new();
    }

    public class TransferRequestCreateDto
    {
        public int ProductId { get; set; }
        public int FromBinId { get; set; }
        public int ToBinId { get; set; }
        public decimal Quantity { get; set; }
        public string? RequestedBy { get; set; }
    }

    public class TransferRequestApprovalDto
    {
        public int RequestId { get; set; }
        public bool Approve { get; set; } = true;
        public string? ApprovedBy { get; set; }
        public string? ApprovalNote { get; set; }
    }

    public class PickListCreateDto
    {
        public int SalesOrderId { get; set; }
    }

    public class PickScanDto
    {
        public int PickListId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
    }

    public class BomTemplateCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? BomCode { get; set; }
        public int FinishedProductId { get; set; }
        public decimal StandardOutputQty { get; set; } = 1;
        public List<BomTemplateItemCreateDto> Items { get; set; } = new();
    }

    public class BomTemplateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal StandardOutputQty { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public List<BomTemplateItemCreateDto> Items { get; set; } = new();
    }

    public class BomTemplateItemCreateDto
    {
        public int ComponentProductId { get; set; }
        public decimal QuantityPerOutput { get; set; }
    }

    public class WorkOrderCreateDto
    {
        public int BomTemplateId { get; set; }
        public DateTime PlannedDate { get; set; } = DateTime.UtcNow.Date;
        public decimal PlannedOutputQty { get; set; } = 1;
        public int InputBinId { get; set; }
        public int OutputBinId { get; set; }
    }

    public class PermissionUpsertDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string ModuleKey { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanExport { get; set; }
    }

    public class AuditLogFilterDto
    {
        public string? EntityName { get; set; }
        public string? Action { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class WhatsAppInvoiceDto
    {
        public int SalesInvoiceId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class RazorpayOrderCreateDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; } = string.Empty;
    }

    public class RazorpayCallbackDto
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
        public string? RawPayload { get; set; }
    }
}
