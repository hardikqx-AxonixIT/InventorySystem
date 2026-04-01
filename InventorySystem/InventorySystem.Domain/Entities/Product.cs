using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Barcode { get; set; }

        [MaxLength(20)]
        public string? HsnCode { get; set; }

        [MaxLength(120)]
        public string? Brand { get; set; }

        public int UOMId { get; set; }
        public UnitOfMeasure? UOM { get; set; }

        public int CategoryId { get; set; }
        public ItemCategory? Category { get; set; }

        public decimal ReorderLevel { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal GstRate { get; set; }
        public bool TrackBatch { get; set; }
        public bool TrackSerial { get; set; }
        public bool TrackExpiry { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
