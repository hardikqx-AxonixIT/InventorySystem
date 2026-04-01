namespace InventorySystem.Domain.Entities
{
    public class DeliveryChallanItem : BaseEntity
    {
        public int DeliveryChallanId { get; set; }
        public DeliveryChallan? DeliveryChallan { get; set; }

        public int SalesOrderItemId { get; set; }
        public SalesOrderItem? SalesOrderItem { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int BinId { get; set; }
        public WarehouseBin? Bin { get; set; }

        public decimal Quantity { get; set; }
    }
}
