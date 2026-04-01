namespace InventorySystem.Domain.Entities
{
    public class GoodsReceiptNoteItem
    {
        public int Id { get; set; }

        public int GoodsReceiptNoteId { get; set; }
        public GoodsReceiptNote? GoodsReceiptNote { get; set; }

        public int PurchaseOrderItemId { get; set; }
        public PurchaseOrderItem? PurchaseOrderItem { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int BinId { get; set; }
        public WarehouseBin? Bin { get; set; }

        public decimal QuantityReceived { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GstRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}
