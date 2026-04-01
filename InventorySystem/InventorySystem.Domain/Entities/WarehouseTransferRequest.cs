using InventorySystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class WarehouseTransferRequest : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int FromBinId { get; set; }
        public WarehouseBin? FromBin { get; set; }

        public int ToBinId { get; set; }
        public WarehouseBin? ToBin { get; set; }

        public decimal Quantity { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Open;
        public string? RequestedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovalNote { get; set; }
    }
}
