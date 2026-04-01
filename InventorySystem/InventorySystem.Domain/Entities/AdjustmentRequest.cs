using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public enum AdjustmentStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class AdjustmentRequest : BaseEntity
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public decimal RequestedAmount { get; set; }
        
        [MaxLength(500)]
        public string? Reason { get; set; }

        public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;

        [MaxLength(100)]
        public string? RequestedBy { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
