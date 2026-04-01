using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class Vendor : BaseEntity
    {
        [Required]
        [MaxLength(180)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(15)]
        public string? Gstin { get; set; }

        [MaxLength(10)]
        public string? Pan { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public int PaymentTermsDays { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
