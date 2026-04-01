using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class Warehouse : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Code { get; set; }

        [MaxLength(250)]
        public string? AddressLine1 { get; set; }

        [MaxLength(250)]
        public string? AddressLine2 { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(15)]
        public string? Gstin { get; set; }

        [MaxLength(10)]
        public string? Pan { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? BankAccountNo { get; set; }

        [MaxLength(20)]
        public string? IfscCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
