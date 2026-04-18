using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class StockBatchDetail : BaseEntity
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int BinId { get; set; }
        public WarehouseBin? Bin { get; set; }

        [MaxLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        public decimal Quantity { get; set; }

        /// <summary>Pre-tax unit cost for FIFO/COGS (typically matches PO unit price).</summary>
        public decimal UnitCost { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
