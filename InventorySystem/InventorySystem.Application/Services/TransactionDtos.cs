using System;
using System.Collections.Generic;

namespace InventorySystem.Application.Services
{
    public class TaxBreakdownDto
    {
        public decimal TaxableAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class TransactionLineRequestDto
    {
        public int ProductId { get; set; }
        public int BinId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GstRate { get; set; }
    }

    public class PurchaseOrderCreateDto
    {
        public int VendorId { get; set; }
        public int WarehouseId { get; set; }
        public string? SupplyState { get; set; }
        public List<TransactionLineRequestDto> Items { get; set; } = new();
    }

    public class PurchaseOrderUpdateDto
    {
        public int VendorId { get; set; }
        public int WarehouseId { get; set; }
        public string? SupplyState { get; set; }
        public List<TransactionLineRequestDto> Items { get; set; } = new();
    }

    public class GrnReceiveLineDto
    {
        public int PurchaseOrderItemId { get; set; }
        public int ProductId { get; set; }
        public int BinId { get; set; }
        public decimal QuantityReceived { get; set; }
    }

    public class GoodsReceiptCreateDto
    {
        public int PurchaseOrderId { get; set; }
        public List<GrnReceiveLineDto> Items { get; set; } = new();
    }

    public class PurchaseInvoiceCreateDto
    {
        public int GoodsReceiptNoteId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class SupplierPaymentCreateDto
    {
        public int PurchaseInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMode { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
    }

    public class StockMovementCreateDto
    {
        public int ProductId { get; set; }
        public int BinId { get; set; }
        public decimal Quantity { get; set; }
        public string? ReferenceNo { get; set; }
    }

    public class StockTransferCreateDto
    {
        public int ProductId { get; set; }
        public int FromBinId { get; set; }
        public int ToBinId { get; set; }
        public decimal Quantity { get; set; }
        public string? ReferenceNo { get; set; }
    }

    public class ProductionComponentDto
    {
        public int ProductId { get; set; }
        public int BinId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ProductionRunCreateDto
    {
        public int FinishedProductId { get; set; }
        public int OutputBinId { get; set; }
        public decimal OutputQuantity { get; set; }
        public string? ReferenceNo { get; set; }
        public List<ProductionComponentDto> Components { get; set; } = new();
    }

    public class SalesOrderCreateDto
    {
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public string? PlaceOfSupplyState { get; set; }
        public List<TransactionLineRequestDto> Items { get; set; } = new();
    }

    public class SalesOrderUpdateDto
    {
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public string? PlaceOfSupplyState { get; set; }
        public List<TransactionLineRequestDto> Items { get; set; } = new();
    }

    public class SalesInvoiceCreateDto
    {
        public int SalesOrderId { get; set; }
    }

    public class TransactionBootstrapDto
    {
        public object Products { get; set; } = new();
        public object Customers { get; set; } = new();
        public object Vendors { get; set; } = new();
        public object Warehouses { get; set; } = new();
        public object Bins { get; set; } = new();
        public object StockLevels { get; set; } = new();
        public object PurchaseOrders { get; set; } = new();
        public object GoodsReceiptNotes { get; set; } = new();
        public object PurchaseInvoices { get; set; } = new();
        public object SupplierPayments { get; set; } = new();
        public object SalesOrders { get; set; } = new();
        public object SalesInvoices { get; set; } = new();
        public GstSummaryDto GstSummary { get; set; } = new();
        public object AccountingSummary { get; set; } = new();
    }

    public class GstSummaryDto
    {
        public decimal PurchaseCgst { get; set; }
        public decimal PurchaseSgst { get; set; }
        public decimal PurchaseIgst { get; set; }
        public decimal SalesCgst { get; set; }
        public decimal SalesSgst { get; set; }
        public decimal SalesIgst { get; set; }
        public decimal NetGstPayable { get; set; }
    }
}
