using InventorySystem.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class ProductionWorkOrder : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string WorkOrderNumber { get; set; } = string.Empty;

        public int BomTemplateId { get; set; }
        public BomTemplate? BomTemplate { get; set; }
        public DateTime PlannedDate { get; set; } = DateTime.UtcNow.Date;
        public decimal PlannedOutputQty { get; set; }
        public int OutputBinId { get; set; }
        public WarehouseBin? OutputBin { get; set; }
        public int InputBinId { get; set; }
        public WarehouseBin? InputBin { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Open;
    }
}
