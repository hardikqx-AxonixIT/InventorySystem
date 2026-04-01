using InventorySystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class PurchaseOrder : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        [MaxLength(100)]
        public string? SupplyState { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DocumentStatus Status { get; set; } = DocumentStatus.Open;

        public decimal Subtotal { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
