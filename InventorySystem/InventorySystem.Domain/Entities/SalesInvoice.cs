using InventorySystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class SalesInvoice : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        [MaxLength(100)]
        public string? PlaceOfSupplyState { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DocumentStatus Status { get; set; } = DocumentStatus.Completed;

        public decimal Subtotal { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        // Indian Market specific Transport Details
        [MaxLength(100)]
        public string? TransporterName { get; set; }
        
        [MaxLength(50)]
        public string? LRNumber { get; set; }
        
        [MaxLength(50)]
        public string? VehicleNumber { get; set; }
        
        public DateTime? DispatchDate { get; set; }

        public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    }
}
