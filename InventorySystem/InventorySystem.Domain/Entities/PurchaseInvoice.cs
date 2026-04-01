using InventorySystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class PurchaseInvoice : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int GoodsReceiptNoteId { get; set; }
        public GoodsReceiptNote? GoodsReceiptNote { get; set; }

        public int PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Open;

        public decimal Subtotal { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    }
}
