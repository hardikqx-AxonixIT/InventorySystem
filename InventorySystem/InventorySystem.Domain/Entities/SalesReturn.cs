using InventorySystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class SalesReturn : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string ReturnNumber { get; set; } = string.Empty;

        public int SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public DocumentStatus Status { get; set; } = DocumentStatus.Completed;

        public decimal Subtotal { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
    }
}
