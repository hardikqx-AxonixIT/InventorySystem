namespace InventorySystem.Domain.Entities
{
    public class SalesReturnItem : BaseEntity
    {
        public int SalesReturnId { get; set; }
        public SalesReturn? SalesReturn { get; set; }

        public int SalesInvoiceItemId { get; set; }
        public SalesInvoiceItem? SalesInvoiceItem { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int BinId { get; set; }
        public WarehouseBin? Bin { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GstRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}
