using InventorySystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class DeliveryChallan : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string ChallanNumber { get; set; } = string.Empty;

        public int SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public DateTime ChallanDate { get; set; } = DateTime.UtcNow;
        public DocumentStatus Status { get; set; } = DocumentStatus.Open;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public ICollection<DeliveryChallanItem> Items { get; set; } = new List<DeliveryChallanItem>();
    }
}
