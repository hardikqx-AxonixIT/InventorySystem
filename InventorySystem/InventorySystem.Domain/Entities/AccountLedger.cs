using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class AccountLedger : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string GroupType { get; set; } = string.Empty; // Asset/Liability/Income/Expense

        public bool IsSystem { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
