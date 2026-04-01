using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class ItemCategory : BaseEntity
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
