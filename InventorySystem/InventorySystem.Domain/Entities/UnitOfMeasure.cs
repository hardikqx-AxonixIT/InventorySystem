using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class UnitOfMeasure : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Symbol { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
