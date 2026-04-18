using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class TenantUpiConfiguration : BaseEntity
    {
        public int TenantId { get; set; } // Assuming multi-tenancy or single business profile
        
        [Required]
        [MaxLength(100)]
        public string UpiId { get; set; } = string.Empty; // Merchant VPA

        [Required]
        [MaxLength(200)]
        public string MerchantName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
