using InventorySystem.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class PickList : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string PickNumber { get; set; } = string.Empty;

        public int SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Open;

        public ICollection<PickListItem> Items { get; set; } = new List<PickListItem>();
    }
}
