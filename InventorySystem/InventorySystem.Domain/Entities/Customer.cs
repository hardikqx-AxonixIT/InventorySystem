using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class Customer : BaseEntity
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

        [MaxLength(12)]
        public string? AadharNumber { get; set; }

        [MaxLength(100)]
        public string? UpiId { get; set; }

        [MaxLength(250)]
        public string? BillingAddress { get; set; }

        [MaxLength(100)]
        public string? BillingCity { get; set; }

        [MaxLength(100)]
        public string? BillingState { get; set; }

        [MaxLength(20)]
        public string? BillingPostalCode { get; set; }

        [MaxLength(250)]
        public string? ShippingAddress { get; set; }

        public int PaymentTermsDays { get; set; }
        public decimal CreditLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
