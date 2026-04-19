using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class NotificationLog : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; } = "WhatsApp"; // WhatsApp, SMS, Email

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        [Required]
        [MaxLength(20)]
        public string RecipientNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string MessageContent { get; set; } = string.Empty;

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Sent"; // Sent, Failed, Delivered, Read

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
    }
}
