namespace InventorySystem.Domain.Entities
{
    public class SalesInvoiceItem
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public int SalesOrderItemId { get; set; }
        public SalesOrderItem? SalesOrderItem { get; set; }

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

        /// <summary>COGS for this line (FIFO, pre-tax) captured at invoice time.</summary>
        public decimal CogsAmount { get; set; }
    }
}
