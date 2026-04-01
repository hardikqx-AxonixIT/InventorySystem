using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class BomTemplate : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string BomCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public int FinishedProductId { get; set; }
        public Product? FinishedProduct { get; set; }
        public decimal StandardOutputQty { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public ICollection<BomTemplateItem> Items { get; set; } = new List<BomTemplateItem>();
    }
}
