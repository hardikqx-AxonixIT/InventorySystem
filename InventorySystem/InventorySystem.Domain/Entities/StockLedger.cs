using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    // Append-Only Ledger
    public class StockLedger
    {
        [Key]
        public long LedgerId { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public decimal ChangeAmount { get; set; }
        public decimal PostChangeBalance { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty; 

        [MaxLength(100)]
        public string? ReferenceDocumentId { get; set; } // e.g. Invoice #, PO #

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
