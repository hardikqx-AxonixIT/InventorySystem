using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class CommercialLicenseRecord
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string LicenseKey { get; set; } = string.Empty;

        public string TenantId { get; set; } = string.Empty;

        public DateTime IssuedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
