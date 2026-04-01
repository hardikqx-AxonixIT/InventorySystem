using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class WarehouseBin : BaseEntity
    {
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        [Required]
        [MaxLength(100)]
        public string WarehouseName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Zone { get; set; }

        [MaxLength(50)]
        public string? Aisle { get; set; }

        [MaxLength(50)]
        public string? Shelf { get; set; }

        [Required]
        [MaxLength(100)]
        public string BinCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
