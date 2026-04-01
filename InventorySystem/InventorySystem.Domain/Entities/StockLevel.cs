using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class StockLevel
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int BinId { get; set; }
        public WarehouseBin? Bin { get; set; }

        public decimal QuantityOnHand { get; set; }
        public decimal ReservedQuantity { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
