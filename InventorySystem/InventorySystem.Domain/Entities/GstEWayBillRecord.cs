using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class GstEWayBillRecord : BaseEntity
    {
        public int SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        [Required]
        [MaxLength(20)]
        public string EWayBillNumber { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? VehicleNumber { get; set; }

        public decimal DistanceKm { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
