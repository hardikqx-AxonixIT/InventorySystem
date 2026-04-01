using InventorySystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class SalesQuotation : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string QuotationNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ValidUntil { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Open;

        public decimal Subtotal { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }

        public ICollection<SalesQuotationItem> Items { get; set; } = new List<SalesQuotationItem>();
    }
}
