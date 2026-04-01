namespace InventorySystem.Domain.Entities
{
    public class PickListItem : BaseEntity
    {
        public int PickListId { get; set; }
        public PickList? PickList { get; set; }

        public int SalesOrderItemId { get; set; }
        public SalesOrderItem? SalesOrderItem { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int BinId { get; set; }
        public WarehouseBin? Bin { get; set; }

        public decimal Quantity { get; set; }
        public decimal PickedQuantity { get; set; }
    }
}
