using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class SupplierPayment : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string PaymentNumber { get; set; } = string.Empty;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }

        [MaxLength(40)]
        public string? PaymentMode { get; set; }

        [MaxLength(100)]
        public string? ReferenceNo { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}
