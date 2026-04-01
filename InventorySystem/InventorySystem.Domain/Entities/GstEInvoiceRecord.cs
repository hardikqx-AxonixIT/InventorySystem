using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class GstEInvoiceRecord : BaseEntity
    {
        public int SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        [Required]
        [MaxLength(120)]
        public string Irn { get; set; } = string.Empty;

        [MaxLength(50)]
        public string AckNo { get; set; } = string.Empty;

        public DateTime AckDate { get; set; } = DateTime.UtcNow;
        public string? SignedInvoice { get; set; }
    }
}
